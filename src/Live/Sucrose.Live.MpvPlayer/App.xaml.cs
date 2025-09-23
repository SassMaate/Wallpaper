using System.Globalization;
using System.IO;
using System.Windows;
using Application = System.Windows.Application;
using SEWTT = Skylark.Enum.WindowsThemeType;
using SHC = Skylark.Helper.Culture;
using SHV = Skylark.Helper.Versionly;
using SMMCB = Sucrose.Memory.Manage.Constant.Backgroundog;
using SMMG = Sucrose.Manager.Manage.General;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMML = Sucrose.Manager.Manage.Library;
using SMMRA = Sucrose.Memory.Manage.Readonly.App;
using SMMRC = Sucrose.Memory.Manage.Readonly.Content;
using SMMRF = Sucrose.Memory.Manage.Readonly.Folder;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRM = Sucrose.Memory.Manage.Readonly.Mutex;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;
using SRER = Sucrose.Resources.Extension.Resources;
using SRHR = Sucrose.Resources.Helper.Resources;
using SSDEWT = Sucrose.Shared.Dependency.Enum.WallpaperType;
using SSDMMG = Sucrose.Shared.Dependency.Manage.Manager.General;
using SSEHA = Sucrose.Shared.Engine.Helper.Awakening;
using SSEHCR = Sucrose.Shared.Engine.Helper.Crashing;
using SSEHCY = Sucrose.Shared.Engine.Helper.Cycyling;
using SSEHR = Sucrose.Shared.Engine.Helper.Run;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SSEMPHC = Sucrose.Shared.Engine.MpvPlayer.Helper.Config;
using SSEMPHP = Sucrose.Shared.Engine.MpvPlayer.Helper.Properties;
using SSEMPMI = Sucrose.Shared.Engine.MpvPlayer.Manage.Internal;
using SSEMPVG = Sucrose.Shared.Engine.MpvPlayer.View.Gif;
using SSEMPVV = Sucrose.Shared.Engine.MpvPlayer.View.Video;
using SSEVDWB = Sucrose.Shared.Engine.View.DarkWarningBox;
using SSEVLWB = Sucrose.Shared.Engine.View.LightWarningBox;
using SSSHC = Sucrose.Shared.Space.Helper.Cycyling;
using SSSHI = Sucrose.Shared.Space.Helper.Instance;
using SSSHS = Sucrose.Shared.Space.Helper.Security;
using SSSHWG = Sucrose.Shared.Space.Helper.Watchdog;
using SSSHWS = Sucrose.Shared.Space.Helper.Windows;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;
using SSTHV = Sucrose.Shared.Theme.Helper.Various;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Live.MpvPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static bool HasError { get; set; } = true;

        public App()
        {
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic);

            System.Windows.Forms.Application.ThreadException += async (s, e) =>
            {
                Exception Exception = e.Exception;

                await SSWEW.Watch_ThreadException(Exception);

                //Close();
                Message(Exception);
            };

            AppDomain.CurrentDomain.FirstChanceException += async (s, e) =>
            {
                Exception Exception = e.Exception;

                await SSWEW.Watch_FirstChanceException(Exception);

                //Close();
                //Message(Exception);
            };

            AppDomain.CurrentDomain.UnhandledException += async (s, e) =>
            {
                Exception Exception = (Exception)e.ExceptionObject;

                await SSWEW.Watch_GlobalUnhandledException(Exception);

                //Close();
                Message(Exception);
            };

            TaskScheduler.UnobservedTaskException += async (s, e) =>
            {
                Exception Exception = e.Exception;

                await SSWEW.Watch_UnobservedTaskException(Exception);

                e.SetObserved();

                //Close();
                Message(Exception);
            };

            Current.DispatcherUnhandledException += async (s, e) =>
            {
                Exception Exception = e.Exception;

                await SSWEW.Watch_DispatcherUnhandledException(Exception);

                e.Handled = true;

                //Close();
                Message(Exception);
            };

            SHC.All = new CultureInfo(SMMG.Culture, true);
        }

        protected void Close()
        {
            Environment.Exit(0);
            Current.Shutdown();
            Shutdown();
        }

        protected void Message(Exception Exception)
        {
            if (HasError)
            {
                HasError = false;

                string Path = SMMI.MpvPlayerLiveLogManager.LogFile();

                SSSHWG.Start(SMMRA.MpvPlayerLive, Exception, Path);

                Close();
            }
        }

        protected bool Check()
        {
            return SSSHWS.IsRedstone1();
        }

        protected void Checker()
        {
            if (Check())
            {
                Configure();
            }
            else
            {
                string CloseText = SRER.GetValue("Live", "Close");

                string DialogInfo = SRER.GetValue("Live", "Info", "MpvPlayer");
                string DialogMessage = SRER.GetValue("Live", "Message", "MpvPlayer");

                string DialogTitle = string.Format(SRER.GetValue("Live", "Title"), "MpvPlayer");

                switch (SSDMMG.ThemeType)
                {
                    case SEWTT.Dark:
                        SSEVDWB DarkWarningBox = new(DialogTitle, DialogMessage, DialogInfo, CloseText);
                        DarkWarningBox.ShowDialog();
                        break;
                    default:
                        SSEVLWB LightWarningBox = new(DialogTitle, DialogMessage, DialogInfo, CloseText);
                        LightWarningBox.ShowDialog();
                        break;
                }

                Close();
            }
        }

        protected void Configure()
        {
            SSEMI.LibraryLocation = SMML.Location;
            SSEMI.LibrarySelected = SMML.Selected;

            if (SMMI.LibrarySettingManager.CheckFile() && !string.IsNullOrEmpty(SSEMI.LibrarySelected))
            {
                SSEMI.InfoPath = Path.Combine(SSEMI.LibraryLocation, SSEMI.LibrarySelected, SMMRC.SucroseInfo);
                SSEMPMI.MpvPath = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.MpvPlayer);
                SSEMI.PropertiesPath = Path.Combine(SSEMI.LibraryLocation, SSEMI.LibrarySelected, SMMRC.SucroseProperties);

                if (File.Exists(SSEMI.InfoPath) && SSTHI.ReadCheck(SSEMI.InfoPath))
                {
                    SSEMI.Info = SSTHI.ReadJson(SSEMI.InfoPath);

                    if (SSEMI.Info.AppVersion.CompareTo(SHV.Entry()) <= 0)
                    {
                        string Source = SSEMI.Info.Source;

                        if (!SSTHV.IsUrl(Source))
                        {
                            Source = Path.Combine(SSEMI.LibraryLocation, SSEMI.LibrarySelected, Source);
                        }

                        SMMI.BackgroundogSettingManager.SetSetting(new KeyValuePair<string, bool>[]
                        {
                            new(SMMCB.PipeRequired, false),
                            new(SMMCB.AudioRequired, false),
                            new(SMMCB.SignalRequired, false),
                            new(SMMCB.PausePerformance, false),
                            new(SMMCB.TransmissionRequired, false)
                        });

                        if (SSTHV.IsUrl(Source) || File.Exists(Source))
                        {
                            SSEHA.Start();

                            SSEHCR.Start();

                            SSEHCY.Start();

                            SSEMPHC.Start();

                            SSEMPHP.Start();

                            SSEMI.Host = $"{Path.Combine(SSEMI.LibraryLocation, SSEMI.LibrarySelected)}/";

                            switch (SSEMI.Info.Type)
                            {
                                case SSDEWT.Gif:
                                    SSEMPVG Gif = new();
                                    Gif.Show();
                                    break;
                                case SSDEWT.Video:
                                    SSEMPVV Video = new();
                                    Video.Show();
                                    break;
                                default:
                                    Close();
                                    break;
                            }
                        }
                        else
                        {
                            Close();
                        }
                    }
                    else
                    {
                        Close();
                    }
                }
                else
                {
                    Close();
                }
            }
            else
            {
                Close();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            //

            Close();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SRHR.SetLanguage(SMMG.Culture);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            if (SSSHI.Basic(SMMRM.Live, SMMRA.MpvPlayerLive) && SSEHR.Check())
            {
                SSSHS.Apply();

                if (SSSHC.Check())
                {
                    SSSHC.Change();
                }
                else
                {
                    Checker();
                }
            }
            else
            {
                Close();
            }
        }
    }
}