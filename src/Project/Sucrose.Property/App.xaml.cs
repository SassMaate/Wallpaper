using HandyControl.Tools;
using System.Globalization;
using System.IO;
using System.Windows;
using Application = System.Windows.Application;
using SHC = Skylark.Helper.Culture;
using SMMG = Sucrose.Manager.Manage.General;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMML = Sucrose.Manager.Manage.Library;
using SMMRA = Sucrose.Memory.Manage.Readonly.App;
using SMMRC = Sucrose.Memory.Manage.Readonly.Content;
using SMMRF = Sucrose.Memory.Manage.Readonly.Folder;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRM = Sucrose.Memory.Manage.Readonly.Mutex;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;
using SPMI = Sucrose.Property.Manage.Internal;
using SPVMW = Sucrose.Property.View.MainWindow;
using SRHR = Sucrose.Resources.Helper.Resources;
using SSDEET = Sucrose.Shared.Dependency.Enum.EngineType;
using SSDEPT = Sucrose.Shared.Dependency.Enum.PropertiesType;
using SSDEWT = Sucrose.Shared.Dependency.Enum.WallpaperType;
using SSDHG = Sucrose.Shared.Dependency.Helper.Graphic;
using SSDHR = Sucrose.Shared.Dependency.Helper.Runtime;
using SSDMME = Sucrose.Shared.Dependency.Manage.Manager.Engine;
using SSSHF = Sucrose.Shared.Space.Helper.Filing;
using SSSHI = Sucrose.Shared.Space.Helper.Instance;
using SSSHW = Sucrose.Shared.Space.Helper.Watchdog;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;
using SSTHP = Sucrose.Shared.Theme.Helper.Properties;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Property
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
            ConfigHelper.Instance.SetLang(SMMG.Culture);

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

                string Path = SMMI.PropertyLogManager.LogFile();

                SSSHW.Start(SMMRA.Property, Exception, Path);

                Close();
            }
        }

        protected void Configure(string[] Args)
        {
            SPMI.LibraryLocation = SMML.Location;
            SPMI.LibrarySelected = SMML.Selected;

            if (Args.Any())
            {
                string[] Arguments = Args.First().Split(SMMRG.ValueSeparatorChar);

                if (Arguments.Any() && Arguments.Count() == 1)
                {
                    SPMI.LibrarySelected = Arguments.First();
                }
            }

            if (SMMI.LibrarySettingManager.CheckFile() && !string.IsNullOrEmpty(SPMI.LibrarySelected))
            {
                SPMI.Path = Path.Combine(SPMI.LibraryLocation, SPMI.LibrarySelected);

                SPMI.InfoPath = Path.Combine(SPMI.Path, SMMRC.SucroseInfo);

                if (File.Exists(SPMI.InfoPath) && SSTHI.ReadCheck(SPMI.InfoPath))
                {
                    SPMI.Info = SSTHI.ReadJson(SPMI.InfoPath);

                    SPMI.PropertiesPath = Path.Combine(SPMI.Path, SMMRC.SucroseProperties);

                    if (SPMI.Info.Type is SSDEWT.Gif or SSDEWT.Web or SSDEWT.Video or SSDEWT.YouTube)
                    {
                        if (SPMI.Info.Type == SSDEWT.Web && File.Exists(SPMI.PropertiesPath))
                        {
                            SPMI.PropertiesType = SSDEPT.Base;
                        }
                        else if (SPMI.Info.Type is SSDEWT.Gif or SSDEWT.Video or SSDEWT.YouTube)
                        {
                            if (SPMI.Info.Type is SSDEWT.Gif or SSDEWT.Video)
                            {
                                if (SPMI.Info.Type == SSDEWT.Gif)
                                {
                                    if (SSDMME.Gif == SSDEET.MpvPlayerLive && (File.Exists(SPMI.PropertiesPath) || File.Exists(Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.MpvPlayer, SMMRC.SucroseProperties))))
                                    {
                                        SPMI.PropertiesType = SSDEPT.MpvPlayer;

                                        if (!File.Exists(SPMI.PropertiesPath))
                                        {
                                            SPMI.PropertiesPath = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.MpvPlayer, SMMRC.SucroseProperties);
                                        }
                                    }
                                    else if (SSDMME.Gif == SSDEET.CefSharpLive && File.Exists(Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.CefSharp, SMMRC.SucroseProperties)))
                                    {
                                        SPMI.PropertiesType = SSDEPT.CefSharp;

                                        SPMI.PropertiesPath = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.CefSharp, SMMRC.SucroseProperties);
                                    }
                                    else if (SSDMME.Gif == SSDEET.WebViewLive && File.Exists(Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.WebView2, SMMRC.SucroseProperties)))
                                    {
                                        SPMI.PropertiesType = SSDEPT.WebView;

                                        SPMI.PropertiesPath = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.WebView2, SMMRC.SucroseProperties);
                                    }
                                }
                                else if (SPMI.Info.Type == SSDEWT.Video)
                                {
                                    if (SSDMME.Video == SSDEET.MpvPlayerLive && (File.Exists(SPMI.PropertiesPath) || File.Exists(Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.MpvPlayer, SMMRC.SucroseProperties))))
                                    {
                                        SPMI.PropertiesType = SSDEPT.MpvPlayer;

                                        if (!File.Exists(SPMI.PropertiesPath))
                                        {
                                            SPMI.PropertiesPath = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.MpvPlayer, SMMRC.SucroseProperties);
                                        }
                                    }
                                    else if (SSDMME.Video == SSDEET.CefSharpLive && File.Exists(Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.CefSharp, SMMRC.SucroseProperties)))
                                    {
                                        SPMI.PropertiesType = SSDEPT.CefSharp;

                                        SPMI.PropertiesPath = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.CefSharp, SMMRC.SucroseProperties);
                                    }
                                    else if (SSDMME.Video == SSDEET.WebViewLive && File.Exists(Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.WebView2, SMMRC.SucroseProperties)))
                                    {
                                        SPMI.PropertiesType = SSDEPT.WebView;

                                        SPMI.PropertiesPath = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.WebView2, SMMRC.SucroseProperties);
                                    }
                                }
                            }
                            else if (SPMI.Info.Type == SSDEWT.YouTube)
                            {
                                if (SSDMME.YouTube == SSDEET.CefSharpLive && File.Exists(Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.CefSharp, SMMRC.SucroseProperties)))
                                {
                                    SPMI.PropertiesType = SSDEPT.CefSharp;

                                    SPMI.PropertiesPath = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.CefSharp, SMMRC.SucroseProperties);
                                }
                                else if (SSDMME.YouTube == SSDEET.WebViewLive && File.Exists(Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.WebView2, SMMRC.SucroseProperties)))
                                {
                                    SPMI.PropertiesType = SSDEPT.WebView;

                                    SPMI.PropertiesPath = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.WebView2, SMMRC.SucroseProperties);
                                }
                            }
                        }

                        if (File.Exists(SPMI.PropertiesPath))
                        {
                            SPMI.PropertiesCache = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.Properties);
                            SPMI.PropertiesFile = Path.Combine(SPMI.PropertiesCache, $"{SPMI.LibrarySelected}{SPMI.PropertiesType}");
                            SPMI.WatcherFile = Path.Combine(SPMI.PropertiesCache, $"*.{SPMI.LibrarySelected}{SPMI.PropertiesType}");

                            if (!Directory.Exists(SPMI.PropertiesCache))
                            {
                                Directory.CreateDirectory(SPMI.PropertiesCache);
                            }

                            if (!File.Exists(SPMI.PropertiesFile))
                            {
                                SSSHF.CopyBuffer(SPMI.PropertiesPath, SPMI.PropertiesFile);
                            }

                            try
                            {
                                SPMI.Properties = SSTHP.ReadJson(SPMI.PropertiesFile);
                            }
                            catch (NotSupportedException Exception)
                            {
                                SSSHF.Delete(SPMI.PropertiesFile);

                                throw new NotSupportedException(Exception.Message);
                            }
                            catch (Exception Exception)
                            {
                                SSSHF.Delete(SPMI.PropertiesFile);

                                throw new Exception(Exception.Message, Exception.InnerException);
                            }

                            SPVMW MainWindow = new();
                            MainWindow.ShowDialog();
                        }
                    }
                }
            }

            Close();
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

            if (SSSHI.Basic(SMMRM.Property, SMMRA.Property))
            {
                Configure(e.Args);
            }
            else
            {
                Close();
            }
        }
    }
}