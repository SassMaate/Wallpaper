using LibVLCSharp.Shared;
using Sucrose.Live.VlcPlayer;
using System.Windows.Media;
using SSEVPHA = Sucrose.Shared.Engine.VlcPlayer.Helper.Aspect;
using SSEVPMI = Sucrose.Shared.Engine.VlcPlayer.Manage.Internal;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Shared.Engine.VlcPlayer.Helper
{
    internal static class Gif
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

        // Rebuilds MediaBase for the animated GIF. The avformat demuxer + software decoding are
        // always required for GIF (see comments below). When Loop is enabled it also adds libVLC's
        // ":input-repeat" option so the input is repeated INTERNALLY (a seek-to-start that keeps the
        // decoder and video output alive) instead of cold-restarting the pipeline via Play() on
        // EndReached. The cold restart left the VideoView's native HWND blank while the (always
        // software) GIF decoder re-initialized, exposing the desktop behind the wallpaper window.
        // 65535 is the VLC maximum (range 0..65535); negative/zero values disable looping.
        public static void BuildMedia(bool Loop)
        {
            SSEVPMI.MediaBase = new(SSEVPMI.MediaLibrary, new Uri(SSEVPMI.Source));

            // VLC's native image demuxer (priority 10) outranks the FFmpeg/avformat demuxer (priority 2)
            // and opens an animated GIF as a single still image, freezing it on the first frame. Forcing
            // the avformat demuxer routes the GIF through FFmpeg, which decodes every frame. The avformat
            // module ships compiled inside libavcodec_plugin.dll, so no extra plugin is required.
            SSEVPMI.MediaBase.AddOption(":demux=avformat");

            // GIF is a software-only codec; hardware decoding offers nothing for it and can produce
            // corrupt frames, so force software decoding for the GIF media specifically.
            SSEVPMI.MediaBase.AddOption(":avcodec-hw=none");

            if (Loop)
            {
                SSEVPMI.MediaBase.AddOption(":input-repeat=65535");
            }

            SSEVPMI.LoopApplied = Loop;
        }

        public static async void SetLoop(bool State)
        {
            try
            {
                if (State != SSEVPMI.LoopApplied)
                {
                    // Loop toggled at runtime. ":input-repeat" is fixed when the Media is created
                    // and cannot be changed on a live Media, so rebuild MediaBase to match the new
                    // intent and restart playback once (only on the explicit toggle, never per loop).
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        ThreadPool.QueueUserWorkItem(_ =>
                        {
                            BuildMedia(State);
                            SSEVPMI.MediaEngine.Play(SSEVPMI.MediaBase);
                        });
                    });
                }
                else if (State && SSEVPMI.MediaEngine.State != VLCState.Playing)
                {
                    // Loop wanted but playback stopped (startup, or the practically-never repeat
                    // exhaustion): replay the already-configured media.
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