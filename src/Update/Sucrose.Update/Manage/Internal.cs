using Downloader;
using System.IO;
using System.Net;
using SMMG = Sucrose.Manager.Manage.General;
using SMMRF = Sucrose.Memory.Manage.Readonly.Folder;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;
using SSCEFT = Sucrose.Shared.Core.Enum.FrameworkType;
using SSSEPS = Sucrose.Shared.Space.Extension.ProgressStream;
using Timer = System.Timers.Timer;

namespace Sucrose.Update.Manage
{
    internal static class Internal
    {
        public static bool Trying = false;

        public static SSSEPS ProgressStream;

        public static string Source = string.Empty;

        public static DownloadService DownloadService;

        public static bool Chance = SMMRG.Randomise.Next(2) == 0;

        public static SSCEFT DefaultFrameworkType = SSCEFT.NET_Framework_4_8;

        public static string CachePath = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.Bundle);

        public static Timer Checker = new()
        {
            Enabled = false,
            Interval = 1000,
            AutoReset = true
        };

        public static Timer Limiter = new()
        {
            Enabled = false,
            Interval = 1000,
            AutoReset = true
        };

        public static readonly DownloadConfiguration DownloadConfiguration = new()
        {
            RangeLow = 0,
            RangeHigh = 0,
            ChunkCount = 1,
            ParallelCount = 1,
            BlockTimeout = 1000,
            MinimumChunkSize = 0,
            RangeDownload = false,
            BufferBlockSize = 4096,
            MaxTryAgainOnFailure = 1,
            ParallelDownload = false,
            MaximumBytesPerSecond = 0,
            EnableLiveStreaming = false,
            MinimumSizeOfChunking = 1024,
            HttpClientTimeout = 100 * 1000,
            EnableAutoResumeDownload = false,
            CheckDiskSizeBeforeDownload = true,
            DownloadFileExtension = ".sucrose",
            FileExistPolicy = FileExistPolicy.Delete,
            ClearPackageOnCompletionWithFailure = true,
            MaximumMemoryBufferBytes = 1024 * 1024 * 50,
            RequestConfiguration = new()
            {
                Accept = "*/*",
                KeepAlive = false,
                UserAgent = SMMG.UserAgent,
                UseDefaultCredentials = false,
                ProtocolVersion = HttpVersion.Version11
            }
        };
    }
}