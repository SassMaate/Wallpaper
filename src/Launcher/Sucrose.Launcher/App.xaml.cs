using System.Globalization;
using System.Windows;
using Application = System.Windows.Application;
using SELLT = Skylark.Enum.LevelLogType;
using SHC = Skylark.Helper.Culture;
using SMMG = Sucrose.Manager.Manage.General;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMMRA = Sucrose.Memory.Manage.Readonly.App;
using SMMRM = Sucrose.Memory.Manage.Readonly.Mutex;
using SRHR = Sucrose.Resources.Helper.Resources;
using SSDEH = Sucrose.Shared.Discord.Extension.Hook;
using SSDHG = Sucrose.Shared.Dependency.Helper.Graphic;
using SSDHR = Sucrose.Shared.Dependency.Helper.Runtime;
using SSLCI = Sucrose.Shared.Launcher.Command.Interface;
using SSLMI = Sucrose.Shared.Launcher.Manage.Internal;
using SSMI = Sucrose.Signal.Manage.Internal;
using SSSHI = Sucrose.Shared.Space.Helper.Instance;
using SSSHW = Sucrose.Shared.Space.Helper.Watchdog;
using SSSSLSS = Sucrose.Shared.Signal.Services.LauncherSignalService;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static SSDEH Discord { get; set; } = new();

        private static bool HasError { get; set; } = true;

        public App()
        {
            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic);

            System.Windows.Forms.Application.ThreadException += async (s, e) =>
            {
                Exception Exception = e.Exception;

                await SSWEW.Watch_ThreadException(Exception);

                Message(Exception, true);
                //Close();
            };

            AppDomain.CurrentDomain.FirstChanceException += async (s, e) =>
            {
                Exception Exception = e.Exception;

                await SSWEW.Watch_FirstChanceException(Exception);

                Message(Exception, false);
                //Close();
            };

            AppDomain.CurrentDomain.UnhandledException += async (s, e) =>
            {
                Exception Exception = (Exception)e.ExceptionObject;

                await SSWEW.Watch_GlobalUnhandledException(Exception);

                Message(Exception, true);
                //Close();
            };

            TaskScheduler.UnobservedTaskException += async (s, e) =>
            {
                e.SetObserved();

                Exception Exception = e.Exception;

                await SSWEW.Watch_UnobservedTaskException(Exception);

                Message(Exception, false);
                //Close();
            };

            Current.DispatcherUnhandledException += async (s, e) =>
            {
                e.Handled = true;

                Exception Exception = e.Exception;

                await SSWEW.Watch_DispatcherUnhandledException(Exception);

                Message(Exception, true);
                //Close();
            };

            SHC.All = new CultureInfo(SMMG.Culture, true);

            SSDHR.Configure();

            SSDHG.Configure();
        }

        protected void Close()
        {
            SMMI.LauncherLogManager.Log(SELLT.Info, "Application has been closed.");

            Environment.Exit(0);
            Current.Shutdown();
            Shutdown();
        }

        protected void Message(Exception Exception, bool Show)
        {
            if (HasError)
            {
                HasError = !Show;

                string Path = SMMI.LauncherLogManager.LogFile();

                SSSHW.Start(SMMRA.Launcher, Exception, Show, Path);

                if (Show)
                {
                    Close();
                }
            }
        }

        protected void Configure()
        {
            SMMI.LauncherLogManager.Log(SELLT.Info, "Configuration initializing..");

            SSLMI.TrayIconManager.Start();

            SMMI.LauncherLogManager.Log(SELLT.Info, "Configuration initialized..");

            SMMI.LauncherLogManager.Log(SELLT.Info, "Server initializing..");

            SSMI.LauncherManager.StartChannel(SSSSLSS.Handler);

            SMMI.LauncherLogManager.Log(SELLT.Info, "Server initialized..");

            SMMI.LauncherLogManager.Log(SELLT.Info, "Discord hook initializing..");

            Discord.Initialize();

            SMMI.LauncherLogManager.Log(SELLT.Info, "Discord hook initialized..");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Discord.Dispose();

            SSMI.LauncherManager.StopChannel();

            SSLMI.TrayIconManager.Dispose();

            Close();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SRHR.SetLanguage(SMMG.Culture);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            SMMI.LauncherLogManager.Log(SELLT.Info, "Application initializing..");

            if (SSSHI.Basic(SMMRM.Launcher, SMMRA.Launcher))
            {
                Configure();

                SMMI.LauncherLogManager.Log(SELLT.Info, "Application initialized..");
            }
            else
            {
                SMMI.LauncherLogManager.Log(SELLT.Warning, "Application could not be initialized!");

                SMMI.LauncherLogManager.Log(SELLT.Info, "Application Interface opening..");

                SSLCI.Command();

                SMMI.LauncherLogManager.Log(SELLT.Info, "Application Interface opened..");

                Close();
            }
        }
    }
}