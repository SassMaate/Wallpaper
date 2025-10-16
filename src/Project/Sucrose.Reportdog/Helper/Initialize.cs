using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Net.Http;
using SMMB = Sucrose.Manager.Manage.Backgroundog;
using SMMC = Sucrose.Manager.Manage.Cycling;
using SMMD = Sucrose.Manager.Manage.Donate;
using SMME = Sucrose.Manager.Manage.Engine;
using SMMG = Sucrose.Manager.Manage.General;
using SMMH = Sucrose.Manager.Manage.Hook;
using SMML = Sucrose.Manager.Manage.Library;
using SMMP = Sucrose.Manager.Manage.Portal;
using SMMRS = Sucrose.Memory.Manage.Readonly.Soferity;
using SMMRU = Sucrose.Memory.Manage.Readonly.Url;
using SMMU = Sucrose.Manager.Manage.Update;
using SRMI = Sucrose.Reportdog.Manage.Internal;
using SSCHA = Sucrose.Shared.Core.Helper.Architecture;
using SSCHF = Sucrose.Shared.Core.Helper.Framework;
using SSCHM = Sucrose.Shared.Core.Helper.Memory;
using SSCHOS = Sucrose.Shared.Core.Helper.OperatingSystem;
using SSCHV = Sucrose.Shared.Core.Helper.Version;
using SSCMMU = Sucrose.Shared.Core.Manage.Manager.Update;
using SSDMI = Sucrose.Shared.Dependency.Manage.Internal;
using SSDMMB = Sucrose.Shared.Dependency.Manage.Manager.Backgroundog;
using SSDMMC = Sucrose.Shared.Dependency.Manage.Manager.Cycling;
using SSDMME = Sucrose.Shared.Dependency.Manage.Manager.Engine;
using SSDMMG = Sucrose.Shared.Dependency.Manage.Manager.General;
using SSDMMP = Sucrose.Shared.Dependency.Manage.Manager.Portal;
using SSDMMU = Sucrose.Shared.Dependency.Manage.Manager.Update;
using SSSHF = Sucrose.Shared.Space.Helper.Filing;
using SSSHN = Sucrose.Shared.Space.Helper.Network;
using SSSHU = Sucrose.Shared.Space.Helper.User;
using SSSHW = Sucrose.Shared.Space.Helper.Watchdog;
using SSSMATD = Sucrose.Shared.Space.Model.AnalyticTelemetryData;
using SSSMOTD = Sucrose.Shared.Space.Model.OnlineTelemetryData;
using SSSMTED = Sucrose.Shared.Space.Model.ThrowExceptionData;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;
using SWHSI = Skylark.Wing.Helper.SystemInfo;
using SWNM = Skylark.Wing.Native.Methods;

namespace Sucrose.Reportdog.Helper
{
    internal class Initialize : IDisposable
    {
        public async void Stop()
        {
            if (SRMI.Watcher != null)
            {
                SRMI.Watcher.EnableRaisingEvents = false;
                SRMI.AnalyticTimer?.Dispose();
                SRMI.OnlineTimer?.Dispose();
                SRMI.Watcher?.Dispose();
                SRMI.Watcher = null;
            }

            await Task.CompletedTask;
        }

        public async void Start()
        {
            if (SRMI.Watcher == null)
            {
                if (!Directory.Exists(SRMI.Source))
                {
                    Directory.CreateDirectory(SRMI.Source);
                }
                else
                {
                    string[] Files = Directory.GetFiles(SRMI.Source, "*.*", SearchOption.TopDirectoryOnly);

                    foreach (string Record in Files)
                    {
                        await SendThrow(Record);

                        await Task.Delay(1000);
                    }
                }

                SRMI.Watcher = new()
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.CreationTime,
                    Path = SRMI.Source,
                    Filter = "*.*"
                };

                SRMI.Watcher.Created += async (s, e) =>
                {
                    await SendThrow(e.FullPath);
                };

                TimerCallback CallbackAnalytic = AnalyticTimer_Callback;
                SRMI.AnalyticTimer = new(CallbackAnalytic, null, SRMI.AnalyticTime, SRMI.AnalyticTime);

                TimerCallback CallbackOnline = OnlineTimer_Callback;
                SRMI.OnlineTimer = new(CallbackOnline, null, SRMI.OnlineTime, SRMI.OnlineTime);

                SRMI.Watcher.EnableRaisingEvents = true;

                await SendOnline(true);

                await SendAnalytic();
            }
        }

        private static async Task SendAnalytic()
        {
            try
            {
                if (SMMG.TelemetryData)
                {
                    if (await SSSHN.GetHostEntryAsync())
                    {
                        CultureInfo Culture = new(SWNM.GetUserDefaultUILanguage());

                        SSSMATD AnalyticData = new()
                        {
                            AppExit = SMMG.AppExit,
                            UpdateAuto = SMMU.Auto,
                            LibraryMove = SMML.Move,
                            CultureName = Culture.Name,
                            UserName = SSSHU.GetName(),
                            CyclingActive = SMMC.Active,
                            EngineGif = $"{SSDMME.Gif}",
                            EngineUrl = $"{SSDMME.Url}",
                            EngineWeb = $"{SSDMME.Web}",
                            AppVersion = SSCHV.GetText(),
                            AppVisible = SMMG.AppVisible,
                            RunStartup = SMMG.RunStartup,
                            StoreAdult = SMMP.StoreAdult,
                            StoreStart = SMME.StoreStart,
                            IsServer = SSCHOS.GetServer(),
                            AppFramework = SSCHF.GetName(),
                            DeviceModel = SSSHU.GetModel(),
                            EngineVideo = $"{SSDMME.Video}",
                            LibraryLocation = SMML.Location,
                            IsException = SMMG.ExceptionData,
                            IsTelemetry = SMMG.TelemetryData,
                            LibraryStart = SMME.LibraryStart,
                            StorePreview = SMMP.StorePreview,
                            VolumeSilent = SMME.VolumeSilent,
                            AppArchitecture = SSCHA.GetText(),
                            ThemeType = $"{SSDMMG.ThemeType}",
                            DeveloperMode = SMME.DeveloperMode,
                            DeveloperPort = SMME.DeveloperPort,
                            OperatingSystem = SSCHOS.GetText(),
                            StoreDuration = SMMP.StoreDuration,
                            WallpaperLoop = SMME.WallpaperLoop,
                            CultureDisplay = Culture.NativeName,
                            EngineYouTube = $"{SSDMME.YouTube}",
                            DiscordConnect = SMMH.DiscordConnect,
                            GraphicAdapter = SMMB.GraphicAdapter,
                            LibraryPreview = SMMP.LibraryPreview,
                            NetworkAdapter = SMMB.NetworkAdapter,
                            TotalMemory = SSCHM.GetTotalMemory(),
                            EngineInputType = $"{SMME.InputType}",
                            StretchType = $"{SSDMME.StretchType}",
                            UpdateAutoType = $"{SSDMMU.AutoType}",
                            BackgroundImage = SMMP.BackgroundImage,
                            EngineInputDesktop = SMME.InputDesktop,
                            StorePagination = SMMP.StorePagination,
                            UserIdentifier = SSSHU.GetIdentifier(),
                            WallpaperVolume = SMME.WallpaperVolume,
                            EngineScreenType = $"{SMME.ScreenType}",
                            IsWorkstation = SSCHOS.GetWorkstation(),
                            AdvertisingDelay = SMMD.AdvertisingDelay,
                            EngineVolumeDesktop = SMME.VolumeDesktop,
                            NumberOfCores = SSSHU.GetNumberOfCores(),
                            OperatingSystemBuild = SSCHV.GetOSText(),
                            UserIdentifying = SSSHU.GetIdentifying(),
                            WallpaperShuffle = SMME.WallpaperShuffle,
                            AdvertisingMenuVisible = SMMD.MenuVisible,
                            LibraryDeleteConfirm = SMML.DeleteConfirm,
                            LibraryDeleteCorrupt = SMML.DeleteCorrupt,
                            UpdateModuleType = $"{SSDMMU.ModuleType}",
                            UpdateServerType = $"{SSDMMU.ServerType}",
                            BackgroundOpacity = SMMP.BackgroundOpacity,
                            AdvertisingActive = SMMD.AdvertisingActive,
                            LibraryPagination = SMMP.LibraryPagination,
                            CyclingTransitionTime = SMMC.TransitionTime,
                            EngineApplication = $"{SSDMME.Application}",
                            ManufacturerBrand = SSSHU.GetManufacturer(),
                            UpdateChannelType = $"{SSCMMU.ChannelType}",
                            PerformanceCounter = SMMB.PerformanceCounter,
                            CultureCode = SMMG.Culture.ToUpperInvariant(),
                            InputModuleType = $"{SSDMME.InputModuleType}",
                            LockPerformance = $"{SSDMMB.LockPerformance}",
                            StoreServerType = $"{SSDMMP.StoreServerType}",
                            DisplayScreenType = $"{SMME.DisplayScreenType}",
                            FocusPerformance = $"{SSDMMB.FocusPerformance}",
                            SleepPerformance = $"{SSDMMB.SleepPerformance}",
                            UpdateExtensionType = $"{SSCMMU.ExtensionType}",
                            BackgroundStretch = $"{SSDMMP.BackgroundStretch}",
                            CommunicationType = $"{SSDMMB.CommunicationType}",
                            MemoryPerformance = $"{SSDMMB.MemoryPerformance}",
                            RemotePerformance = $"{SSDMMB.RemotePerformance}",
                            CyclingTransitionType = $"{SSDMMC.TransitionType}",
                            BatteryPerformance = $"{SSDMMB.BatteryPerformance}",
                            ConsolePerformance = $"{SSDMMB.ConsolePerformance}",
                            GraphicPerformance = $"{SSDMMB.GraphicPerformance}",
                            NetworkPerformance = $"{SSDMMB.NetworkPerformance}",
                            NumberOfProcessors = SSCHOS.GetNumberOfProcessors(),
                            PausePerformance = $"{SSDMMB.PausePerformanceType}",
                            SessionPerformance = $"{SSDMMB.SessionPerformance}",
                            VirtualPerformance = $"{SSDMMB.VirtualPerformance}",
                            GraphicAdapters = string.Join(",", SSSHU.GetGraphic()),
                            NetworkAdapters = string.Join(",", SSSHU.GetNetwork()),
                            VolumeSilentSensitivity = SMME.VolumeSilentSensitivity,
                            ProcessorPerformance = $"{SSDMMB.ProcessorPerformance}",
                            DeviceProcessors = string.Join(",", SSSHU.GetProcessor()),
                            FullScreenPerformance = $"{SSDMMB.FullScreenPerformance}",
                            ProcessArchitecture = SSCHOS.GetProcessArchitectureText(),
                            ProcessorArchitecture = SSCHOS.GetProcessorArchitecture(),
                            ScreenSaverPerformance = $"{SSDMMB.ScreenSaverPerformance}",
                            BatterySaverPerformance = $"{SSDMMB.BatterySaverPerformance}",
                            OperatingSystemArchitecture = SWHSI.GetSystemInfoArchitecture()
                        };

                        StringContent Content = new(JsonConvert.SerializeObject(AnalyticData, Formatting.Indented), SMMRS.Encoding, SMMRS.ApplicationJson);

                        HttpResponseMessage Response = await SSDMI.Client.PostAsync($"{SMMRU.Soferity}/{SMMRS.Version}/{SMMRS.Telemetry}/{SMMRS.Analytic}/{SSSHU.GetGuid()}", Content);

                        Response.EnsureSuccessStatusCode();

                        if (Response.IsSuccessStatusCode)
                        {
                            SRMI.AnalyticTimer.Change(Timeout.Infinite, Timeout.Infinite);

                            SRMI.AnalyticTimer.Dispose();
                        }
                    }
                }
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        private static async Task SendOnline(bool First)
        {
            try
            {
                if (SMMG.TelemetryData && await SSSHN.GetHostEntryAsync())
                {
                    SSSMOTD OnlineData = new()
                    {
                        AppVersion = SSCHV.GetText(),
                        Time = First ? 1 : Convert.ToInt32(SRMI.OnlineTime.TotalSeconds)
                    };

                    StringContent Content = new(JsonConvert.SerializeObject(OnlineData, Formatting.Indented), SMMRS.Encoding, SMMRS.ApplicationJson);

                    HttpResponseMessage Response = await SSDMI.Client.PostAsync($"{SMMRU.Soferity}/{SMMRS.Version}/{SMMRS.Telemetry}/{SMMRS.Online}/{SSSHU.GetGuid()}", Content);

                    Response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        private static async Task SendThrow(string Path)
        {
            try
            {
                if (SMMG.ExceptionData && await SSSHN.GetHostEntryAsync())
                {
                    await Task.Delay(50);

                    if (File.Exists(Path))
                    {
                        SSSMTED ThrowData = JsonConvert.DeserializeObject<SSSMTED>(SSSHW.Read(Path));

                        StringContent Content = new(JsonConvert.SerializeObject(ThrowData, Formatting.Indented), SMMRS.Encoding, SMMRS.ApplicationJson);

                        HttpResponseMessage Response = await SSDMI.Client.PostAsync($"{SMMRU.Soferity}/{SMMRS.Version}/{SMMRS.Exception}/{SMMRS.Throw}/{SSSHU.GetGuid()}", Content);

                        Response.EnsureSuccessStatusCode();

                        if (Response.IsSuccessStatusCode)
                        {
                            await Task.Delay(50);

                            SSSHF.Delete(Path);
                        }
                    }
                }
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        private async void OnlineTimer_Callback(object State)
        {
            await SendOnline(false);
        }

        private async void AnalyticTimer_Callback(object State)
        {
            await SendAnalytic();
        }

        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}