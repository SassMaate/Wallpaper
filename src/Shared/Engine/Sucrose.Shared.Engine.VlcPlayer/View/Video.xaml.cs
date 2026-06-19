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
using SSEVPEV = Sucrose.Shared.Engine.VlcPlayer.Event.Video;
using SSEVPHS = Sucrose.Shared.Engine.VlcPlayer.Helper.Surface;
using SSEVPHV = Sucrose.Shared.Engine.VlcPlayer.Helper.Video;
using SSEVPMI = Sucrose.Shared.Engine.VlcPlayer.Manage.Internal;

namespace Sucrose.Shared.Engine.VlcPlayer.View
{
    /// <summary>
    /// Interaction logic for Video.xaml
    /// </summary>
    public sealed partial class Video : Window, IDisposable
    {
        public Video()
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

            SSEVPMI.MediaHandle = SSEVPMI.MediaEngine.Hwnd;
            SSEVPMI.MediaEngine.SetAdjustInt(VideoAdjustOption.Enable, 1);

            SSEVPMI.MediaEngine.EndReached += SSEVPEV.MediaEngineEndReached;

            SSEVPMI.MediaView.Loaded += SSEVPEV.MediaViewLoaded;

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

            SSEVPHV.SetLoop(SSEHD.GetLoop());

            SSEVPHV.SetVolume(SSEHD.GetVolume());

            SSEVPHV.SetStretch((Stretch)SSEHD.GetStretch());

            if (SMMB.PausePerformance)
            {
                SSEVPHV.Pause();

                SSEMI.PausePerformance = true;
            }
            else if (SSEMI.PausePerformance)
            {
                SSEVPHV.Play();

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