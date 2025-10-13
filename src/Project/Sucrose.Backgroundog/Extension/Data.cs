using Newtonsoft.Json.Linq;
using SBMI = Sucrose.Backgroundog.Manage.Internal;

namespace Sucrose.Backgroundog.Extension
{
    internal static class Data
    {
        public static JObject GetBiosInfo()
        {
            return new JObject
            {
                { "Name", SBMI.BiosData.Name },
                { "State", SBMI.BiosData.State },
                { "Caption", SBMI.BiosData.Caption },
                { "Version", SBMI.BiosData.Version },
                { "Description", SBMI.BiosData.Description },
                { "ReleaseDate", SBMI.BiosData.ReleaseDate },
                { "Manufacturer", SBMI.BiosData.Manufacturer },
                { "SerialNumber", SBMI.BiosData.SerialNumber },
                { "CurrentLanguage", SBMI.BiosData.CurrentLanguage }
            };
        }

        public static JObject GetDateInfo()
        {
            return new JObject
            {
                { "Day", SBMI.DateData.Day },
                { "Hour", SBMI.DateData.Hour },
                { "Year", SBMI.DateData.Year },
                { "Month", SBMI.DateData.Month },
                { "State", SBMI.DateData.State },
                { "Minute", SBMI.DateData.Minute },
                { "Second", SBMI.DateData.Second },
                { "Kind", $"{SBMI.DateData.Kind}" },
                { "DayOfYear", SBMI.DateData.DayOfYear },
                { "Millisecond", SBMI.DateData.Millisecond },
                { "DayOfWeek", $"{SBMI.DateData.DayOfWeek}" }
            };
        }

        public static JObject GetAudioInfo()
        {
            return new JObject
            {
                //{ "PID", SBMI.AudioData.PID },
                { "State", SBMI.AudioData.State },
                { "Title", SBMI.AudioData.Title },
                { "Artist", SBMI.AudioData.Artist },
                //{ "Hwnd", $"{SBMI.AudioData.Hwnd}" },
                { "Subtitle", SBMI.AudioData.Subtitle },
                { "AlbumTitle", SBMI.AudioData.AlbumTitle },
                { "Data", new JArray(SBMI.AudioData.Data) },
                { "AlbumArtist", SBMI.AudioData.AlbumArtist },
                { "SourceAppId", SBMI.AudioData.SourceAppId },
                { "TrackNumber", SBMI.AudioData.TrackNumber },
                { "MediaType", $"{SBMI.AudioData.MediaType}" },
                { "PlaybackRate", SBMI.AudioData.PlaybackRate },
                { "PropsValid", $"{SBMI.AudioData.PropsValid}" },
                { "RepeatMode", $"{SBMI.AudioData.RepeatMode}" },
                //{ "RenderDeviceId", SBMI.AudioData.RenderDeviceId },
                { "ShuffleEnabled", SBMI.AudioData.ShuffleEnabled },
                //{ "SourceDeviceId", SBMI.AudioData.SourceDeviceId },
                //{ "PlaybackCaps", $"{SBMI.AudioData.PlaybackCaps}" },
                { "PlaybackMode", $"{SBMI.AudioData.PlaybackMode}" },
                { "AlbumTrackCount", SBMI.AudioData.AlbumTrackCount },
                { "ThumbnailString", SBMI.AudioData.ThumbnailString },
                { "PlaybackState", $"{SBMI.AudioData.PlaybackState}" },
                { "EndTime", SBMI.AudioData.EndTime.TotalMilliseconds },
                { "Position", SBMI.AudioData.Position.TotalMilliseconds },
                { "StartTime", SBMI.AudioData.StartTime.TotalMilliseconds },
                { "LastPlayingFileTime", SBMI.AudioData.LastPlayingFileTime },
                { "PositionSetFileTime", SBMI.AudioData.PositionSetFileTime },
                { "MaxSeekTime", SBMI.AudioData.MaxSeekTime.TotalMilliseconds },
                { "MinSeekTime", SBMI.AudioData.MinSeekTime.TotalMilliseconds }
            };
        }

        public static JObject GetMemoryInfo()
        {
            return new JObject
            {
                { "Name", SBMI.MemoryData.Name },
                { "State", SBMI.MemoryData.State },
                { "MemoryLoad", SBMI.MemoryData.MemoryLoad },
                { "MemoryUsed", SBMI.MemoryData.MemoryUsed },
                { "VirtualName", SBMI.MemoryData.VirtualName },
                { "MemoryAvailable", SBMI.MemoryData.MemoryAvailable },
                { "VirtualMemoryLoad", SBMI.MemoryData.VirtualMemoryLoad },
                { "VirtualMemoryUsed", SBMI.MemoryData.VirtualMemoryUsed },
                { "VirtualMemoryAvailable", SBMI.MemoryData.VirtualMemoryAvailable }
            };
        }

        public static JObject GetBatteryInfo()
        {
            return new JObject
            {
                { "Name", SBMI.BatteryData.Name },
                { "State", SBMI.BatteryData.State },
                { "Status", SBMI.BatteryData.Status },
                { "Chemistry", SBMI.BatteryData.Chemistry },
                { "SavingMode", SBMI.BatteryData.SavingMode },
                { "BatteryFlag", SBMI.BatteryData.BatteryFlag },
                { "Description", SBMI.BatteryData.Description },
                { "LifePercent", SBMI.BatteryData.LifePercent },
                { "SaverStatus", SBMI.BatteryData.SaverStatus },
                { "FullLifetime", SBMI.BatteryData.FullLifetime },
                { "ACPowerStatus", SBMI.BatteryData.ACPowerStatus },
                { "DesignVoltage", SBMI.BatteryData.DesignVoltage },
                { "LifeRemaining", SBMI.BatteryData.LifeRemaining },
                { "ChargeStatus", $"{SBMI.BatteryData.ChargeStatus}" },
                { "BatteryLifeTime", SBMI.BatteryData.BatteryLifeTime },
                { "PowerPlanType", $"{SBMI.BatteryData.PowerPlanType}" },
                { "EstimatedRunTime", SBMI.BatteryData.EstimatedRunTime },
                { "EnergySaverType", $"{SBMI.BatteryData.EnergySaverType}" },
                { "PowerLineStatus", $"{SBMI.BatteryData.PowerLineStatus}" },
                { "BatteryLifePercent", SBMI.BatteryData.BatteryLifePercent },
                { "BatteryFullLifeTime", SBMI.BatteryData.BatteryFullLifeTime },
                { "EstimatedChargeRemaining", SBMI.BatteryData.EstimatedChargeRemaining }
            };
        }

        public static JObject GetGraphicInfo()
        {
            return new JObject
            {
                { "Amd", SBMI.GraphicData.Amd },
                { "Name", SBMI.GraphicData.Name },
                { "Intel", SBMI.GraphicData.Intel },
                { "State", SBMI.GraphicData.State },
                { "Nvidia", SBMI.GraphicData.Nvidia },
                { "Manufacturer", SBMI.GraphicData.Manufacturer }
            };
        }

        public static JObject GetNetworkInfo()
        {
            return new JObject
            {
                { "Host", SBMI.NetworkData.Host },
                { "Name", SBMI.NetworkData.Name },
                { "Ping", SBMI.NetworkData.Ping },
                { "State", SBMI.NetworkData.State },
                { "Total", SBMI.NetworkData.Total },
                { "Online", SBMI.NetworkData.Online },
                { "Upload", SBMI.NetworkData.Upload },
                { "Download", SBMI.NetworkData.Download },
                { "PingAddress", SBMI.NetworkData.PingAddress },
                { "FormatTotalData", SBMI.NetworkData.FormatTotalData },
                { "FormatUploadData", SBMI.NetworkData.FormatUploadData },
                { "FormatDownloadData", SBMI.NetworkData.FormatDownloadData },
                {
                    "PingData", new JObject
                    {
                        { "Ttl", SBMI.NetworkData.PingData.Ttl },
                        { "Buffer", SBMI.NetworkData.PingData.Buffer },
                        { "Address", SBMI.NetworkData.PingData.Address },
                        { "Fragment", SBMI.NetworkData.PingData.Fragment },
                        { "Result", (int)SBMI.NetworkData.PingData.Result },
                        { "RoundTrip", SBMI.NetworkData.PingData.RoundTrip },
                        { "ResultText", SBMI.NetworkData.PingData.ResultText }
                    }
                },
                {
                    "TotalData", new JObject
                    {
                        { "More", SBMI.NetworkData.TotalData.More },
                        { "Value", SBMI.NetworkData.TotalData.Value },
                        { "Long", (int)SBMI.NetworkData.TotalData.Long },
                        { "Type", (int)SBMI.NetworkData.TotalData.Type },
                        { "Short", (int)SBMI.NetworkData.TotalData.Short },
                        { "LongText", SBMI.NetworkData.TotalData.LongText },
                        { "TypeText", SBMI.NetworkData.TotalData.TypeText },
                        { "ShortText", SBMI.NetworkData.TotalData.ShortText }
                    }
                },
                {
                    "UploadData", new JObject
                    {
                        { "More", SBMI.NetworkData.UploadData.More },
                        { "Value", SBMI.NetworkData.UploadData.Value },
                        { "Long", (int)SBMI.NetworkData.UploadData.Long },
                        { "Type", (int)SBMI.NetworkData.UploadData.Type },
                        { "Short", (int)SBMI.NetworkData.UploadData.Short },
                        { "LongText", SBMI.NetworkData.UploadData.LongText },
                        { "TypeText", SBMI.NetworkData.UploadData.TypeText },
                        { "ShortText", SBMI.NetworkData.UploadData.ShortText }
                    }
                },
                {
                    "DownloadData", new JObject
                    {
                        { "More", SBMI.NetworkData.DownloadData.More },
                        { "Value", SBMI.NetworkData.DownloadData.Value },
                        { "Long", (int)SBMI.NetworkData.DownloadData.Long },
                        { "Type", (int)SBMI.NetworkData.DownloadData.Type },
                        { "Short", (int)SBMI.NetworkData.DownloadData.Short },
                        { "LongText", SBMI.NetworkData.DownloadData.LongText },
                        { "TypeText", SBMI.NetworkData.DownloadData.TypeText },
                        { "ShortText", SBMI.NetworkData.DownloadData.ShortText }
                    }
                }
            };
        }

        public static JObject GetStorageInfo()
        {
            return new JObject
            {
                { "State", SBMI.StorageData.State },
                { "Drivers", SBMI.StorageData.Drivers },
                { "LogicalDrivers", SBMI.StorageData.LogicalDrivers },
                { "PhysicalDrivers", SBMI.StorageData.PhysicalDrivers }
            };
        }

        public static JObject GetProcessorInfo()
        {
            return new JObject
            {
                { "Max", SBMI.ProcessorData.Max },
                { "Min", SBMI.ProcessorData.Min },
                { "Now", SBMI.ProcessorData.Now },
                { "Core", SBMI.ProcessorData.Core },
                { "Name", SBMI.ProcessorData.Name },
                { "Type", SBMI.ProcessorData.Type },
                { "Cores", SBMI.ProcessorData.Cores },
                { "State", SBMI.ProcessorData.State },
                { "Thread", SBMI.ProcessorData.Thread },
                { "CoreMax", SBMI.ProcessorData.CoreMax },
                { "CoreMin", SBMI.ProcessorData.CoreMin },
                { "CoreNow", SBMI.ProcessorData.CoreNow },
                { "ProcessorCount", SBMI.ProcessorData.ProcessorCount }
            };
        }

        public static JObject GetMotherboardInfo()
        {
            return new JObject
            {
                { "Name", SBMI.MotherboardData.Name },
                { "State", SBMI.MotherboardData.State },
                { "Product", SBMI.MotherboardData.Product },
                { "Version", SBMI.MotherboardData.Version },
                { "Manufacturer", SBMI.MotherboardData.Manufacturer }
            };
        }
    }
}