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
        public event EventHandler<double[]> AudioDataAvailable;

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

        // Output is mapped to dBFS: 0 dBFS (full scale) -> 1.0, -DynamicRangeDb -> 0.0.
        // Anything quieter than -DynamicRangeDb is clamped to 0, which doubles as a
        // noise gate so silence / paused audio stays flat instead of flickering.
        private const double DynamicRangeDb = 55.0;

        // Asymmetric (attack/decay) smoothing applied per bin, per FFT update:
        // fast rise so bars snap to the beat, slow fall so they ease back down.
        // These assume the current per-callback update rate; retune if FFT updates
        // are ever throttled to a fixed rate.
        private const double AttackSpeed = 0.35;
        private const double ReleaseSpeed = 0.06;

        // Neighbour-bin averaging to take the edge off jagged bars.
        private const int HorizontalSmoothness = 1;
        // --------------------------------------------------------------------

        private int SampleRate;
        private int[] BandStart;
        private int[] BandEnd;
        private double[] Window;
        private double Reference; // bin magnitude of a full-scale tone (the 0 dBFS anchor)
        private bool RingFilled;
        private int RingWritePos;

        // Pre-allocated and reused every update to avoid per-callback GC churn.
        private readonly float[] MonoRing = new float[FftSize];
        private readonly Complex[] Values = new Complex[FftSize];
        private readonly double[] Bands = new double[MaxSample];
        private readonly double[] Envelope = new double[MaxSample];

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

                // Only the recorded region is valid; the WaveBuffer is over-allocated.
                int SampleCount = e.BytesRecorded / BytesPerSample;
                int Frames = SampleCount / Channels;

                // De-interleave to mono and append into the ring buffer.
                for (int F = 0; F < Frames; F++)
                {
                    double Sum = 0;
                    int Base = F * Channels;

                    for (int C = 0; C < Channels; C++)
                    {
                        int S = Base + C;

                        Sum += IsFloat32 ? Buffer.FloatBuffer[S]
                            : IsPcm16 ? Buffer.ShortBuffer[S] / 32768.0
                            : Buffer.IntBuffer[S] / 2147483648.0;
                    }

                    MonoRing[RingWritePos] = (float)(Sum / Channels);
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

                // Windowed FFT over the latest FftSize mono samples (oldest -> newest).
                // NoScaling keeps the magnitude scale deterministic, so Reference (the
                // 0 dBFS anchor) is exact regardless of the FFT library's conventions.
                int Index = RingWritePos;

                for (int N = 0; N < FftSize; N++)
                {
                    Values[N] = new Complex(MonoRing[Index] * Window[N], 0.0);
                    Index++;

                    if (Index >= FftSize)
                    {
                        Index = 0;
                    }
                }

                Fourier.Forward(Values, FourierOptions.NoScaling);

                // Peak magnitude per log-spaced band (peak keeps a tone from being
                // diluted to nothing inside the wide high-frequency bands).
                for (int I = 0; I < MaxSample; I++)
                {
                    double Peak = 0;

                    for (int B = BandStart[I]; B < BandEnd[I]; B++)
                    {
                        double Magnitude = Values[B].Magnitude;

                        if (Magnitude > Peak)
                        {
                            Peak = Magnitude;
                        }
                    }

                    Bands[I] = Peak;
                }

                // Map to dBFS [0, 1] and apply attack/decay smoothing.
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
                        double Decibel = 20.0 * Math.Log10(Magnitude / Reference);

                        Normalized = 1.0 + Decibel / DynamicRangeDb;

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

                    Envelope[I] = Previous + (Normalized - Previous) * Coefficient;
                }

                // Neighbour-bin smoothing into a fresh array (consumers keep this reference).
                double[] AudioData = new double[MaxSample];

                for (int I = 0; I < MaxSample; I++)
                {
                    double Value = 0;
                    int Count = 0;

                    for (int H = Math.Max(I - HorizontalSmoothness, 0); H <= Math.Min(I + HorizontalSmoothness, MaxSample - 1); H++)
                    {
                        Value += Envelope[H];
                        Count++;
                    }

                    AudioData[I] = Value / Count;
                }

                AudioDataAvailable?.Invoke(this, AudioData);
            }
            catch (Exception Exception)
            {
                Debug.WriteLine($"Failed to process audio data: {Exception.Message}");
            }
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

            // Reset state so stale samples from the previous device/rate are dropped.
            RingFilled = false;
            RingWritePos = 0;
            Array.Clear(Envelope, 0, Envelope.Length);

            double BinHz = (double)Rate / FftSize;
            double LogMin = Math.Log(MinFrequency);
            double LogMax = Math.Log(Math.Min(MaxFrequency, Rate / 2.0));

            for (int I = 0; I < MaxSample; I++)
            {
                double F0 = Math.Exp(LogMin + (LogMax - LogMin) * I / MaxSample);
                double F1 = Math.Exp(LogMin + (LogMax - LogMin) * (I + 1) / MaxSample);

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