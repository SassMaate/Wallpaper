using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;
using SEWTT = Skylark.Enum.WindowsThemeType;
using SHC = Skylark.Helper.Culture;
using SMMG = Sucrose.Manager.Manage.General;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMMRA = Sucrose.Memory.Manage.Readonly.App;
using SMMRF = Sucrose.Memory.Manage.Readonly.Folder;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;
using SMMRW = Sucrose.Memory.Manage.Readonly.Watch;
using SRER = Sucrose.Resources.Extension.Resources;
using SRHR = Sucrose.Resources.Helper.Resources;
using SSCHA = Sucrose.Shared.Core.Helper.Architecture;
using SSCHF = Sucrose.Shared.Core.Helper.Framework;
using SSCHOS = Sucrose.Shared.Core.Helper.OperatingSystem;
using SSCHV = Sucrose.Shared.Core.Helper.Version;
using SSDHG = Sucrose.Shared.Dependency.Helper.Graphic;
using SSDHR = Sucrose.Shared.Dependency.Helper.Runtime;
using SSDMMG = Sucrose.Shared.Dependency.Manage.Manager.General;
using SSECCE = Skylark.Standard.Extension.Cryptology.CryptologyExtension;
using SSSEWE = Sucrose.Shared.Space.Extension.WatchException;
using SSSHE = Sucrose.Shared.Space.Helper.Exceptioner;
using SSSHP = Sucrose.Shared.Space.Helper.Processor;
using SSSHUE = Sucrose.Shared.Space.Helper.Unique;
using SSSHUR = Sucrose.Shared.Space.Helper.User;
using SSSHW = Sucrose.Shared.Space.Helper.Watchdog;
using SSSMTED = Sucrose.Shared.Space.Model.ThrowExceptionData;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;
using SSWHD = Sucrose.Shared.Watchdog.Helper.Dataset;
using SWHSI = Skylark.Wing.Helper.SystemInfo;
using SWMI = Sucrose.Watchdog.Manage.Internal;
using SWNM = Skylark.Wing.Native.Methods;
using SWVDEMB = Sucrose.Watchdog.View.DarkErrorMessageBox;
using SWVLEMB = Sucrose.Watchdog.View.LightErrorMessageBox;

namespace Sucrose.Watchdog
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static bool HasError { get; set; } = true;

        public App()
        {
            System.Windows.Media.RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;

            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic);

            System.Windows.Forms.Application.ThreadException += async (s, e) =>
            {
                Exception Exception = e.Exception;

                await SSWEW.Watch_ThreadException(Exception);

                SSWHD.Add("Exception Type", "Thread Exception");

                Message(Exception, true);
                //Close();
            };

            AppDomain.CurrentDomain.FirstChanceException += async (s, e) =>
            {
                Exception Exception = e.Exception;

                await SSWEW.Watch_FirstChanceException(Exception);

                SSWHD.Add("Exception Type", "First Chance Exception");

                //Message(Exception, false);
                //Close();
            };

            AppDomain.CurrentDomain.UnhandledException += async (s, e) =>
            {
                Exception Exception = (Exception)e.ExceptionObject;

                await SSWEW.Watch_GlobalUnhandledException(Exception);

                SSWHD.Add("Exception Type", "Global Unhand Exception");

                Message(Exception, true);
                //Close();
            };

            TaskScheduler.UnobservedTaskException += async (s, e) =>
            {
                e.SetObserved();

                Exception Exception = e.Exception;

                await SSWEW.Watch_UnobservedTaskException(Exception);

                SSWHD.Add("Exception Type", "Unobserved Task Exception");

                Message(Exception, false);
                //Close();
            };

            Current.DispatcherUnhandledException += async (s, e) =>
            {
                e.Handled = true;

                Exception Exception = e.Exception;

                await SSWEW.Watch_DispatcherUnhandledException(Exception);

                SSWHD.Add("Exception Type", "Dispatcher Unhandled Exception");

                Message(Exception, true);
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

        protected string Trim(string Value)
        {
            if (string.IsNullOrEmpty(Value))
            {
                return Value;
            }
            else if (Value.StartsWith("\"") && Value.EndsWith("\""))
            {
                return Value[1..^1];
            }
            else if (Value.StartsWith("\""))
            {
                return Value[1..];
            }
            else if (Value.EndsWith("\""))
            {
                return Value[..^1];
            }
            else
            {
                return Value;
            }
        }

        protected void Message(Exception Exception, bool Show)
        {
            if (HasError)
            {
                HasError = !Show;

                string Path = SMMI.WatchdogLogManager.LogFile();

                SSSHW.Start(SMMRA.Watchdog, Exception, Show, Path);

                if (Show)
                {
                    Close();
                }
            }
        }

        protected void ShowFallbackMessage(string Application, string Message, Exception FallbackException)
        {
            string DiagnosticInfo = $"Application: {Application}\n\n" +
                                   $"Error: {Message}\n\n" +
                                   $"WPF UI Error: {FallbackException.GetType().Name}\n" +
                                   $"{FallbackException.Message}\n\n" +
                                   "This may indicate missing Visual C++ Redistributable or .NET Desktop Runtime.\n" +
                                   "Please reinstall the application or contact support.";

            _ = SWNM.MessageBox(IntPtr.Zero, DiagnosticInfo, "Sucrose Watchdog - Critical Error", SWMI.MB_OK | SWMI.MB_ICONERROR | SWMI.MB_SYSTEMMODAL);
        }

        protected void Configure(string[] Args)
        {
            if (Args.Any())
            {
                bool Show = true;
                string Decode = SSECCE.BaseToText(Args.First());
                string[] Arguments = Decode.Split(SMMRG.ValueSeparatorChar);

                if (Arguments.Any() && Arguments.Count() == 4)
                {
                    Guid Id = Guid.NewGuid();
                    string Log = Arguments[3];
                    string Text = string.Empty;
                    string Source = string.Empty;
                    string User = SSSHUR.GetName();
                    string Model = SSSHUR.GetModel();
                    string Application = Arguments[0];
                    string RawException = Arguments[1];
                    bool.TryParse(Arguments[2], out Show);
                    Guid AppId = SSSHUE.Generate(Application);
                    string Manufacturer = SSSHUR.GetManufacturer();
                    Exception Exception = SSSEWE.Convert(RawException);
                    CultureInfo Culture = new(SWNM.GetUserDefaultUILanguage());
                    string Message = SSSHE.GetMessage(Exception, SRER.GetValue("Watchdog", "ErrorEmpty"), SMMRG.ExceptionSplit);

                    foreach (string Key in Exception.Data.Keys)
                    {
                        string DataKey = Trim(Key);
                        string DataValue = Trim(Exception.Data[Key].ToString());

                        if (DataKey == SMMRW.Text)
                        {
                            Text = DataValue;
                        }
                        else if (DataKey == SMMRW.Source)
                        {
                            Source = DataValue;
                        }
                    }

                    SSSMTED ThrowData = new()
                    {
                        Id = Id,
                        AppId = AppId,
                        UserName = User,
                        DeviceModel = Model,
                        AppName = Application,
                        CultureName = Culture.Name,
                        AppVersion = SSCHV.GetText(),
                        IsServer = SSCHOS.GetServer(),
                        AppFramework = SSCHF.GetName(),
                        ManufacturerBrand = Manufacturer,
                        AppArchitecture = SSCHA.GetText(),
                        OperatingSystem = SSCHOS.GetText(),
                        CultureDisplay = Culture.NativeName,
                        Exception = JObject.Parse(RawException),
                        IsWorkstation = SSCHOS.GetWorkstation(),
                        OperatingSystemBuild = SSCHV.GetOSText(),
                        CultureCode = SMMG.Culture.ToUpperInvariant(),
                        Sid = SSSHUE.Generate($"{User}-{Model}-{Manufacturer}"),
                        ProcessArchitecture = SSCHOS.GetProcessArchitectureText(),
                        ProcessorArchitecture = SSCHOS.GetProcessorArchitecture(),
                        OperatingSystemArchitecture = SWHSI.GetSystemInfoArchitecture(),
                    };

                    SSSHW.Write(Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.Report, $"{Id}.json"), JsonConvert.SerializeObject(ThrowData, Formatting.Indented));

                    if (Application != SMMRA.Watchdog)
                    {
                        SSSHP.Kill(Application);
                    }

                    if (Show)
                    {
                        try
                        {
                            switch (SSDMMG.ThemeType)
                            {
                                case SEWTT.Dark:
                                    SWVDEMB DarkMessageBox = new(RawException, Message, Log, Source, Text);
                                    DarkMessageBox.ShowDialog();
                                    break;
                                default:
                                    SWVLEMB LightMessageBox = new(RawException, Message, Log, Source, Text);
                                    LightMessageBox.ShowDialog();
                                    break;
                            }
                        }
                        catch (Exception FallbackException)
                        {
                            SSWHD.Add("Exception Type", "Fallback Exception");

                            this.Message(FallbackException, false);

                            ShowFallbackMessage(Application, Message, FallbackException);
                        }
                    }

                    Close();
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

            Close();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SRHR.SetLanguage(SMMG.Culture);

            ShutdownMode = ShutdownMode.OnLastWindowClose;

            Configure(e.Args);
        }
    }
}