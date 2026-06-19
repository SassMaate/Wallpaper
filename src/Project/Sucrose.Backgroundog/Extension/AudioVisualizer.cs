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

        // Output contract: 128 magnitudes (wallpapers index Data[0..127]); keep this count.
        private readonly int MaxSample = 128;

        // Fixed power-of-two FFT size (decoupled from the WASAPI callback buffer size,
        // which used to vary and shift the bin -> frequency mapping per callback).
        private const int FftSize = 4096;
        private const int HalfFft = FftSize / 2;

        // Frequency range mapped across the 128 output bins, on a logarithmic
        // (octave/perceptual) scale instead of the old "first 128 linear bins".
        // This raises the visualised ceiling from ~a couple of kHz to the full audible range.
        private const double MinFrequency = 20.0;
        private const double MaxFrequency = 16000.0;

        // Output scaling. The old code emitted raw, un-normalised single-bin
        // magnitudes; with the Hann window (coherent gain ~0.5) and per-band
        // averaging the natural scale is a bit lower, so compensate here.
        // Tune this if your wallpapers look flatter/spikier than before.
        private const double Gain = 2.0;

        private readonly int VerticalSmoothness = 2;   // temporal frames to average
        private readonly int HorizontalSmoothness = 1; // neighbour bins to average

        private int SampleRate;
        private int[] BandStart;
        private int[] BandEnd;
        private double[] Window;
        private bool RingFilled;
        private int RingWritePos;
        private readonly float[] MonoRing = new float[FftSize];
        private readonly List<double[]> Smooth = [];

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

                if (BandStart == null || Format.SampleRate != SampleRate)
                {
                    BuildBands(Format.SampleRate);
                }

                WaveBuffer Buffer = new(e.Buffer);

                // Only the recorded region is valid; the WaveBuffer is over-allocated.
                int ValidFloats = e.BytesRecorded / 4;
                int Frames = ValidFloats / Channels;

                // De-interleave to mono and append into the ring buffer.
                for (int F = 0; F < Frames; F++)
                {
                    double Sum = 0;
                    int Base = F * Channels;

                    for (int C = 0; C < Channels; C++)
                    {
                        Sum += Buffer.FloatBuffer[Base + C];
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
                Complex[] Values = new Complex[FftSize];
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

                Fourier.Forward(Values, FourierOptions.Default);

                // Aggregate magnitudes into 128 log-spaced bands.
                double[] Frame = new double[MaxSample];

                for (int I = 0; I < MaxSample; I++)
                {
                    double Accumulator = 0;
                    int Count = 0;

                    for (int B = BandStart[I]; B < BandEnd[I]; B++)
                    {
                        Accumulator += Values[B].Magnitude;
                        Count++;
                    }

                    Frame[I] = Count > 0 ? Accumulator / Count * Gain : 0.0;
                }

                // Temporal smoothing across the last VerticalSmoothness frames.
                Smooth.Add(Frame);

                if (Smooth.Count > VerticalSmoothness)
                {
                    Smooth.RemoveAt(0);
                }

                // Combine temporal and horizontal (neighbour-bin) smoothing.
                double[] AudioData = new double[MaxSample];

                for (int I = 0; I < MaxSample; I++)
                {
                    double Value = 0;
                    int Count = 0;

                    for (int H = Math.Max(I - HorizontalSmoothness, 0); H <= Math.Min(I + HorizontalSmoothness, MaxSample - 1); H++)
                    {
                        Value += VSmooth(H);
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

        private double VSmooth(int Bin)
        {
            double Value = 0;

            for (int V = 0; V < Smooth.Count; V++)
            {
                Value += Smooth[V] != null ? Smooth[V][Bin] : 0.0;
            }

            return Smooth.Count > 0 ? Value / Smooth.Count : 0.0;
        }

        private void BuildWindow()
        {
            Window = new double[FftSize];

            for (int N = 0; N < FftSize; N++)
            {
                Window[N] = 0.5 * (1.0 - Math.Cos(2.0 * Math.PI * N / (FftSize - 1))); // Hann
            }
        }

        private void BuildBands(int Rate)
        {
            SampleRate = Rate;
            BandStart = new int[MaxSample];
            BandEnd = new int[MaxSample];

            // Reset the ring so stale samples from the previous device/rate are dropped.
            RingFilled = false;
            RingWritePos = 0;
            Smooth.Clear();

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