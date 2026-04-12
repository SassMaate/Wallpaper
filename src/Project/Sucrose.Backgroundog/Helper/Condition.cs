using Skylark.Enum;
using Skylark.Standard.Extension.Storage;
using System.Diagnostics;
using SBMI = Sucrose.Backgroundog.Manage.Internal;
using SEEST = Skylark.Enum.EnergySaverType;
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
using SSIL = Sucrose.Signal.Interface.Launcher;
using SSLHR = Sucrose.Shared.Live.Helper.Run;
using SSMI = Sucrose.Signal.Manage.Internal;
using SSSEL = Sucrose.Shared.Space.Extension.Lifecycle;
using SSSHL = Sucrose.Shared.Space.Helper.Live;
using SSSHM = Sucrose.Shared.Space.Helper.Management;
using SSSHP = Sucrose.Shared.Space.Helper.Processor;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;
using SWHSS = Skylark.Wing.Helper.ScreenSaver;

namespace Sucrose.Backgroundog.Helper
{
    internal static class Condition
    {
        public static async Task Start()
        {
            if (SBMI.Performance == SSDEPT.Pause && ((SBMI.App != null && SBMI.App.HasExited) || (SBMI.Live != null && SBMI.Live.HasExited)))
            {
                SBMI.Condition = false;
                SBMI.Performance = SSDEPT.Resume;
                SBMI.NetworkPerformance = SSDENPT.Not;
                SBMI.CategoryPerformance = SSDECPT.Not;
            }
            else
            {
                if (await LockCondition())
                {
                    return;
                }

                if (await FocusCondition())
                {
                    return;
                }

                if (await SleepCondition())
                {
                    return;
                }

                if (await MemoryCondition())
                {
                    return;
                }

                if (await RemoteCondition())
                {
                    return;
                }

                if (await BatteryCondition())
                {
                    return;
                }

                if (await ConsoleCondition())
                {
                    return;
                }

                if (await GraphicCondition())
                {
                    return;
                }

                if (await NetworkCondition())
                {
                    return;
                }

                if (await SessionCondition())
                {
                    return;
                }

                if (await VirtualCondition())
                {
                    return;
                }

                if (await ProcessorCondition())
                {
                    return;
                }

                if (await FullScreenCondition())
                {
                    return;
                }

                if (await ScreenSaverCondition())
                {
                    return;
                }

                if (await BatterySaverCondition())
                {
                    return;
                }
            }

            await Task.CompletedTask;
        }

        private static async void Lifecycle()
        {
            if (SBMI.Performance == SSDEPT.Close)
            {
                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.ClosePerformance, false);
                SSLHR.Start();
            }
            else
            {
                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.PausePerformance, false);
                SBMI.Live = SSSHL.Get();

                if (SBMI.Live != null && !SBMI.Live.HasExited)
                {
                    if (SBMI.PausePerformance == SSDEPPT.Heavy)
                    {
                        SSSEL.Resume(SBMI.Live);

                        if (SMMRA.WebViewLive.Contains(SBMI.Live.ProcessName) || SMMRA.CefSharpLive.Contains(SBMI.Live.ProcessName))
                        {
                            try
                            {
                                Process.GetProcesses()
                                    ?.Where(Process => (Process.ProcessName.Contains(SMMRP.WebViewName) || Process.ProcessName.Contains(SMMRP.CefSharpName)) && SSSHM.GetCommandLine(Process.Id).Contains(SMMRG.AppName))
                                    ?.ToList()
                                    ?.ForEach(Process =>
                                    {
                                        SSSEL.Resume(Process.MainWindowHandle);
                                        SSSEL.Resume(Process.Handle);
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
                                        SSSEL.Resume(App.MainWindowHandle);
                                        SSSEL.Resume(App.Handle);
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

            if (SSSHP.Work(SMMRA.Launcher))
            {
                SSMI.LauncherManager.FileSave<SSIL>(new() { Icon = false });
            }
        }

        private static async Task<bool> LockCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.Lock)
            {
                int Count = 0;
                int MaxCount = 0;

                while (!SBMI.WindowsLock || SSDMMB.LockPerformance == SSDEPT.Resume)
                {
                    if (Count >= MaxCount)
                    {
                        Lifecycle();
                        SBMI.Condition = false;
                        SBMI.Performance = SSDEPT.Resume;
                        SBMI.CategoryPerformance = SSDECPT.Not;

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

        private static async Task<bool> FocusCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.Focus)
            {
                int Count = 0;
                int MaxCount = 3;

                while (SBMI.FocusDesktop || SSDMMB.FocusPerformance == SSDEPT.Resume)
                {
                    if (Count >= MaxCount)
                    {
                        Lifecycle();
                        SBMI.Condition = false;
                        SBMI.Performance = SSDEPT.Resume;
                        SBMI.CategoryPerformance = SSDECPT.Not;

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

        private static async Task<bool> SleepCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.Sleep)
            {
                int Count = 0;
                int MaxCount = 0;

                while (!SBMI.WindowsSleep || SSDMMB.SleepPerformance == SSDEPT.Resume)
                {
                    if (Count >= MaxCount)
                    {
                        Lifecycle();
                        SBMI.Condition = false;
                        SBMI.Performance = SSDEPT.Resume;
                        SBMI.CategoryPerformance = SSDECPT.Not;

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

        private static async Task<bool> MemoryCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.Memory)
            {
                int Count = 0;
                int MaxCount = 3;

                while (SMMB.MemoryUsage <= 0 || SBMI.MemoryData.MemoryLoad < SMMB.MemoryUsage || SSDMMB.MemoryPerformance == SSDEPT.Resume)
                {
                    if (Count >= MaxCount)
                    {
                        Lifecycle();
                        SBMI.Condition = false;
                        SBMI.Performance = SSDEPT.Resume;
                        SBMI.CategoryPerformance = SSDECPT.Not;

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

        private static async Task<bool> RemoteCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.Remote)
            {
                int Count = 0;
                int MaxCount = 3;

                while (!SBMI.RemoteDesktop || SSDMMB.RemotePerformance == SSDEPT.Resume)
                {
                    if (Count >= MaxCount)
                    {
                        Lifecycle();
                        SBMI.Condition = false;
                        SBMI.Performance = SSDEPT.Resume;
                        SBMI.CategoryPerformance = SSDECPT.Not;

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

        private static async Task<bool> BatteryCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.Battery)
            {
                int Count = 0;
                int MaxCount = 3;

                while (SMMB.BatteryUsage <= 0 || SBMI.BatteryData.PowerLineStatus == PowerLineStatus.Online || SBMI.BatteryData.ACPowerStatus == "Online" || SBMI.BatteryData.EstimatedChargeRemaining > SMMB.BatteryUsage || SSDMMB.BatteryPerformance == SSDEPT.Resume)
                {
                    if (Count >= MaxCount)
                    {
                        Lifecycle();
                        SBMI.Condition = false;
                        SBMI.Performance = SSDEPT.Resume;
                        SBMI.CategoryPerformance = SSDECPT.Not;

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

        private static async Task<bool> ConsoleCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.Console)
            {
                int Count = 0;
                int MaxCount = 0;

                while (SBMI.WindowsConsole || SSDMMB.ConsolePerformance == SSDEPT.Resume)
                {
                    if (Count >= MaxCount)
                    {
                        Lifecycle();
                        SBMI.Condition = false;
                        SBMI.Performance = SSDEPT.Resume;
                        SBMI.CategoryPerformance = SSDECPT.Not;

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

        private static async Task<bool> GraphicCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.Graphic)
            {
                int Count = 0;
                int MaxCount = 3;

                while (SMMB.GraphicUsage <= 0 || SBMI.GraphicData.Now < SMMB.GraphicUsage || SSDMMB.GraphicPerformance == SSDEPT.Resume)
                {
                    if (Count >= MaxCount)
                    {
                        Lifecycle();
                        SBMI.Condition = false;
                        SBMI.Performance = SSDEPT.Resume;
                        SBMI.CategoryPerformance = SSDECPT.Not;

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

        private static async Task<bool> NetworkCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.Network)
            {
                int Count = 0;
                int MaxCount = 3;


                if (SBMI.NetworkPerformance == SSDENPT.Ping)
                {
                    while (SMMB.PingValue <= 0 || SBMI.NetworkData.Ping < SMMB.PingValue || SSDMMB.NetworkPerformance == SSDEPT.Resume)
                    {
                        if (Count >= MaxCount)
                        {
                            Lifecycle();
                            SBMI.Condition = false;
                            SBMI.Performance = SSDEPT.Resume;
                            SBMI.NetworkPerformance = SSDENPT.Not;
                            SBMI.CategoryPerformance = SSDECPT.Not;

                            return true;
                        }
                        else
                        {
                            Count++;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                }
                else if (SBMI.NetworkPerformance == SSDENPT.Upload)
                {
                    while (SMMB.UploadValue <= 0 || SBMI.NetworkData.Upload < StorageExtension.Convert(SMMB.UploadValue, SMMB.UploadType, StorageType.Byte, ModeStorageType.Palila) || SSDMMB.NetworkPerformance == SSDEPT.Resume)
                    {
                        if (Count >= MaxCount)
                        {
                            Lifecycle();
                            SBMI.Condition = false;
                            SBMI.Performance = SSDEPT.Resume;
                            SBMI.NetworkPerformance = SSDENPT.Not;
                            SBMI.CategoryPerformance = SSDECPT.Not;

                            return true;
                        }
                        else
                        {
                            Count++;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                }
                else if (SBMI.NetworkPerformance == SSDENPT.Download)
                {
                    while (SMMB.DownloadValue <= 0 || SBMI.NetworkData.Download < StorageExtension.Convert(SMMB.DownloadValue, SMMB.DownloadType, StorageType.Byte, ModeStorageType.Palila) || SSDMMB.NetworkPerformance == SSDEPT.Resume)
                    {
                        if (Count >= MaxCount)
                        {
                            Lifecycle();
                            SBMI.Condition = false;
                            SBMI.Performance = SSDEPT.Resume;
                            SBMI.NetworkPerformance = SSDENPT.Not;
                            SBMI.CategoryPerformance = SSDECPT.Not;

                            return true;
                        }
                        else
                        {
                            Count++;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                }
            }

            return false;
        }

        private static async Task<bool> SessionCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.Session)
            {
                int Count = 0;
                int MaxCount = 0;

                while (SBMI.WindowsSession || SSDMMB.SessionPerformance == SSDEPT.Resume)
                {
                    if (Count >= MaxCount)
                    {
                        Lifecycle();
                        SBMI.Condition = false;
                        SBMI.Performance = SSDEPT.Resume;
                        SBMI.CategoryPerformance = SSDECPT.Not;

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

        private static async Task<bool> VirtualCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.Virtual)
            {
                int Count = 0;
                int MaxCount = 3;

                while (!SBMI.Virtuality || SSDMMB.VirtualPerformance == SSDEPT.Resume)
                {
                    if (Count >= MaxCount)
                    {
                        Lifecycle();
                        SBMI.Condition = false;
                        SBMI.Performance = SSDEPT.Resume;
                        SBMI.CategoryPerformance = SSDECPT.Not;

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

        private static async Task<bool> ProcessorCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.Processor)
            {
                int Count = 0;
                int MaxCount = 3;

                while (SMMB.ProcessorUsage <= 0 || SBMI.ProcessorData.Now < SMMB.ProcessorUsage || SSDMMB.ProcessorPerformance == SSDEPT.Resume)
                {
                    if (Count >= MaxCount)
                    {
                        Lifecycle();
                        SBMI.Condition = false;
                        SBMI.Performance = SSDEPT.Resume;
                        SBMI.CategoryPerformance = SSDECPT.Not;

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

        private static async Task<bool> FullScreenCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.FullScreen)
            {
                int Count = 0;
                int MaxCount = 3;

                while (!SBMI.FullScreen || SSDMMB.FullScreenPerformance == SSDEPT.Resume)
                {
                    if (Count >= MaxCount)
                    {
                        Lifecycle();
                        SBMI.Condition = false;
                        SBMI.Performance = SSDEPT.Resume;
                        SBMI.CategoryPerformance = SSDECPT.Not;

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

        private static async Task<bool> ScreenSaverCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.ScreenSaver)
            {
                int Count = 0;
                int MaxCount = 0;

                while (!SWHSS.IsRunning() || !SBMI.WindowsScreenSaver || SSDMMB.ScreenSaverPerformance == SSDEPT.Resume)
                {
                    if (Count >= MaxCount)
                    {
                        Lifecycle();
                        SBMI.Condition = false;
                        SBMI.Performance = SSDEPT.Resume;
                        SBMI.CategoryPerformance = SSDECPT.Not;

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

        private static async Task<bool> BatterySaverCondition()
        {
            if (SBMI.CategoryPerformance == SSDECPT.BatterySaver)
            {
                int Count = 0;
                int MaxCount = 3;

                while (SBMI.BatteryData.EnergySaverType != SEEST.On || SSDMMB.BatterySaverPerformance == SSDEPT.Resume)
                {
                    if (Count >= MaxCount)
                    {
                        Lifecycle();
                        SBMI.Condition = false;
                        SBMI.Performance = SSDEPT.Resume;
                        SBMI.CategoryPerformance = SSDECPT.Not;

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