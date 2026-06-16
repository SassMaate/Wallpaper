using LibVLCSharp.Shared;
using Sucrose.Live.VlcPlayer;
using System.Windows.Media;
using SSEVPHA = Sucrose.Shared.Engine.VlcPlayer.Helper.Aspect;
using SSEVPMI = Sucrose.Shared.Engine.VlcPlayer.Manage.Internal;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Shared.Engine.VlcPlayer.Helper
{
    internal static class Video
    {
        public static async void Pause()
        {
            try
            {
                SSEVPMI.MediaEngine.SetPause(true);
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        public static async void Play()
        {
            try
            {
                SSEVPMI.MediaEngine.Play();
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        public static async void Stop()
        {
            try
            {
                SSEVPMI.MediaEngine.Stop();
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        public static async void SetLoop(bool State)
        {
            try
            {
                if (State && SSEVPMI.MediaEngine.State != VLCState.Playing)
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ThreadPool.QueueUserWorkItem(_ => SSEVPMI.MediaEngine.Play(SSEVPMI.MediaBase));
                    });
                }
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        public static async void SetMute(bool State)
        {
            try
            {
                SSEVPMI.MediaEngine.Mute = State;
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        public static async void SetVolume(int Volume)
        {
            try
            {
                SSEVPMI.MediaEngine.Volume = Volume;
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        public static async void SetSpeed(float Speed)
        {
            try
            {
                SSEVPMI.MediaEngine.SetRate(Speed);
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        public static async void SetStretch(Stretch Mode)
        {
            try
            {
                switch (Mode)
                {
                    case Stretch.Fill:
                        // Stretch to fill window (may distort)
                        SSEVPMI.MediaEngine.Scale = 0f;  // Auto-scale to window
                        SSEVPMI.MediaEngine.AspectRatio = SSEVPHA.GetRatio();
                        SSEVPMI.MediaEngine.CropGeometry = null;
                        break;
                    case Stretch.Uniform:
                        // Fit inside window keeping aspect ratio (letterbox)
                        SSEVPMI.MediaEngine.Scale = 0f;           // Auto-scale to window
                        SSEVPMI.MediaEngine.AspectRatio = null;   // Keep original aspect ratio
                        SSEVPMI.MediaEngine.CropGeometry = null;  // No cropping
                        break;
                    case Stretch.UniformToFill:
                        // Fill window keeping aspect ratio (crop sides)
                        SSEVPMI.MediaEngine.Scale = 0f;                         // Auto-scale to window
                        SSEVPMI.MediaEngine.AspectRatio = null;                 // Keep original aspect ratio  
                        SSEVPMI.MediaEngine.CropGeometry = SSEVPHA.GetRatio();  // Crop to window ratio
                        break;
                    case Stretch.None:
                    default:
                        // Original size, no scaling
                        SSEVPMI.MediaEngine.Scale = 1.0f;
                        SSEVPMI.MediaEngine.AspectRatio = null;
                        SSEVPMI.MediaEngine.CropGeometry = null;
                        break;
                }
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }
    }
}