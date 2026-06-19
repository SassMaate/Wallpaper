using LibVLCSharp.Shared;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;
using MediaEngine = LibVLCSharp.Shared.MediaPlayer;
using SMMB = Sucrose.Manager.Manage.Backgroundog;
using SMME = Sucrose.Manager.Manage.Engine;
using SSEEH = Sucrose.Shared.Engine.Event.Handler;
using SSEHD = Sucrose.Shared.Engine.Helper.Data;
using SSEHR = Sucrose.Shared.Engine.Helper.Run;
using SSEHS = Sucrose.Shared.Engine.Helper.Source;
using SSEHV = Sucrose.Shared.Engine.Helper.Volume;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SSEVPEG = Sucrose.Shared.Engine.VlcPlayer.Event.Gif;
using SSEVPHG = Sucrose.Shared.Engine.VlcPlayer.Helper.Gif;
using SSEVPHS = Sucrose.Shared.Engine.VlcPlayer.Helper.Surface;
using SSEVPMI = Sucrose.Shared.Engine.VlcPlayer.Manage.Internal;

namespace Sucrose.Shared.Engine.VlcPlayer.View
{
    /// <summary>
    /// Interaction logic for Gif.xaml
    /// </summary>
    public sealed partial class Gif : Window, IDisposable
    {
        public Gif()
        {
            InitializeComponent();

            SystemEvents.DisplaySettingsChanged += (s, e) => SSEEH.DisplaySettingsChanged(this);

            SSEVPMI.Source = SSEHS.GetSource(SSEMI.Info.Source, SSEMI.Host).ToString();

            ContentRendered += (s, e) => SSEEH.ContentRendered(this);

            SSEVPMI.MediaView = new();
            Content = SSEVPMI.MediaView;

            SSEVPMI.MediaLibrary = new
            (
                "no-lua",
                "no-osd",
                "no-spu",
                "no-stats",
                "no-drop-late-frames",
                "no-snapshot-preview",
                "no-sub-autodetect-file",
                "no-metadata-network-access",
                SMME.StayAwake ? "disable-screensaver" : "no-disable-screensaver"
            );

            SSEVPMI.MediaEngine = new MediaEngine(SSEVPMI.MediaLibrary)
            {
                Fullscreen = true,
                EnableKeyInput = false,
                EnableMouseInput = false,
                Volume = SSEHD.GetVolume(),
                EnableHardwareDecoding = SMME.HardwareAcceleration
            };

            SSEVPMI.MediaBase = new(SSEVPMI.MediaLibrary, new Uri(SSEVPMI.Source));

            // VLC's native image demuxer (priority 10) outranks the FFmpeg/avformat demuxer (priority 2)
            // and opens an animated GIF as a single still image, freezing it on the first frame. Forcing
            // the avformat demuxer routes the GIF through FFmpeg, which decodes every frame. The avformat
            // module ships compiled inside libavcodec_plugin.dll, so no extra plugin is required.
            SSEVPMI.MediaBase.AddOption(":demux=avformat");

            // GIF is a software-only codec; hardware decoding offers nothing for it and can produce
            // corrupt frames, so force software decoding for the GIF media specifically.
            SSEVPMI.MediaBase.AddOption(":avcodec-hw=none");

            SSEVPMI.MediaHandle = SSEVPMI.MediaEngine.Hwnd;
            SSEVPMI.MediaEngine.SetAdjustInt(VideoAdjustOption.Enable, 1);

            SSEVPMI.MediaEngine.EndReached += SSEVPEG.MediaEngineEndReached;

            SSEVPMI.MediaView.Loaded += SSEVPEG.MediaViewLoaded;

            SSEMI.GeneralTimer.Tick += new EventHandler(GeneralTimer_Tick);
            SSEMI.GeneralTimer.Interval = new TimeSpan(0, 0, 1);
            SSEMI.GeneralTimer.Start();

            Closing += (s, e) => SSEVPMI.MediaEngine.Dispose();
            Loaded += (s, e) => SSEEH.WindowLoaded(this);
            SizeChanged += (s, e) => SSEVPHS.Correct();

            SSEHV.Start();
        }

        private void GeneralTimer_Tick(object sender, EventArgs e)
        {
            Dispose();

            SSEHR.Control();

            SSEVPHG.SetLoop(SSEHD.GetLoop());

            SSEVPHG.SetStretch((Stretch)SSEHD.GetStretch());

            if (SMMB.PausePerformance)
            {
                SSEVPHG.Pause();

                SSEMI.PausePerformance = true;
            }
            else if (SSEMI.PausePerformance)
            {
                SSEVPHG.Play();

                SSEMI.PausePerformance = false;
            }
        }

        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}