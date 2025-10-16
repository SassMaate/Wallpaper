using System.Globalization;
using System.IO;
using System.Windows;
using Application = System.Windows.Application;
using SHC = Skylark.Helper.Culture;
using SHV = Skylark.Helper.Versionly;
using SMMCB = Sucrose.Memory.Manage.Constant.Backgroundog;
using SMMG = Sucrose.Manager.Manage.General;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMML = Sucrose.Manager.Manage.Library;
using SMMRA = Sucrose.Memory.Manage.Readonly.App;
using SMMRC = Sucrose.Memory.Manage.Readonly.Content;
using SMMRM = Sucrose.Memory.Manage.Readonly.Mutex;
using SRHR = Sucrose.Resources.Helper.Resources;
using SSDEWT = Sucrose.Shared.Dependency.Enum.WallpaperType;
using SSDHG = Sucrose.Shared.Dependency.Helper.Graphic;
using SSDHR = Sucrose.Shared.Dependency.Helper.Runtime;
using SSEAVA = Sucrose.Shared.Engine.Aurora.View.Application;
using SSEHA = Sucrose.Shared.Engine.Helper.Awakening;
using SSEHCR = Sucrose.Shared.Engine.Helper.Crashing;
using SSEHCY = Sucrose.Shared.Engine.Helper.Cycyling;
using SSEHR = Sucrose.Shared.Engine.Helper.Run;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SSSHC = Sucrose.Shared.Space.Helper.Cycyling;
using SSSHI = Sucrose.Shared.Space.Helper.Instance;
using SSSHS = Sucrose.Shared.Space.Helper.Security;
using SSSHW = Sucrose.Shared.Space.Helper.Watchdog;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;
using SSTHV = Sucrose.Shared.Theme.Helper.Various;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Live.Aurora
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

                Message(Exception);
                //Close();
            };

            AppDomain.CurrentDomain.FirstChanceException += async (s, e) =>
            {
                Exception Exception = e.Exception;

                await SSWEW.Watch_FirstChanceException(Exception);

                //Message(Exception);
                //Close();
            };

            AppDomain.CurrentDomain.UnhandledException += async (s, e) =>
            {
                Exception Exception = (Exception)e.ExceptionObject;

                await SSWEW.Watch_GlobalUnhandledException(Exception);

                Message(Exception);
                //Close();
            };

            TaskScheduler.UnobservedTaskException += async (s, e) =>
            {
                e.SetObserved();

                Exception Exception = e.Exception;

                await SSWEW.Watch_UnobservedTaskException(Exception);

                //Message(Exception);
                //Close();
            };

            Current.DispatcherUnhandledException += async (s, e) =>
            {
                e.Handled = true;

                Exception Exception = e.Exception;

                await SSWEW.Watch_DispatcherUnhandledException(Exception);

                Message(Exception);
                //Close();
            };

            SHC.All = new CultureInfo(SMMG.Culture, true);

            SSDHR.Configure();

            SSDHG.Configure();
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

                string Path = SMMI.AuroraLiveLogManager.LogFile();

                SSSHW.Start(SMMRA.AuroraLive, Exception, Path);

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

                if (File.Exists(SSEMI.InfoPath) && SSTHI.ReadCheck(SSEMI.InfoPath))
                {
                    SSEMI.Info = SSTHI.ReadJson(SSEMI.InfoPath);

                    if (SSEMI.Info.AppVersion.CompareTo(SHV.Entry()) <= 0)
                    {
                        string Source = SSEMI.Info.Source;

                        if (SSTHV.IsUrl(Source))
                        {
                            Close();
                        }
                        else
                        {
                            Source = Path.Combine(SSEMI.LibraryLocation, SSEMI.LibrarySelected, Source);

                            SMMI.BackgroundogSettingManager.SetSetting(new KeyValuePair<string, bool>[]
                            {
                                new(SMMCB.PipeRequired, false),
                                new(SMMCB.AudioRequired, false),
                                new(SMMCB.SignalRequired, false),
                                new(SMMCB.PausePerformance, false),
                                new(SMMCB.TransmissionRequired, false)
                            });

                            if (File.Exists(Source))
                            {
                                SSEHA.Start();

                                SSEHCR.Start();

                                SSEHCY.Start();

                                SSEMI.Host = $"{Path.Combine(SSEMI.LibraryLocation, SSEMI.LibrarySelected)}/";

                                switch (SSEMI.Info.Type)
                                {
                                    case SSDEWT.Application:
                                        SSEAVA Application = new();
                                        Application.Show();
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

            if (SSSHI.Basic(SMMRM.Live, SMMRA.AuroraLive) && SSEHR.Check())
            {
                SSSHS.Apply();

                if (SSSHC.Check())
                {
                    SSSHC.Change();
                }
                else
                {
                    Configure();
                }
            }
            else
            {
                Close();
            }
        }
    }
}