using Skylark.Enum;
using Skylark.Standard.Extension.Storage;
using System.Diagnostics;
using SBEG = Sucrose.Backgroundog.Extension.Graphic;
using SBMI = Sucrose.Backgroundog.Manage.Internal;
using SMMA = Sucrose.Manager.Manage.Aurora;
using SMMB = Sucrose.Manager.Manage.Backgroundog;
using SMMCB = Sucrose.Memory.Manage.Constant.Backgroundog;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMMRA = Sucrose.Memory.Manage.Readonly.App;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRP = Sucrose.Memory.Manage.Readonly.Process;
using SSDECPT = Sucrose.Shared.Dependency.Enum.CategoryPerformanceType;
using SSDENPT = Sucrose.Shared.Dependency.Enum.NetworkPerformanceType;
using SSDEPPT = Sucrose.Shared.Dependency.Enum.PausePerformanceType;
using SSDEPT = Sucrose.Shared.Dependency.Enum.PerformanceType;
using SSDMMB = Sucrose.Shared.Dependency.Manage.Manager.Backgroundog;
using SSLHK = Sucrose.Shared.Live.Helper.Kill;
using SSSEL = Sucrose.Shared.Space.Extension.Lifecycle;
using SSSHL = Sucrose.Shared.Space.Helper.Live;
using SSSHM = Sucrose.Shared.Space.Helper.Management;
using SSSHP = Sucrose.Shared.Space.Helper.Processor;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Backgroundog.Helper
{
    internal static class Performance
    {
        public static async Task Start()
        {
            if (await LockPerformance())
            {
                return;
            }

            if (await FocusPerformance())
            {
                return;
            }

            if (await SleepPerformance())
            {
                return;
            }

            if (await MemoryPerformance())
            {
                return;
            }

            if (await RemotePerformance())
            {
                return;
            }

            if (await BatteryPerformance())
            {
                return;
            }

            if (await ConsolePerformance())
            {
                return;
            }

            if (await GraphicPerformance())
            {
                return;
            }

            if (await NetworkPerformance())
            {
                return;
            }

            if (await SessionPerformance())
            {
                return;
            }

            if (await VirtualPerformance())
            {
                return;
            }
            if (await ProcessorPerformance())
            {
                return;
            }

            if (await FullScreenPerformance())
            {
                return;
            }

            if (await ScreenSaverPerformance())
            {
                return;
            }

            if (await BatterySaverPerformance())
            {
                return;
            }

            await Task.CompletedTask;
        }

        private static async void Lifecycle()
        {
            if (SBMI.Performance == SSDEPT.Close)
            {
                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.ClosePerformance, true);
                SSLHK.Stop();
            }
            else
            {
                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.PausePerformance, true);
                SBMI.PausePerformance = SSDMMB.PausePerformanceType;
                SBMI.Live = SSSHL.Get();

                if (SBMI.Live != null && !SBMI.Live.HasExited)
                {
                    if (SBMI.PausePerformance == SSDEPPT.Heavy)
                    {
                        SSSEL.Suspend(SBMI.Live);

                        if (SMMRA.WebViewLive.Contains(SBMI.Live.ProcessName) || SMMRA.CefSharpLive.Contains(SBMI.Live.ProcessName))
                        {
                            try
                            {
                                Process[] Processes = Process.GetProcesses();

                                Processes
                                    .Where(Process => (Process.ProcessName.Contains(SMMRP.WebViewName) || Process.ProcessName.Contains(SMMRP.CefSharpName)) && SSSHM.GetCommandLine(Process).Contains(SMMRG.AppName))
                                    .ToList()
                                    .ForEach(Process =>
                                    {
                                        SSSEL.Suspend(Process.MainWindowHandle);
                                        SSSEL.Suspend(Process.Handle);
                                    });
                            }
                            catch (Exception Exception)
                            {
                                await SSWEW.Watch_CatchException(Exception);
                            }
                        }
                    }
                }

                if (SBMI.PausePerformance == SSDEPPT.Heavy)
                {
                    if (!string.IsNullOrEmpty(SMMA.AppProcessName))
                    {
                        SBMI.Apps = SSSHP.Gets(SMMA.AppProcessName);

                        if (SBMI.Apps != null)
                        {
                            foreach (Process App in SBMI.Apps)
                            {
                                SBMI.App = App;

                                if (App != null && !App.HasExited)
                                {
                                    try
                                    {
                                        SSSEL.Suspend(App.MainWindowHandle);
                                        SSSEL.Suspend(App.Handle);
                                    }
                                    catch (Exception Exception)
                                    {
                                        await SSWEW.Watch_CatchException(Exception);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static async Task<bool> LockPerformance()
        {
            if (SSDMMB.LockPerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 0;
                SSDEPT Performance = SSDMMB.LockPerformance;

                while (SBMI.WindowsLock && SSDMMB.LockPerformance == Performance)
                {
                    if (Count >= MaxCount)
                    {
                        SBMI.Performance = SSDMMB.LockPerformance;
                        SBMI.CategoryPerformance = SSDECPT.Lock;
                        SBMI.Condition = true;
                        Lifecycle();

                        return true;
                    }
                    else
                    {
                        Count++;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private static async Task<bool> FocusPerformance()
        {
            if (SSDMMB.FocusPerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 5;
                SSDEPT Performance = SSDMMB.FocusPerformance;

                while (!SBMI.FocusDesktop && SSDMMB.FocusPerformance == Performance)
                {
                    if (Count >= MaxCount)
                    {
                        SBMI.Performance = SSDMMB.FocusPerformance;
                        SBMI.CategoryPerformance = SSDECPT.Focus;
                        SBMI.Condition = true;
                        Lifecycle();

                        return true;
                    }
                    else
                    {
                        Count++;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private static async Task<bool> SleepPerformance()
        {
            if (SSDMMB.SleepPerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 0;
                SSDEPT Performance = SSDMMB.SleepPerformance;

                while (SBMI.WindowsSleep && SSDMMB.SleepPerformance == Performance)
                {
                    if (Count >= MaxCount)
                    {
                        SBMI.Performance = SSDMMB.SleepPerformance;
                        SBMI.CategoryPerformance = SSDECPT.Sleep;
                        SBMI.Condition = true;
                        Lifecycle();

                        return true;
                    }
                    else
                    {
                        Count++;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private static async Task<bool> MemoryPerformance()
        {
            if (SSDMMB.MemoryPerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 5;
                SSDEPT Performance = SSDMMB.MemoryPerformance;

                while (SBMI.MemoryData.State && SMMB.MemoryUsage > 0 && SBMI.MemoryData.MemoryLoad >= SMMB.MemoryUsage && SSDMMB.MemoryPerformance == Performance)
                {
                    if (Count >= MaxCount)
                    {
                        SBMI.Performance = SSDMMB.MemoryPerformance;
                        SBMI.CategoryPerformance = SSDECPT.Memory;
                        SBMI.Condition = true;
                        Lifecycle();

                        return true;
                    }
                    else
                    {
                        Count++;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private static async Task<bool> RemotePerformance()
        {
            if (SSDMMB.RemotePerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 5;
                SSDEPT Performance = SSDMMB.RemotePerformance;

                while (SBMI.RemoteDesktop && SSDMMB.RemotePerformance == Performance)
                {
                    if (Count >= MaxCount)
                    {
                        SBMI.Performance = SSDMMB.RemotePerformance;
                        SBMI.CategoryPerformance = SSDECPT.Remote;
                        SBMI.Condition = true;
                        Lifecycle();

                        return true;
                    }
                    else
                    {
                        Count++;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private static async Task<bool> BatteryPerformance()
        {
            if (SSDMMB.BatteryPerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 5;
                SSDEPT Performance = SSDMMB.BatteryPerformance;

                while (SBMI.BatteryData.State && (SBMI.BatteryData.PowerLineStatus != PowerLineStatus.Online || SBMI.BatteryData.ACPowerStatus != "Online") && SMMB.BatteryUsage > 0 && SBMI.BatteryData.ChargeLevel <= SMMB.BatteryUsage && SSDMMB.BatteryPerformance == Performance)
                {
                    if (Count >= MaxCount)
                    {
                        SBMI.Performance = SSDMMB.BatteryPerformance;
                        SBMI.CategoryPerformance = SSDECPT.Battery;
                        SBMI.Condition = true;
                        Lifecycle();

                        return true;
                    }
                    else
                    {
                        Count++;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private static async Task<bool> ConsolePerformance()
        {
            if (SSDMMB.ConsolePerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 0;
                SSDEPT Performance = SSDMMB.ConsolePerformance;

                while (!SBMI.WindowsConsole && SSDMMB.ConsolePerformance == Performance)
                {
                    if (Count >= MaxCount)
                    {
                        SBMI.Performance = SSDMMB.ConsolePerformance;
                        SBMI.CategoryPerformance = SSDECPT.Console;
                        SBMI.Condition = true;
                        Lifecycle();

                        return true;
                    }
                    else
                    {
                        Count++;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private static async Task<bool> GraphicPerformance()
        {
            if (SSDMMB.GraphicPerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 5;
                SSDEPT Performance = SSDMMB.GraphicPerformance;

                while (SBMI.GraphicData.State && SMMB.GraphicUsage > 0 && SBEG.Performance() && SSDMMB.GraphicPerformance == Performance)
                {
                    if (Count >= MaxCount)
                    {
                        SBMI.Performance = SSDMMB.GraphicPerformance;
                        SBMI.CategoryPerformance = SSDECPT.Graphic;
                        SBMI.Condition = true;
                        Lifecycle();

                        return true;
                    }
                    else
                    {
                        Count++;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private static async Task<bool> NetworkPerformance()
        {
            if (SSDMMB.NetworkPerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 5;
                SSDEPT Performance = SSDMMB.NetworkPerformance;

                while (SBMI.NetworkData.State && (SMMB.PingValue > 0 || SMMB.UploadValue > 0 || SMMB.DownloadValue > 0) && SSDMMB.NetworkPerformance == Performance)
                {
                    if (SBMI.NetworkData.Ping >= SMMB.PingValue)
                    {
                        if (Count >= MaxCount)
                        {
                            SBMI.Performance = SSDMMB.NetworkPerformance;
                            SBMI.CategoryPerformance = SSDECPT.Network;
                            SBMI.NetworkPerformance = SSDENPT.Ping;
                            SBMI.Condition = true;
                            Lifecycle();

                            return true;
                        }
                        else
                        {
                            Count++;
                        }
                    }
                    else if (SBMI.NetworkData.Upload >= StorageExtension.Convert(SMMB.UploadValue, SMMB.UploadType, StorageType.Byte, ModeStorageType.Palila))
                    {
                        if (Count >= MaxCount)
                        {
                            SBMI.Performance = SSDMMB.NetworkPerformance;
                            SBMI.CategoryPerformance = SSDECPT.Network;
                            SBMI.NetworkPerformance = SSDENPT.Upload;
                            SBMI.Condition = true;
                            Lifecycle();

                            return true;
                        }
                        else
                        {
                            Count++;
                        }
                    }
                    else if (SBMI.NetworkData.Download >= StorageExtension.Convert(SMMB.DownloadValue, SMMB.DownloadType, StorageType.Byte, ModeStorageType.Palila))
                    {
                        if (Count >= MaxCount)
                        {
                            SBMI.Performance = SSDMMB.NetworkPerformance;
                            SBMI.CategoryPerformance = SSDECPT.Network;
                            SBMI.NetworkPerformance = SSDENPT.Download;
                            SBMI.Condition = true;
                            Lifecycle();

                            return true;
                        }
                        else
                        {
                            Count++;
                        }
                    }
                    else
                    {
                        break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private static async Task<bool> SessionPerformance()
        {
            if (SSDMMB.SessionPerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 0;
                SSDEPT Performance = SSDMMB.SessionPerformance;

                while (!SBMI.WindowsSession && SSDMMB.SessionPerformance == Performance)
                {
                    if (Count >= MaxCount)
                    {
                        SBMI.Performance = SSDMMB.SessionPerformance;
                        SBMI.CategoryPerformance = SSDECPT.Session;
                        SBMI.Condition = true;
                        Lifecycle();

                        return true;
                    }
                    else
                    {
                        Count++;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private static async Task<bool> VirtualPerformance()
        {
            if (SSDMMB.VirtualPerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 5;
                SSDEPT Performance = SSDMMB.VirtualPerformance;

                while (SBMI.Virtuality && SSDMMB.VirtualPerformance == Performance)
                {
                    if (Count >= MaxCount)
                    {
                        SBMI.Performance = SSDMMB.VirtualPerformance;
                        SBMI.CategoryPerformance = SSDECPT.Virtual;
                        SBMI.Condition = true;
                        Lifecycle();

                        return true;
                    }
                    else
                    {
                        Count++;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private static async Task<bool> ProcessorPerformance()
        {
            if (SSDMMB.ProcessorPerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 5;
                SSDEPT Performance = SSDMMB.ProcessorPerformance;

                while (SBMI.ProcessorData.State && SMMB.ProcessorUsage > 0 && SBMI.ProcessorData.Now >= SMMB.ProcessorUsage && SSDMMB.ProcessorPerformance == Performance)
                {
                    if (Count >= MaxCount)
                    {
                        SBMI.Performance = SSDMMB.ProcessorPerformance;
                        SBMI.CategoryPerformance = SSDECPT.Processor;
                        SBMI.Condition = true;
                        Lifecycle();

                        return true;
                    }
                    else
                    {
                        Count++;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private static async Task<bool> FullScreenPerformance()
        {
            if (SSDMMB.FullScreenPerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 5;
                SSDEPT Performance = SSDMMB.FullScreenPerformance;

                while (SBMI.FullScreen && SSDMMB.FullScreenPerformance == Performance)
                {
                    if (Count >= MaxCount)
                    {
                        SBMI.Performance = SSDMMB.FullScreenPerformance;
                        SBMI.CategoryPerformance = SSDECPT.FullScreen;
                        SBMI.Condition = true;
                        Lifecycle();

                        return true;
                    }
                    else
                    {
                        Count++;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private static async Task<bool> ScreenSaverPerformance()
        {
            if (SSDMMB.ScreenSaverPerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 0;
                SSDEPT Performance = SSDMMB.ScreenSaverPerformance;

                while (SBMI.WindowsScreenSaver && SSDMMB.ScreenSaverPerformance == Performance)
                {
                    if (Count >= MaxCount)
                    {
                        SBMI.Performance = SSDMMB.ScreenSaverPerformance;
                        SBMI.CategoryPerformance = SSDECPT.ScreenSaver;
                        SBMI.Condition = true;
                        Lifecycle();

                        return true;
                    }
                    else
                    {
                        Count++;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }

        private static async Task<bool> BatterySaverPerformance()
        {
            if (SSDMMB.BatterySaverPerformance != SSDEPT.Resume)
            {
                int Count = 0;
                int MaxCount = 5;
                SSDEPT Performance = SSDMMB.BatterySaverPerformance;

                while (SBMI.BatteryData.State && (SBMI.BatteryData.SavingMode || SBMI.BatteryData.SaverStatus == "On") && SSDMMB.BatterySaverPerformance == Performance)
                {
                    if (Count >= MaxCount)
                    {
                        SBMI.Performance = SSDMMB.BatterySaverPerformance;
                        SBMI.CategoryPerformance = SSDECPT.BatterySaver;
                        SBMI.Condition = true;
                        Lifecycle();

                        return true;
                    }
                    else
                    {
                        Count++;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }

            return false;
        }
    }
}