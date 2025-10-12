using LibreHardwareMonitor.Hardware;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.Sockets;
using SBEAS = Sucrose.Backgroundog.Extension.AudioSession;
using SBED = Sucrose.Backgroundog.Extension.Data;
using SBEG = Sucrose.Backgroundog.Extension.Graphic;
using SBER = Sucrose.Backgroundog.Extension.Remote;
using SBEUV = Sucrose.Backgroundog.Extension.UpdateVisitor;
using SBEV = Sucrose.Backgroundog.Extension.Virtual;
using SBMI = Sucrose.Backgroundog.Manage.Internal;
using SBSCS = Sucrose.Backgroundog.Struct.ChildSensor;
using SBSSS = Sucrose.Backgroundog.Struct.StorageSensor;
using SBSS = Sucrose.Backgroundog.Struct.Sensor;
using SECNT = Skylark.Enum.ClearNumericType;
using SEMST = Skylark.Enum.ModeStorageType;
using SEST = Skylark.Enum.StorageType;
using SHN = Skylark.Helper.Numeric;
using SMMB = Sucrose.Manager.Manage.Backgroundog;
using SMMCB = Sucrose.Memory.Manage.Constant.Backgroundog;
using SMMCS = Sucrose.Memory.Manage.Constant.System;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SPIB = Sucrose.Pipe.Interface.Backgroundog;
using SPMI = Sucrose.Pipe.Manage.Internal;
using SSDECPT = Sucrose.Shared.Dependency.Enum.CategoryPerformanceType;
using SSDEPT = Sucrose.Shared.Dependency.Enum.PerformanceType;
using SSDMMB = Sucrose.Shared.Dependency.Manage.Manager.Backgroundog;
using SSDSH = Sucrose.Shared.Dependency.Struct.Host;
using SSEPPE = Skylark.Standard.Extension.Ping.PingExtension;
using SSESSE = Skylark.Standard.Extension.Storage.StorageExtension;
using SSIB = Sucrose.Signal.Interface.Backgroundog;
using SSMI = Sucrose.Signal.Manage.Internal;
using SSMMS = Skylark.Struct.Monitor.MonitorStruct;
using SSSHM = Sucrose.Shared.Space.Helper.Management;
using SSSHN = Sucrose.Shared.Space.Helper.Network;
using SSSHU = Sucrose.Shared.Space.Helper.User;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;
using STMI = Sucrose.Transmission.Manage.Internal;
using SWHFS = Skylark.Wing.Helper.FullScreen;
using SWNM = Skylark.Wing.Native.Methods;
using SWUD = Skylark.Wing.Utility.Desktop;
using SWUPN = Skylark.Wing.Utility.Plan;
using SWUPR = Skylark.Wing.Utility.Power;
using SWUS = Skylark.Wing.Utility.Screene;
using SystemInformation = System.Windows.Forms.SystemInformation;

namespace Sucrose.Backgroundog.Helper
{
    internal static class Specification
    {
        public static async Task Start()
        {
            if (SBMI.Exit)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        DateTime Date = DateTime.Now;

                        SBMI.DateData = new()
                        {
                            State = true,
                            Day = Date.Day,
                            Hour = Date.Hour,
                            Kind = Date.Kind,
                            Year = Date.Year,
                            Month = Date.Month,
                            Minute = Date.Minute,
                            Second = Date.Second,
                            DayOfWeek = Date.DayOfWeek,
                            DayOfYear = Date.DayOfYear,
                            Millisecond = Date.Millisecond
                        };
                    }
                    catch (Exception Exception)
                    {
                        await SSWEW.Watch_CatchException(Exception);
                    }
                });

                if (SBMI.BiosManagement)
                {
                    SBMI.BiosManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            ManagementObjectSearcher Searcher = new("SELECT * FROM Win32_BIOS");

                            foreach (ManagementObject Object in Searcher.Get().Cast<ManagementObject>())
                            {
                                SBMI.BiosData.State = true;
                                SBMI.BiosData.Name = SSSHM.Check(Object, "Name", string.Empty);
                                SBMI.BiosData.Caption = SSSHM.Check(Object, "Caption", string.Empty);
                                SBMI.BiosData.Version = SSSHM.Check(Object, "Version", string.Empty);
                                SBMI.BiosData.Description = SSSHM.Check(Object, "Description", string.Empty);
                                SBMI.BiosData.ReleaseDate = SSSHM.Check(Object, "ReleaseDate", string.Empty);
                                SBMI.BiosData.Manufacturer = SSSHM.Check(Object, "Manufacturer", string.Empty);
                                SBMI.BiosData.SerialNumber = SSSHM.Check(Object, "SerialNumber", string.Empty);
                                SBMI.BiosData.CurrentLanguage = SSSHM.Check(Object, "CurrentLanguage", string.Empty);

                                break;
                            }
                        }
                        catch (Exception Exception)
                        {
                            SBMI.BiosManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.PingManagement)
                {
                    SBMI.PingManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            if (SSSHN.GetHostEntry())
                            {
                                List<SSDSH> Hosts = SSSHN.GetHost();

                                SSDSH? Host = Hosts.FirstOrDefault(Host => Host.Name == SMMB.PingType);

                                if (Host != null)
                                {
                                    if (string.IsNullOrEmpty(SBMI.PingAddress) || SMMB.PingType != SBMI.PingHost)
                                    {
                                        foreach (IPAddress Address in SSSHN.GetHostAddresses(Host?.Address))
                                        {
                                            try
                                            {
                                                SBMI.PingAddress = $"{Address}";

                                                SBMI.NetworkData.PingData = await SSEPPE.SendAsync(SBMI.PingAddress, 1000);

                                                SBMI.PingHost = SMMB.PingType;
                                                SBMI.NetworkData.Host = Host?.Address;
                                                SBMI.NetworkData.Ping = SBMI.NetworkData.PingData.RoundTrip;
                                                SBMI.NetworkData.PingAddress = $"{SBMI.NetworkData.PingData.Address} ({Host?.Address})";

                                                break;
                                            }
                                            catch (Exception Exception)
                                            {
                                                SBMI.NetworkData.Ping = 0;
                                                SBMI.PingAddress = string.Empty;
                                                SBMI.NetworkData.PingData = new();
                                                await SSWEW.Watch_CatchException(Exception);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            SBMI.NetworkData.PingData = await SSEPPE.SendAsync(SBMI.PingAddress, 1000);

                                            SBMI.NetworkData.Host = Host?.Address;
                                            SBMI.NetworkData.Ping = SBMI.NetworkData.PingData.RoundTrip;
                                            SBMI.NetworkData.PingAddress = $"{SBMI.NetworkData.PingData.Address} ({Host?.Address})";
                                        }
                                        catch (Exception Exception)
                                        {
                                            SBMI.NetworkData.Ping = 0;
                                            SBMI.PingAddress = string.Empty;
                                            SBMI.NetworkData.PingData = new();
                                            await SSWEW.Watch_CatchException(Exception);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                SBMI.NetworkData.Ping = 0;
                                SBMI.NetworkData.PingData = new();
                            }

                            await Task.Delay(SBMI.SpecificationLessTime);

                            SBMI.PingManagement = true;
                        }
                        catch (Exception Exception)
                        {
                            SBMI.PingManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.AudioManagement)
                {
                    SBMI.AudioManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            if (SMMB.AudioRequired)
                            {
                                if (SBMI.SessionManagement)
                                {
                                    SBMI.SessionManagement = false;

                                    SBMI.SessionManager = new();

                                    SBMI.AudioVisualizer = new();

                                    SBMI.AudioVisualizer.AudioDataAvailable += (s, e) =>
                                    {
                                        try
                                        {
                                            SBMI.AudioData.Data = e;
                                        }
                                        catch { }
                                    };

                                    SBMI.AudioVisualizer.Start();

                                    SBMI.SessionManager.SessionListChanged += (s, e) => SBEAS.SessionListChanged();
                                }

                                SBEAS.SessionListChanged();

                                await Task.Delay(SBMI.SpecificationTime);

                                SBMI.AudioManagement = true;
                            }
                            else
                            {

                                SBMI.DataSource = null;
                                SBMI.SessionManager = null;
                                SBMI.AudioManagement = true;
                                SBMI.AudioData.State = false;
                                SBMI.SessionManagement = true;

                                try
                                {
                                    SBMI.AudioVisualizer.Stop();
                                    SBMI.SessionManager.SessionListChanged -= (s, e) => SBEAS.SessionListChanged();
                                }
                                catch { }
                            }
                        }
                        catch (Exception Exception)
                        {
                            SBMI.AudioManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.MemoryManagement)
                {
                    SBMI.MemoryManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            SWNM.MEMORYSTATUSEX MemoryStatus = new();

                            if (SWNM.GlobalMemoryStatusEx(MemoryStatus))
                            {
                                SBMI.MemoryData.State = true;

                                SBMI.MemoryData.Name = "Total Memory";
                                SBMI.MemoryData.MemoryAvailable = (float)MemoryStatus.ullAvailPhys / (1024 * 1024 * 1024);
                                SBMI.MemoryData.MemoryLoad = 100.0f - (100.0f * MemoryStatus.ullAvailPhys / MemoryStatus.ullTotalPhys);
                                SBMI.MemoryData.MemoryUsed = (float)(MemoryStatus.ullTotalPhys - MemoryStatus.ullAvailPhys) / (1024 * 1024 * 1024);

                                SBMI.MemoryData.VirtualName = "Virtual Memory";
                                SBMI.MemoryData.VirtualMemoryAvailable = (float)MemoryStatus.ullAvailPageFile / (1024 * 1024 * 1024);
                                SBMI.MemoryData.VirtualMemoryLoad = 100.0f - (100.0f * MemoryStatus.ullAvailPageFile / MemoryStatus.ullTotalPageFile);
                                SBMI.MemoryData.VirtualMemoryUsed = (float)(MemoryStatus.ullTotalPageFile - MemoryStatus.ullAvailPageFile) / (1024 * 1024 * 1024);
                            }

                            await Task.Delay(SBMI.SpecificationLessTime);

                            SBMI.MemoryManagement = true;
                        }
                        catch (Exception Exception)
                        {
                            SBMI.MemoryManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.BatteryManagement)
                {
                    SBMI.BatteryManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            ManagementObjectSearcher Searcher = new("SELECT * FROM Win32_Battery");

                            foreach (ManagementObject Object in Searcher.Get().Cast<ManagementObject>())
                            {
                                SBMI.BatteryData.State = true;

                                SBMI.BatteryData.Name = SSSHM.Check(Object, "Name", string.Empty);
                                SBMI.BatteryData.Status = SSSHM.Check(Object, "Status", string.Empty);
                                SBMI.BatteryData.Description = SSSHM.Check(Object, "Description", string.Empty);
                                SBMI.BatteryData.Chemistry = Convert.ToInt32(SSSHM.Check(Object, "Chemistry", "0"));
                                SBMI.BatteryData.DesignVoltage = Convert.ToInt32(SSSHM.Check(Object, "DesignVoltage", "0"));
                                SBMI.BatteryData.EstimatedRunTime = Convert.ToInt32(SSSHM.Check(Object, "EstimatedRunTime", "0"));
                                SBMI.BatteryData.EstimatedChargeRemaining = Convert.ToInt32(SSSHM.Check(Object, "EstimatedChargeRemaining", "0"));

                                break;
                            }

                            SBMI.BatteryData.SavingMode = SWUPR.IsBatterySavingMode;
                            SBMI.BatteryData.BatteryFlag = $"{SWUPR.GetBatteryFlag()}";
                            SBMI.BatteryData.BatteryLifeTime = SWUPR.GetBatteryLifeTime();
                            SBMI.BatteryData.ACPowerStatus = $"{SWUPR.GetACPowerStatus()}";
                            SBMI.BatteryData.EnergySaverType = SWUPR.GetEnergySaverState();
                            SBMI.BatteryData.SaverStatus = $"{SWUPR.GetBatterySaverStatus()}";
                            SBMI.BatteryData.BatteryLifePercent = SWUPR.GetBatteryLifePercent();
                            SBMI.BatteryData.BatteryFullLifeTime = SWUPR.GetBatteryFullLifeTime();

                            SBMI.BatteryData.LifePercent = SystemInformation.PowerStatus.BatteryLifePercent;
                            SBMI.BatteryData.PowerLineStatus = SystemInformation.PowerStatus.PowerLineStatus;
                            SBMI.BatteryData.FullLifetime = SystemInformation.PowerStatus.BatteryFullLifetime;
                            SBMI.BatteryData.ChargeStatus = SystemInformation.PowerStatus.BatteryChargeStatus;
                            SBMI.BatteryData.LifeRemaining = SystemInformation.PowerStatus.BatteryLifeRemaining;

                            SBMI.BatteryData.PowerPlanType = SWUPN.GetPlanFromGuid(SWUPN.GetActivePowerSchemeGuid());

                            await Task.Delay(SBMI.SpecificationTime);

                            SBMI.BatteryManagement = true;
                        }
                        catch (Exception Exception)
                        {
                            SBMI.BatteryManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.GraphicManagement)
                {
                    SBMI.GraphicManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            SBMI.GraphicInterfaces = SSSHU.GetGraphic();

                            SMMI.SystemSettingManager.SetSetting(SMMCS.GraphicInterfaces, SBMI.GraphicInterfaces);

                            if (SBMI.GraphicInterfaces.Any() && (string.IsNullOrEmpty(SMMB.GraphicAdapter) || !SBMI.GraphicInterfaces.Contains(SMMB.GraphicAdapter)))
                            {
                                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.GraphicAdapter, SBMI.GraphicInterfaces.FirstOrDefault());
                            }
                        }
                        catch (Exception Exception)
                        {
                            SBMI.GraphicManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.NetworkManagement)
                {
                    SBMI.NetworkManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            SBMI.NetworkInterfaces = SSSHN.InstanceNetworkInterfaces();

                            SMMI.SystemSettingManager.SetSetting(SMMCS.NetworkInterfaces, SBMI.NetworkInterfaces);

                            if (SBMI.NetworkInterfaces.Any() && (string.IsNullOrEmpty(SMMB.NetworkAdapter) || !SBMI.NetworkInterfaces.Contains(SMMB.NetworkAdapter)))
                            {
                                SMMI.BackgroundogSettingManager.SetSetting(SMMCB.NetworkAdapter, SBMI.NetworkInterfaces.FirstOrDefault());
                            }
                        }
                        catch (Exception Exception)
                        {
                            SBMI.NetworkManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.StorageManagement)
                {
                    SBMI.StorageManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            ManagementObjectSearcher Searcher = new("SELECT * FROM Win32_LogicalDisk");

                            foreach (ManagementObject Object in Searcher.Get().Cast<ManagementObject>())
                            {
                                SBMI.StorageData.State = true;

                                SBMI.BatteryData.Name = SSSHM.Check(Object, "Name", string.Empty);
                                SBMI.BatteryData.Description = SSSHM.Check(Object, "Description", string.Empty);

                                break;
                            }

                            await Task.Delay(SBMI.SpecificationTime);

                            SBMI.StorageManagement = true;
                        }
                        catch (Exception Exception)
                        {
                            SBMI.StorageManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.GraphicManagement2)
                {
                    SBMI.GraphicManagement2 = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            SBMI.GraphicData.Name = SMMB.GraphicAdapter;
                            SBMI.GraphicData.Manufacturer = SBEG.Manufacturer();

                            await Task.Delay(SBMI.SpecificationLessTime);

                            SBMI.GraphicManagement2 = true;
                        }
                        catch (Exception Exception)
                        {
                            SBMI.GraphicManagement2 = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.ProcessorManagement)
                {
                    SBMI.ProcessorManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            ManagementObjectSearcher Searcher = new("SELECT * FROM Win32_Processor");

                            foreach (ManagementObject Object in Searcher.Get().Cast<ManagementObject>())
                            {
                                SBMI.ProcessorData.State = true;
                                SBMI.ProcessorData.ProcessorCount = Environment.ProcessorCount;
                                SBMI.ProcessorData.Name = SSSHM.Check(Object, "Name", string.Empty);
                                SBMI.ProcessorData.Core = Convert.ToInt32(SSSHM.Check(Object, "NumberOfCores", "0"));
                                SBMI.ProcessorData.Thread = Convert.ToInt32(SSSHM.Check(Object, "NumberOfLogicalProcessors", "0"));

                                break;
                            }

                            SMMI.SystemSettingManager.SetSetting(SMMCS.ProcessorInterfaces, SSSHU.GetProcessor());
                        }
                        catch (Exception Exception)
                        {
                            SBMI.ProcessorManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.ProcessorManagement2)
                {
                    SBMI.ProcessorManagement2 = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            if (SBMI.ProcessorCounter == null)
                            {
                                SBMI.ProcessorCounter = new("Processor", "% Processor Time", "_Total");
                            }
                            else
                            {
                                SBMI.ProcessorData.Now = SBMI.ProcessorCounter.NextValue();

                                SBMI.ProcessorData.Type = $"{SBMI.ProcessorCounter.CounterType}";

                                SBMI.ProcessorData.Max = SBMI.ProcessorData.Now > SBMI.ProcessorData.Max ? SBMI.ProcessorData.Now : SBMI.ProcessorData.Max;
                                SBMI.ProcessorData.Min = SBMI.ProcessorData.Now < SBMI.ProcessorData.Min ? SBMI.ProcessorData.Now : SBMI.ProcessorData.Min;
                            }

                            if (SBMI.ProcessorsCounter == null)
                            {
                                SBMI.ProcessorsCounter = new PerformanceCounter[Environment.ProcessorCount];

                                for (int Core = 0; Core < SBMI.ProcessorsCounter.Length; Core++)
                                {
                                    SBMI.ProcessorsCounter[Core] = new PerformanceCounter("Processor", "% Processor Time", Core.ToString());
                                }
                            }
                            else
                            {
                                List<SBSCS> Sensors = new();

                                SBMI.ProcessorData.CoreNow = 0;

                                for (int Core = 0; Core < SBMI.ProcessorsCounter.Length; Core++)
                                {
                                    float Now = SBMI.ProcessorsCounter[Core].NextValue();

                                    SBMI.ProcessorData.CoreNow = SBMI.ProcessorData.CoreNow > Now ? SBMI.ProcessorData.CoreNow : Now;

                                    SBMI.ProcessorData.CoreMax = SBMI.ProcessorData.CoreNow > SBMI.ProcessorData.CoreMax ? SBMI.ProcessorData.CoreNow : SBMI.ProcessorData.CoreMax;
                                    SBMI.ProcessorData.CoreMin = SBMI.ProcessorData.CoreNow < SBMI.ProcessorData.CoreMin ? SBMI.ProcessorData.CoreNow : SBMI.ProcessorData.CoreMin;

                                    Sensors.Add(new SBSCS
                                    {
                                        Now = Now,
                                        Index = Core,
                                        Name = $"Core #{Core}",
                                        Type = $"{SBMI.ProcessorsCounter[Core].CounterType}"
                                    });
                                }

                                string Result = JsonConvert.SerializeObject(Sensors, Formatting.Indented);

                                SBMI.ProcessorData.Cores = JArray.Parse(Result);
                            }

                            await Task.Delay(SBMI.SpecificationLessTime);

                            SBMI.ProcessorManagement2 = true;
                        }
                        catch (Exception Exception)
                        {
                            SBMI.ProcessorManagement2 = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.MotherboardManagement)
                {
                    SBMI.MotherboardManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            ManagementObjectSearcher Searcher = new("SELECT * FROM Win32_BaseBoard");

                            foreach (ManagementObject Object in Searcher.Get().Cast<ManagementObject>())
                            {
                                SBMI.MotherboardData.State = true;
                                SBMI.MotherboardData.Product = SSSHM.Check(Object, "Product", string.Empty);
                                SBMI.MotherboardData.Version = SSSHM.Check(Object, "Version", string.Empty);
                                SBMI.MotherboardData.Manufacturer = SSSHM.Check(Object, "Manufacturer", string.Empty);
                                SBMI.MotherboardData.Name = $"{SBMI.MotherboardData.Manufacturer} {SBMI.MotherboardData.Product}";

                                break;
                            }
                        }
                        catch (Exception Exception)
                        {
                            SBMI.MotherboardManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.NetworkManagement2 && SBMI.NetworkInterfaces.Any())
                {
                    SBMI.NetworkManagement2 = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            if (SBMI.NetworkInterfaces.Contains(SMMB.NetworkAdapter))
                            {
                                foreach (string Name in SBMI.NetworkInterfaces)
                                {
                                    if (SMMB.NetworkAdapter == Name)
                                    {
                                        if (SMMB.NetworkAdapter != SBMI.NetworkData.Name)
                                        {
                                            SBMI.NetworkData.State = true;
                                            SBMI.NetworkData.Name = SMMB.NetworkAdapter;

                                            SBMI.UploadCounter = new("Network Interface", "Bytes Sent/sec", Name);
                                            SBMI.DownloadCounter = new("Network Interface", "Bytes Received/sec", Name);
                                        }

                                        if (SBMI.UploadCounter != null)
                                        {
                                            SBMI.NetworkData.Upload = SBMI.UploadCounter.NextValue();

                                            SBMI.NetworkData.UploadData = SSESSE.AutoConvert(SBMI.NetworkData.Upload, SEST.Byte, SEMST.Palila);

                                            SBMI.NetworkData.FormatUploadData = SHN.Numeral(SBMI.NetworkData.UploadData.Value, true, true, 2, '0', SECNT.None) + " " + SBMI.NetworkData.UploadData.Text;
                                        }

                                        if (SBMI.DownloadCounter != null)
                                        {
                                            SBMI.NetworkData.Download = SBMI.DownloadCounter.NextValue();

                                            SBMI.NetworkData.DownloadData = SSESSE.AutoConvert(SBMI.NetworkData.Download, SEST.Byte, SEMST.Palila);

                                            SBMI.NetworkData.FormatDownloadData = SHN.Numeral(SBMI.NetworkData.DownloadData.Value, true, true, 2, '0', SECNT.None) + " " + SBMI.NetworkData.DownloadData.Text;
                                        }

                                        break;
                                    }
                                }
                            }
                            else
                            {
                                SBMI.NetworkData.State = false;
                                SBMI.NetworkData.Name = SMMB.NetworkAdapter;
                            }

                            await Task.Delay(SBMI.SpecificationLessTime);

                            SBMI.NetworkManagement2 = true;
                        }
                        catch (Exception Exception)
                        {
                            SBMI.NetworkManagement2 = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.RemoteManagement && (SSDMMB.RemotePerformance != SSDEPT.Resume || SBMI.CategoryPerformance == SSDECPT.Remote))
                {
                    SBMI.RemoteManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            SBMI.RemoteDesktop = SBMI.WindowsRemote || SBER.RemotelyActive();

                            await Task.Delay(SBMI.SpecificationTime);

                            SBMI.RemoteManagement = true;
                        }
                        catch (Exception Exception)
                        {
                            SBMI.RemoteManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.VirtualityManagement && (SSDMMB.VirtualPerformance != SSDEPT.Resume || SBMI.CategoryPerformance == SSDECPT.Virtual))
                {
                    SBMI.VirtualityManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            SBMI.Virtuality = SBEV.VirtualityActive();

                            await Task.Delay(SBMI.SpecificationTime);

                            SBMI.VirtualityManagement = true;
                        }
                        catch (Exception Exception)
                        {
                            SBMI.VirtualityManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.FullScreenManagement && (SSDMMB.FullScreenPerformance != SSDEPT.Resume || SBMI.CategoryPerformance == SSDECPT.FullScreen))
                {
                    SBMI.FullScreenManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            SBMI.FullScreen = false;

                            if (!SBMI.FocusDesktop)
                            {
                                SWUS.Initialize();

                                IntPtr Foreground = SWNM.GetForegroundWindow();

                                foreach (SSMMS Screen in SWUS.Screens)
                                {
                                    if (SWHFS.IsFullScreen(Foreground, Screen.rcMonitor))
                                    {
                                        SBMI.FullScreen = true;

                                        break;
                                    }
                                }
                            }

                            await Task.Delay(SBMI.SpecificationLessTime);

                            SBMI.FullScreenManagement = true;
                        }
                        catch (Exception Exception)
                        {
                            SBMI.FullScreenManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.FocusManagement && (SSDMMB.FocusPerformance != SSDEPT.Resume || SSDMMB.FullScreenPerformance != SSDEPT.Resume || SBMI.CategoryPerformance == SSDECPT.Focus || SBMI.CategoryPerformance == SSDECPT.FullScreen))
                {
                    SBMI.FocusManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            SBMI.FocusDesktop = SWUD.IsDesktopBasic() || SWUD.IsDesktopAdvanced();

                            await Task.Delay(SBMI.SpecificationLessTime);

                            SBMI.FocusManagement = true;
                        }
                        catch (Exception Exception)
                        {
                            SBMI.FocusManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                if (SBMI.ComputerManagement)
                {
                    SBMI.ComputerManagement = false;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            SBMI.Computer.Accept(new SBEUV());

                            foreach (IHardware Hardware in SBMI.Computer.Hardware)
                            {
                                if (Hardware.HardwareType is HardwareType.GpuAmd or HardwareType.GpuIntel or HardwareType.GpuNvidia)
                                {
                                    _ = Task.Run(async () =>
                                    {
                                        try
                                        {
                                            //Hardware.Update();

                                            List<SBSS> Sensors = new()
                                            {
                                                new SBSS
                                                {
                                                    Name = Hardware.Name,
                                                    Type = $"{Hardware.HardwareType}"
                                                }
                                            };

                                            foreach (ISensor Sensor in Hardware.Sensors)
                                            {
                                                Sensors.Add(new SBSS
                                                {
                                                    Max = Sensor.Max,
                                                    Min = Sensor.Min,
                                                    Name = Sensor.Name,
                                                    Now = Sensor.Value,
                                                    Type = $"{Sensor.SensorType}"
                                                });
                                            }

                                            string Result = JsonConvert.SerializeObject(Sensors, Formatting.Indented);

                                            switch (Hardware.HardwareType)
                                            {
                                                case HardwareType.GpuAmd:
                                                    SBMI.GraphicData.State = true;
                                                    SBMI.GraphicData.Amd = JArray.Parse(Result);
                                                    break;
                                                case HardwareType.GpuIntel:
                                                    SBMI.GraphicData.State = true;
                                                    SBMI.GraphicData.Intel = JArray.Parse(Result);
                                                    break;
                                                case HardwareType.GpuNvidia:
                                                    SBMI.GraphicData.State = true;
                                                    SBMI.GraphicData.Nvidia = JArray.Parse(Result);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        catch (Exception Exception)
                                        {
                                            await SSWEW.Watch_CatchException(Exception);
                                        }
                                    });
                                }
                            }

                            await Task.Delay(SBMI.SpecificationLessTime);

                            SBMI.ComputerManagement = true;
                        }
                        catch (Exception Exception)
                        {
                            SBMI.ComputerManagement = true;
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    });
                }

                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (!SBMI.Condition && SBMI.PipeManagement && SMMB.PipeRequired)
                        {
                            SBMI.PipeManagement = false;

                            JsonSerializerSettings SerializerSettings = new()
                            {
                                Formatting = Formatting.None,
                                TypeNameHandling = TypeNameHandling.None
                            };

                            await SPMI.BackgroundogManager.StartClient(JsonConvert.SerializeObject(new SPIB()
                            {
                                Bios = SBED.GetBiosInfo(),
                                Date = SBED.GetDateInfo(),
                                Audio = SBED.GetAudioInfo(),
                                Memory = SBED.GetMemoryInfo(),
                                Battery = SBED.GetBatteryInfo(),
                                Graphic = SBED.GetGraphicInfo(),
                                Network = SBED.GetNetworkInfo(),
                                Storage = SBED.GetStorageInfo(),
                                Processor = SBED.GetProcessorInfo(),
                                Motherboard = SBED.GetMotherboardInfo()
                            }, SerializerSettings));

                            SBMI.PipeManagement = true;
                        }
                    }
                    catch (Exception Exception)
                    {
                        SBMI.PipeManagement = true;
                        await SSWEW.Watch_CatchException(Exception);
                    }
                });

                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (!SBMI.Condition && SBMI.SignalManagement && SMMB.SignalRequired)
                        {
                            SBMI.SignalManagement = false;

                            SSMI.BackgroundogManager.FileSave<SSIB>(new()
                            {
                                Bios = SBED.GetBiosInfo(),
                                Date = SBED.GetDateInfo(),
                                Audio = SBED.GetAudioInfo(),
                                Memory = SBED.GetMemoryInfo(),
                                Battery = SBED.GetBatteryInfo(),
                                Graphic = SBED.GetGraphicInfo(),
                                Network = SBED.GetNetworkInfo(),
                                Storage = SBED.GetStorageInfo(),
                                Processor = SBED.GetProcessorInfo(),
                                Motherboard = SBED.GetMotherboardInfo()
                            });

                            SBMI.SignalManagement = true;
                        }
                    }
                    catch (Exception Exception)
                    {
                        SBMI.SignalManagement = true;
                        await SSWEW.Watch_CatchException(Exception);
                    }
                });

                _ = Task.Run(async () =>
                {
                    try
                    {
                        if (!SBMI.Condition && SBMI.TransmissionManagement && SMMB.TransmissionRequired)
                        {
                            SBMI.TransmissionManagement = false;

                            if (STMI.BackgroundogManager == null)
                            {
                                STMI.BackgroundogManager = new(SMMRG.Loopback, SMMB.TransmissionPort);
                            }

                            JsonSerializerSettings SerializerSettings = new()
                            {
                                Formatting = Formatting.None,
                                TypeNameHandling = TypeNameHandling.None
                            };

                            await STMI.BackgroundogManager.StartClient(JsonConvert.SerializeObject(new SPIB()
                            {
                                Bios = SBED.GetBiosInfo(),
                                Date = SBED.GetDateInfo(),
                                Audio = SBED.GetAudioInfo(),
                                Memory = SBED.GetMemoryInfo(),
                                Battery = SBED.GetBatteryInfo(),
                                Graphic = SBED.GetGraphicInfo(),
                                Network = SBED.GetNetworkInfo(),
                                Storage = SBED.GetStorageInfo(),
                                Processor = SBED.GetProcessorInfo(),
                                Motherboard = SBED.GetMotherboardInfo()
                            }, SerializerSettings));

                            SBMI.TransmissionManagement = true;
                        }
                    }
                    catch (SocketException Exception)
                    {
                        SBMI.TransmissionManagement = true;
                        await SSWEW.Watch_CatchException(Exception);
                        STMI.BackgroundogManager = new(SMMRG.Loopback, SMMB.TransmissionPort);
                    }
                    catch (Exception Exception)
                    {
                        SBMI.TransmissionManagement = true;
                        await SSWEW.Watch_CatchException(Exception);
                    }
                });

                //_ = Task.Run(() =>
                //{
                //    foreach (IHardware Hardware in SBMI.Computer.Hardware)
                //    {
                //        Console.WriteLine("Hardware: {0}, Type: {1}", Hardware.Name, Hardware.HardwareType);

                //        foreach (IHardware Subhardware in Hardware.SubHardware)
                //        {
                //            Console.WriteLine("\tSubhardware: {0}, Type: {1}", Subhardware.Name, Hardware.HardwareType);

                //            foreach (ISensor Sensor in Subhardware.Sensors)
                //            {
                //                Console.WriteLine("\t\tSensor: {0}, Type: {1}, Value: {2}", Sensor.Name, Sensor.SensorType, Sensor.Value);
                //            }
                //        }

                //        foreach (ISensor Sensor in Hardware.Sensors)
                //        {
                //            Console.WriteLine("\tSensor: {0}, Type: {1}, Value: {2}", Sensor.Name, Sensor.SensorType, Sensor.Value);
                //        }
                //    }

                //    Console.WriteLine("----------------------------------------------");
                //});
            }

            await Task.CompletedTask;
        }
    }
}