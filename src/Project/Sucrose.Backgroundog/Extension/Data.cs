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
                { "Millisecond", SBMI.DateData.Millisecond }
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
                { "Voltage", SBMI.BatteryData.Voltage },
                { "ChargeRate", SBMI.BatteryData.ChargeRate },
                { "SavingMode", SBMI.BatteryData.SavingMode },
                { "ChargeLevel", SBMI.BatteryData.ChargeLevel },
                { "LifePercent", SBMI.BatteryData.LifePercent },
                { "SaverStatus", SBMI.BatteryData.SaverStatus },
                { "FullLifetime", SBMI.BatteryData.FullLifetime },
                { "ACPowerStatus", SBMI.BatteryData.ACPowerStatus },
                { "ChargeCurrent", SBMI.BatteryData.ChargeCurrent },
                { "DischargeRate", SBMI.BatteryData.DischargeRate },
                { "LifeRemaining", SBMI.BatteryData.LifeRemaining },
                { "DischargeLevel", SBMI.BatteryData.DischargeLevel },
                { "ChargeStatus", $"{SBMI.BatteryData.ChargeStatus}" },
                { "DesignedCapacity", SBMI.BatteryData.DesignedCapacity },
                { "DegradationLevel", SBMI.BatteryData.DegradationLevel },
                { "DischargeCurrent", SBMI.BatteryData.DischargeCurrent },
                { "RemainingCapacity", SBMI.BatteryData.RemainingCapacity },
                { "PowerLineStatus", $"{SBMI.BatteryData.PowerLineStatus}" },
                { "ChargeDischargeRate", SBMI.BatteryData.ChargeDischargeRate },
                { "FullChargedCapacity", SBMI.BatteryData.FullChargedCapacity },
                { "ChargeDischargeCurrent", SBMI.BatteryData.ChargeDischargeCurrent },
                { "RemainingTimeEstimated", SBMI.BatteryData.RemainingTimeEstimated }
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
                { "Upload", SBMI.NetworkData.Upload },
                { "Download", SBMI.NetworkData.Download },
                { "PingAddress", SBMI.NetworkData.PingAddress },
                { "FormatUploadData", SBMI.NetworkData.FormatUploadData },
                { "FormatDownloadData", SBMI.NetworkData.FormatDownloadData },
                {
                    "PingData", new JObject
                    {
                        { "Ttl", SBMI.NetworkData.PingData.Ttl },
                        { "Buffer", SBMI.NetworkData.PingData.Buffer },
                        { "Address", SBMI.NetworkData.PingData.Address },
                        { "Fragment", SBMI.NetworkData.PingData.Fragment },
                        { "Result", $"{SBMI.NetworkData.PingData.Result}" },
                        { "RoundTrip", SBMI.NetworkData.PingData.RoundTrip }
                    }
                },
                {
                    "UploadData", new JObject
                    {
                        { "Text", SBMI.NetworkData.UploadData.Text },
                        { "Value", SBMI.NetworkData.UploadData.Value },
                        { "Long", $"{SBMI.NetworkData.UploadData.Long}" },
                        { "More", $"{SBMI.NetworkData.UploadData.More}" },
                        { "Type", $"{SBMI.NetworkData.UploadData.Type}" },
                        { "Short", $"{SBMI.NetworkData.UploadData.Short}" }
                    }
                },
                {
                    "DownloadData", new JObject
                    {
                        { "Text", SBMI.NetworkData.DownloadData.Text },
                        { "Value", SBMI.NetworkData.DownloadData.Value },
                        { "Long", $"{SBMI.NetworkData.DownloadData.Long}" },
                        { "More", $"{SBMI.NetworkData.DownloadData.More}" },
                        { "Type", $"{SBMI.NetworkData.DownloadData.Type}" },
                        { "Short", $"{SBMI.NetworkData.DownloadData.Short}" }
                    }
                }
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
                { "State", SBMI.ProcessorData.State },
                { "Thread", SBMI.ProcessorData.Thread },
                { "CoreMax", SBMI.ProcessorData.CoreMax },
                { "CoreMin", SBMI.ProcessorData.CoreMin },
                { "CoreNow", SBMI.ProcessorData.CoreNow },
                { "FullName", SBMI.ProcessorData.FullName }
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