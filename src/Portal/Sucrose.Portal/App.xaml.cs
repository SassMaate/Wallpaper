using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sucrose.Portal.Dependency;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using Wpf.Ui;
using Wpf.Ui.DependencyInjection;
using Application = System.Windows.Application;
using SEAT = Skylark.Enum.AssemblyType;
using SHA = Skylark.Helper.Assemblies;
using SHC = Skylark.Helper.Culture;
using SMMG = Sucrose.Manager.Manage.General;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMMRA = Sucrose.Memory.Manage.Readonly.App;
using SMMRM = Sucrose.Memory.Manage.Readonly.Mutex;
using SPMAC = Sucrose.Portal.Models.AppConfig;
using SPSAHS = Sucrose.Portal.Services.ApplicationHostService;
using SPSCIW = Sucrose.Portal.Services.Contracts.IWindow;
using SPSWPS = Sucrose.Portal.Services.WindowsProviderService;
using SPVMPDSVM = Sucrose.Portal.ViewModels.Pages.DonateSettingViewModel;
using SPVMPGSVM = Sucrose.Portal.ViewModels.Pages.GeneralSettingViewModel;
using SPVMPLVM = Sucrose.Portal.ViewModels.Pages.LibraryViewModel;
using SPVMPOSVM = Sucrose.Portal.ViewModels.Pages.OtherSettingViewModel;
using SPVMPPESVM = Sucrose.Portal.ViewModels.Pages.PerformanceSettingViewModel;
using SPVMPPLSVM = Sucrose.Portal.ViewModels.Pages.PersonalSettingViewModel;
using SPVMPSSVM = Sucrose.Portal.ViewModels.Pages.SystemSettingViewModel;
using SPVMPSVM = Sucrose.Portal.ViewModels.Pages.StoreViewModel;
using SPVMPWSVM = Sucrose.Portal.ViewModels.Pages.WallpaperSettingViewModel;
using SPVMWMWVM = Sucrose.Portal.ViewModels.Windows.MainWindowViewModel;
using SPVPLP = Sucrose.Portal.Views.Pages.LibraryPage;
using SPVPSDSP = Sucrose.Portal.Views.Pages.Setting.DonateSettingPage;
using SPVPSGSP = Sucrose.Portal.Views.Pages.Setting.GeneralSettingPage;
using SPVPSOSP = Sucrose.Portal.Views.Pages.Setting.OtherSettingPage;
using SPVPSP = Sucrose.Portal.Views.Pages.StorePage;
using SPVPSPESP = Sucrose.Portal.Views.Pages.Setting.PerformanceSettingPage;
using SPVPSPLSP = Sucrose.Portal.Views.Pages.Setting.PersonalSettingPage;
using SPVPSSSP = Sucrose.Portal.Views.Pages.Setting.SystemSettingPage;
using SPVPSWSP = Sucrose.Portal.Views.Pages.Setting.WallpaperSettingPage;
using SPVWMW = Sucrose.Portal.Views.Windows.MainWindow;
using SRHR = Sucrose.Resources.Helper.Resources;
using SSDHG = Sucrose.Shared.Dependency.Helper.Graphic;
using SSDHR = Sucrose.Shared.Dependency.Helper.Runtime;
using SSSHI = Sucrose.Shared.Space.Helper.Instance;
using SSSHS = Sucrose.Shared.Space.Helper.Security;
using SSSHW = Sucrose.Shared.Space.Helper.Watchdog;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Portal
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static bool HasError { get; set; } = true;

        // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(configure =>
            {
                configure.SetBasePath(AppContext.BaseDirectory);
            })
            .ConfigureServices((context, services) =>
            {
                // Navigation
                services.AddNavigationViewPageProvider();

                // App Host
                services.AddHostedService<SPSAHS>();

                // Main window container with navigation
                services.AddSingleton<SPSCIW, SPVWMW>();
                services.AddSingleton<SPVMWMWVM>();
                services.AddSingleton<IThemeService, ThemeService>();
                services.AddSingleton<ITaskBarService, TaskBarService>();
                services.AddSingleton<ISnackbarService, SnackbarService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddSingleton<IContentDialogService, ContentDialogService>();
                services.AddSingleton<SPSWPS>();

                // Top-level pages
                services.AddTransient<SPVPLP>();
                services.AddTransient<SPVMPLVM>();

                services.AddTransient<SPVPSP>();
                services.AddTransient<SPVMPSVM>();

                services.AddTransient<SPVPSDSP>();
                services.AddTransient<SPVMPDSVM>();

                services.AddTransient<SPVPSGSP>();
                services.AddTransient<SPVMPGSVM>();

                services.AddTransient<SPVPSOSP>();
                services.AddTransient<SPVMPOSVM>();

                services.AddTransient<SPVPSPESP>();
                services.AddTransient<SPVMPPESVM>();

                services.AddTransient<SPVPSPLSP>();
                services.AddTransient<SPVMPPLSVM>();

                services.AddTransient<SPVPSSSP>();
                services.AddTransient<SPVMPSSVM>();

                services.AddTransient<SPVPSWSP>();
                services.AddTransient<SPVMPWSVM>();

                // All other pages and view models
                services.AddTransientFromNamespace("Sucrose.Portal.Views", SHA.Assemble(SEAT.Executing));
                services.AddTransientFromNamespace("Sucrose.Portal.ViewModels", SHA.Assemble(SEAT.Executing));

                // Configuration
                services.Configure<SPMAC>(context.Configuration.GetSection(nameof(SPMAC)));
            })
            .Build();

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

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetService<T>() where T : class
        {
            return _host.Services.GetService(typeof(T)) as T ?? null;
        }

        /// <summary>
        /// Gets registered service.
        /// </summary>
        /// <typeparam name="T">Type of the service to get.</typeparam>
        /// <returns>Instance of the service or <see langword="null"/>.</returns>
        public static T GetRequiredService<T>() where T : class
        {
            return _host.Services.GetRequiredService<T>();
        }

        protected void Close()
        {
            _host.StopAsync().Wait();

            _host.Dispose();

            Environment.Exit(0);
            Current.Shutdown();
            Shutdown();
        }

        protected void Message(Exception Exception, bool Show)
        {
            if (HasError)
            {
                HasError = !Show;

                string Path = SMMI.PortalLogManager.LogFile();

                SSSHW.Start(SMMRA.Portal, Exception, Show, Path);

                if (Show)
                {
                    Close();
                }
            }
        }

        protected void Configure()
        {
            SSSHS.Apply();

            _host.Start();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Close();
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SRHR.SetLanguage(SMMG.Culture);

            ShutdownMode = ShutdownMode.OnLastWindowClose;

            if (SSSHI.Basic(SMMRM.Portal, SMMRA.Portal))
            {
                Configure();
            }
            else
            {
                Close();
            }
        }
    }
}