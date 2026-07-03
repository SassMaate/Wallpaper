using MathNet.Numerics.IntegralTransforms;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using NAudio.Wave;
using System.Diagnostics;
using System.Numerics;

namespace Sucrose.Backgroundog.Extension
{
    internal class AudioVisualizer : IMMNotificationClient
    {
        /// <summary>
        /// One spectrum snapshot: Mono (back-compat), plus per-channel Left/Right.
        /// Each is 128 values normalised to [0, 1].
        /// </summary>
        public sealed class Spectrum
        {
            public double[] Mono;
            public double[] Left;
            public double[] Right;
        }

        public event EventHandler<Spectrum> AudioDataAvailable;

        // Output contract: 128 bins, each normalised to [0, 1] (wallpapers index Data[0..127]).
        private const int MaxSample = 128;

        // Fixed power-of-two FFT size (decoupled from the WASAPI callback buffer size,
        // which used to vary and shift the bin -> frequency mapping per callback).
        private const int FftSize = 4096;
        private const int HalfFft = FftSize / 2;

        // ---- Tunables ------------------------------------------------------
        // Frequency range mapped across the 128 output bins, on a logarithmic
        // (octave/perceptual) scale instead of the old "first 128 linear bins".
        // This raises the visualised ceiling from ~a couple of kHz to the full audible range.
        private const double MinFrequency = 20.0;
        private const double MaxFrequency = 16000.0;

        // Output is LINEAR magnitude (not dBFS): magnitude / full-scale * Gain, clamped to
        // [0, 1]. Linear preserves the natural dynamic range — bass towering over treble —
        // so spectrum visuals keep their punchy, peaked shape. (dBFS log-compressed that
        // into a flat, lifeless band.) Gain pushes typical loud content up toward full scale.
        private const double LinearGain = 2.5;

        // Subtracted before clamping to gate residual hiss so silence / paused audio stays flat.
        private const double NoiseFloor = 0.01;

        // High-frequency tilt (dB/octave), applied per band, anchored at MinFrequency.
        // Music rolls off (~-4.5 dB/oct), so without compensation the upper half of the
        // 16 kHz log spectrum sits near 0 and the right side of every visualiser looks dead.
        // This boost makes the full spectrum register (like a spectrum analyser's "slope"),
        // without flattening the bass-vs-treble dynamics the way a gamma curve would. 0 = off.
        private const double TiltDbPerOctave = 2.0;

        // Asymmetric (attack/decay) smoothing applied per bin, per emitted frame:
        // fast rise so bars snap to the beat, slow fall so they ease back down.
        // Tuned for the throttled ~EmitIntervalMs cadence below.
        private const double AttackSpeed = 0.35;
        private const double ReleaseSpeed = 0.06;

        // Neighbour-bin averaging to take the edge off jagged bars.
        private const int HorizontalSmoothness = 1;

        // FFT/emit throttle: the ring is fed on every WASAPI callback (cheap), but the
        // FFT + event only run this often. WASAPI fires far faster than any wallpaper
        // can render, so this caps CPU without losing visual smoothness.
        private const int EmitIntervalMs = 16; // ~60 Hz
        // --------------------------------------------------------------------

        private int SampleRate;
        private int[] BandStart;
        private int[] BandEnd;
        private double[] TiltGain;
        private double[] Window;
        private double Reference; // bin magnitude of a full-scale tone (the 0 dBFS anchor)
        private bool RingFilled;
        private int RingWritePos;
        private long LastEmitMs; // 0; first frame emits once the ring fills (~85 ms)
        private readonly Stopwatch Clock = Stopwatch.StartNew();

        // Pre-allocated and reused every update to avoid per-callback GC churn.
        private readonly float[] LeftRing = new float[FftSize];
        private readonly float[] RightRing = new float[FftSize];
        private readonly Complex[] ValuesLeft = new Complex[FftSize];
        private readonly Complex[] ValuesRight = new Complex[FftSize];
        private readonly double[] BandsMono = new double[MaxSample];
        private readonly double[] BandsLeft = new double[MaxSample];
        private readonly double[] BandsRight = new double[MaxSample];
        private readonly double[] EnvelopeMono = new double[MaxSample];
        private readonly double[] EnvelopeLeft = new double[MaxSample];
        private readonly double[] EnvelopeRight = new double[MaxSample];

        private WasapiLoopbackCapture Capture;
        private readonly MMDeviceEnumerator DeviceEnum = new();

        public AudioVisualizer()
        {
            try
            {
                BuildWindow();

                int HRESULT = DeviceEnum.RegisterEndpointNotificationCallback(this);

                if (HRESULT != 0)
                {
                    Debug.WriteLine("Failed to register audio device notifications.");
                }

                Capture = CreateWasapiLoopbackCapture();
            }
            catch (Exception Exception)
            {
                Debug.WriteLine($"Failed to initialize audio visualizer: {Exception.Message}");
            }
        }

        public void Start()
        {
            Capture?.StartRecording();
        }

        public void Stop()
        {
            Capture?.StopRecording();
        }

        private WasapiLoopbackCapture CreateWasapiLoopbackCapture(MMDevice Device = null)
        {
            WasapiLoopbackCapture TempCapture = Device != null ? new WasapiLoopbackCapture(Device) : new WasapiLoopbackCapture();

            TempCapture.DataAvailable += ProcessAudioData;

            TempCapture.RecordingStopped += (s, a) =>
            {
                TempCapture.DataAvailable -= ProcessAudioData;

                TempCapture?.Dispose();
            };

            return TempCapture;
        }

        private void ProcessAudioData(object sender, WaveInEventArgs e)
        {
            try
            {
                WaveFormat Format = Capture?.WaveFormat;

                if (Format == null)
                {
                    return;
                }

                int Channels = Format.Channels <= 0 ? 2 : Format.Channels;
                int Bits = Format.BitsPerSample;
                int BytesPerSample = Bits / 8;

                if (BytesPerSample <= 0)
                {
                    return;
                }

                // Loopback is normally 32-bit IEEE float; PCM 16/32 handled as a safety net.
                bool IsFloat32 = Bits == 32 && (Format.Encoding == WaveFormatEncoding.IeeeFloat || Format.Encoding == WaveFormatEncoding.Extensible);
                bool IsPcm16 = Bits == 16 && Format.Encoding == WaveFormatEncoding.Pcm;
                bool IsPcm32 = Bits == 32 && Format.Encoding == WaveFormatEncoding.Pcm;

                if (!IsFloat32 && !IsPcm16 && !IsPcm32)
                {
                    return; // unsupported sample format; skip safely
                }

                if (BandStart == null || Format.SampleRate != SampleRate)
                {
                    BuildBands(Format.SampleRate);
                }

                WaveBuffer Buffer = new(e.Buffer);

                float Read(int Sample)
                {
                    return IsFloat32 ? Buffer.FloatBuffer[Sample]
                        : IsPcm16 ? Buffer.ShortBuffer[Sample] / 32768f
                        : Buffer.IntBuffer[Sample] / 2147483648f;
                }

                // Only the recorded region is valid; the WaveBuffer is over-allocated.
                int SampleCount = e.BytesRecorded / BytesPerSample;
                int Frames = SampleCount / Channels;

                // De-interleave into per-channel ring buffers (channel 0 = left, 1 = right;
                // mono devices mirror the single channel to both).
                for (int F = 0; F < Frames; F++)
                {
                    int Base = F * Channels;

                    float Left = Read(Base);
                    float Right = Channels >= 2 ? Read(Base + 1) : Left;

                    LeftRing[RingWritePos] = Left;
                    RightRing[RingWritePos] = Right;
                    RingWritePos++;

                    if (RingWritePos >= FftSize)
                    {
                        RingWritePos = 0;
                        RingFilled = true;
                    }
                }

                if (!RingFilled)
                {
                    return;
                }

                // Throttle the expensive FFT + emit to ~EmitIntervalMs.
                long Now = Clock.ElapsedMilliseconds;

                if (Now - LastEmitMs < EmitIntervalMs)
                {
                    return;
                }

                LastEmitMs = Now;

                // Windowed FFT of each channel over the latest FftSize samples.
                // NoScaling keeps the magnitude scale deterministic, so Reference
                // (the 0 dBFS anchor) is exact regardless of the FFT conventions.
                int Index = RingWritePos;

                for (int N = 0; N < FftSize; N++)
                {
                    ValuesLeft[N] = new Complex(LeftRing[Index] * Window[N], 0.0);
                    ValuesRight[N] = new Complex(RightRing[Index] * Window[N], 0.0);
                    Index++;

                    if (Index >= FftSize)
                    {
                        Index = 0;
                    }
                }

                Fourier.Forward(ValuesLeft, FourierOptions.NoScaling);
                Fourier.Forward(ValuesRight, FourierOptions.NoScaling);

                // Peak magnitude per log-spaced band, for mono / left / right.
                // Mono spectrum = |(L + R) / 2| (exact: the FFT of the mixed signal).
                for (int I = 0; I < MaxSample; I++)
                {
                    double PeakMono = 0;
                    double PeakLeft = 0;
                    double PeakRight = 0;

                    for (int B = BandStart[I]; B < BandEnd[I]; B++)
                    {
                        Complex L = ValuesLeft[B];
                        Complex R = ValuesRight[B];

                        double Mono = ((L + R) * 0.5).Magnitude;
                        double Left = L.Magnitude;
                        double Right = R.Magnitude;

                        if (Mono > PeakMono)
                        {
                            PeakMono = Mono;
                        }

                        if (Left > PeakLeft)
                        {
                            PeakLeft = Left;
                        }

                        if (Right > PeakRight)
                        {
                            PeakRight = Right;
                        }
                    }

                    BandsMono[I] = PeakMono;
                    BandsLeft[I] = PeakLeft;
                    BandsRight[I] = PeakRight;
                }

                AudioDataAvailable?.Invoke(this, new Spectrum
                {
                    Mono = Finalize(BandsMono, EnvelopeMono),
                    Left = Finalize(BandsLeft, EnvelopeLeft),
                    Right = Finalize(BandsRight, EnvelopeRight)
                });
            }
            catch (Exception Exception)
            {
                Debug.WriteLine($"Failed to process audio data: {Exception.Message}");
            }
        }

        /// <summary>
        /// Maps band magnitudes to dBFS [0, 1], applies attack/decay into the persistent
        /// envelope, then neighbour-bin smoothing into a fresh output array.
        /// </summary>
        private double[] Finalize(double[] Bands, double[] Envelope)
        {
            for (int I = 0; I < MaxSample; I++)
            {
                double Magnitude = Bands[I];
                double Normalized;

                if (Magnitude <= 0)
                {
                    Normalized = 0;
                }
                else
                {
                    Normalized = (Magnitude * TiltGain[I] / Reference * LinearGain) - NoiseFloor;

                    if (Normalized < 0)
                    {
                        Normalized = 0;
                    }
                    else if (Normalized > 1)
                    {
                        Normalized = 1;
                    }
                }

                double Previous = Envelope[I];
                double Coefficient = Normalized > Previous ? AttackSpeed : ReleaseSpeed;

                Envelope[I] = Previous + ((Normalized - Previous) * Coefficient);
            }

            double[] Output = new double[MaxSample];

            for (int I = 0; I < MaxSample; I++)
            {
                double Value = 0;
                int Count = 0;

                for (int H = Math.Max(I - HorizontalSmoothness, 0); H <= Math.Min(I + HorizontalSmoothness, MaxSample - 1); H++)
                {
                    Value += Envelope[H];
                    Count++;
                }

                Output[I] = Value / Count;
            }

            return Output;
        }

        private void BuildWindow()
        {
            Window = new double[FftSize];

            double Total = 0;

            for (int N = 0; N < FftSize; N++)
            {
                Window[N] = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * N / (FftSize - 1))); // Hann
                Total += Window[N];
            }

            // Peak bin magnitude of a full-scale tone through this window (NoScaling FFT).
            Reference = Total / 2.0;
        }

        private void BuildBands(int Rate)
        {
            SampleRate = Rate;
            BandStart = new int[MaxSample];
            BandEnd = new int[MaxSample];
            TiltGain = new double[MaxSample];

            // Reset state so stale samples from the previous device/rate are dropped.
            RingFilled = false;
            RingWritePos = 0;
            Array.Clear(EnvelopeMono, 0, EnvelopeMono.Length);
            Array.Clear(EnvelopeLeft, 0, EnvelopeLeft.Length);
            Array.Clear(EnvelopeRight, 0, EnvelopeRight.Length);

            double BinHz = (double)Rate / FftSize;
            double LogMin = Math.Log(MinFrequency);
            double LogMax = Math.Log(Math.Min(MaxFrequency, Rate / 2.0));

            for (int I = 0; I < MaxSample; I++)
            {
                double F0 = Math.Exp(LogMin + ((LogMax - LogMin) * I / MaxSample));
                double F1 = Math.Exp(LogMin + ((LogMax - LogMin) * (I + 1) / MaxSample));

                int StartBin = (int)Math.Floor(F0 / BinHz);
                int EndBin = (int)Math.Floor(F1 / BinHz);

                if (StartBin < 1)
                {
                    StartBin = 1; // skip DC
                }

                if (EndBin <= StartBin)
                {
                    EndBin = StartBin + 1; // guarantee at least one bin per band
                }

                if (EndBin > HalfFft)
                {
                    EndBin = HalfFft;
                }

                BandStart[I] = StartBin;
                BandEnd[I] = EndBin;

                double Center = Math.Exp(LogMin + ((LogMax - LogMin) * (I + 0.5) / MaxSample));
                TiltGain[I] = Math.Pow(Center / MinFrequency, TiltDbPerOctave / (20.0 * Math.Log10(2.0)));
            }
        }

        public void OnDefaultDeviceChanged(DataFlow Flow, Role Role, string DefaultDeviceId)
        {
            if (Flow == DataFlow.Render)
            {
                try
                {
                    Capture?.StopRecording();

                    //MMDeviceEnumerator Enumerator = new();
                    //MMDevice DefaultDevice = Enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

                    Capture = CreateWasapiLoopbackCapture();
                    Capture.StartRecording();
                }
                catch (Exception Exception)
                {
                    Debug.WriteLine($"Failed to update WasapiLoopbackCapture device: {Exception.Message}");
                }
            }
        }

        public void OnDeviceStateChanged(string DeviceId, DeviceState NewState)
        {
            Debug.WriteLine($"Device state changed: Device Id -> {DeviceId} State -> {NewState}");
        }

        public void OnDeviceAdded(string PwstrDeviceId)
        {
            Debug.WriteLine($"Device added: {PwstrDeviceId}");
        }

        public void OnDeviceRemoved(string DeviceId)
        {
            Debug.WriteLine($"Device removed: {DeviceId}");
        }

        public void OnPropertyValueChanged(string PwstrDeviceId, PropertyKey Key)
        {
            Debug.WriteLine($"Property Value Changed: formatId -> {Key.formatId}  propertyId -> {Key.propertyId}");
        }

        public void Dispose()
        {
            DeviceEnum?.UnregisterEndpointNotificationCallback(this);
            Stop();
            //Calling dispose outside hangs.
            //Capture?.Dispose();
        }
    }
}