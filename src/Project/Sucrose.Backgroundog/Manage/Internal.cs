using LibreHardwareMonitor.Hardware;
using NPSMLib;
using System.Diagnostics;
using SBEAV = Sucrose.Backgroundog.Extension.AudioVisualizer;
using SBEWL = Sucrose.Backgroundog.Extension.WindowsListener;
using SBHI = Sucrose.Backgroundog.Helper.Initialize;
using SBSDA = Sucrose.Backgroundog.Struct.Data.Audio;
using SBSDBS = Sucrose.Backgroundog.Struct.Data.Bios;
using SBSDBY = Sucrose.Backgroundog.Struct.Data.Battery;
using SBSDD = Sucrose.Backgroundog.Struct.Data.Date;
using SBSDG = Sucrose.Backgroundog.Struct.Data.Graphic;
using SBSDMD = Sucrose.Backgroundog.Struct.Data.Motherboard;
using SBSDMY = Sucrose.Backgroundog.Struct.Data.Memory;
using SBSDN = Sucrose.Backgroundog.Struct.Data.Network;
using SBSDP = Sucrose.Backgroundog.Struct.Data.Processor;
using SSDECPT = Sucrose.Shared.Dependency.Enum.CategoryPerformanceType;
using SSDENPT = Sucrose.Shared.Dependency.Enum.NetworkPerformanceType;
using SSDEPPT = Sucrose.Shared.Dependency.Enum.PausePerformanceType;
using SSDEPT = Sucrose.Shared.Dependency.Enum.PerformanceType;
using SWHESM = Skylark.Wing.Helper.EventSubscriptionManager;
using Timer = System.Threading.Timer;

namespace Sucrose.Backgroundog.Manage
{
    internal static class Internal
    {
        public static bool Exit = true;

        public static Process App = null;

        public static int AppTime = 1000;

        public static Process Live = null;

        public static Process[] Apps = null;

        public static SBEAV AudioVisualizer;

        public static bool Condition = false;

        public static bool Processing = true;

        public static bool FullScreen = false;

        public static SBHI Initialize = new();

        public static bool Virtuality = false;

        public static int InitializeTime = 100;

        public static SSDEPPT PausePerformance;

        public static bool WindowsLock = false;

        public static bool FocusDesktop = false;

        public static bool WindowsSleep = false;

        public static bool BiosManagement = true;

        public static bool PingManagement = true;

        public static bool PipeManagement = true;

        public static bool RemoteDesktop = false;

        public static bool WindowsConsole = true;

        public static bool WindowsRemote = false;

        public static bool WindowsSession = true;

        public static bool AudioManagement = true;

        public static bool FocusManagement = true;

        public static Timer InitializeTimer = null;

        public static bool RemoteManagement = true;

        public static bool SignalManagement = true;

        public static SBEWL WindowsListener = null;

        public static bool BatteryManagement = true;

        public static bool GraphicManagement = true;

        public static bool NetworkManagement = true;

        public static bool SessionManagement = true;

        public static bool ComputerManagement = true;

        public static bool GraphicManagement2 = true;

        public static bool NetworkManagement2 = true;

        public static string PingHost = string.Empty;

        public static bool EqualizerManagement = true;

        public static bool ProcessorManagement = true;

        public static bool WindowsScreenSaver = false;

        public static bool FullScreenManagement = true;

        public static bool ProcessorManagement2 = true;

        public static bool VirtualityManagement = true;

        public static bool MotherboardManagement = true;

        public static string PingAddress = string.Empty;

        public static SWHESM SubscriptionManager = null;

        public static readonly object LockObject = new();

        public static SSDEPT Performance = SSDEPT.Resume;

        public static bool TransmissionManagement = true;

        public static NowPlayingSession PlayingSession = null;

        public static PerformanceCounter UploadCounter = null;

        public static SSDENPT NetworkPerformance = SSDENPT.Not;

        public static SSDECPT CategoryPerformance = SSDECPT.Not;

        public static MediaPlaybackDataSource DataSource = null;

        public static PerformanceCounter DownloadCounter = null;

        public static PerformanceCounter ProcessorCounter = null;

        public static int SpecificationTime = InitializeTime * 20;

        public static PerformanceCounter[] ProcessorsCounter = null;

        public static NowPlayingSessionManager SessionManager = null;

        public static int SpecificationMaxTime = InitializeTime * 30;

        public static int SpecificationLessTime = InitializeTime * 10;

        public static string[] GraphicInterfaces = Array.Empty<string>();

        public static string[] NetworkInterfaces = Array.Empty<string>();

        public static SBSDBS BiosData = new()
        {
            State = false,
            Name = string.Empty,
            Caption = string.Empty,
            Version = string.Empty,
            Description = string.Empty,
            ReleaseDate = string.Empty,
            Manufacturer = string.Empty,
            SerialNumber = string.Empty,
            CurrentLanguage = string.Empty
        };

        public static SBSDD DateData = new()
        {
            Day = 0,
            Hour = 0,
            Year = 0,
            Month = 0,
            Minute = 0,
            Second = 0,
            DayOfYear = 0,
            State = false,
            Millisecond = 0,
            DayOfWeek = DayOfWeek.Sunday,
            Kind = DateTimeKind.Unspecified
        };

        public static SBSDA AudioData = new()
        {
            //PID = 0,
            State = false,
            TrackNumber = 0,
            PlaybackRate = 0d,
            //Hwnd = IntPtr.Zero,
            AlbumTrackCount = 0,
            Title = string.Empty,
            Artist = string.Empty,
            ShuffleEnabled = false,
            Subtitle = string.Empty,
            EndTime = TimeSpan.Zero,
            Position = TimeSpan.Zero,
            AlbumTitle = string.Empty,
            StartTime = TimeSpan.Zero,
            AlbumArtist = string.Empty,
            SourceAppId = string.Empty,
            MinSeekTime = TimeSpan.Zero,
            MaxSeekTime = TimeSpan.Zero,
            LastPlayingFileTime = new(),
            PositionSetFileTime = new(),
            Data = Array.Empty<double>(),
            //SourceDeviceId = string.Empty,
            //RenderDeviceId = string.Empty,
            ThumbnailString = string.Empty,
            MediaType = MediaPlaybackMode.Unknown,
            PlaybackMode = MediaPlaybackMode.Unknown,
            PlaybackState = MediaPlaybackState.Unknown,
            PropsValid = MediaPlaybackProps.Capabilities,
            RepeatMode = MediaPlaybackRepeatMode.Unknown,
            //PlaybackCaps = MediaPlaybackCapabilities.None
        };

        public static SBSDMY MemoryData = new()
        {
            State = false,
            MemoryLoad = 0f,
            MemoryUsed = 0f,
            Name = string.Empty,
            MemoryAvailable = 0f,
            VirtualMemoryLoad = 0f,
            VirtualMemoryUsed = 0f,
            VirtualName = string.Empty,
            VirtualMemoryAvailable = 0f
        };

        public static Computer Computer = new()
        {
            IsGpuEnabled = true,
            IsCpuEnabled = false,
            IsPsuEnabled = false,
            IsMemoryEnabled = true,
            IsBatteryEnabled = true,
            IsNetworkEnabled = false,
            IsStorageEnabled = false,
            IsControllerEnabled = false,
            IsMotherboardEnabled = false
        };

        public static SBSDG GraphicData = new()
        {
            Amd = new(),
            State = false,
            Intel = new(),
            Nvidia = new(),
            Name = string.Empty,
            Manufacturer = string.Empty
        };

        public static SBSDN NetworkData = new()
        {
            Ping = 0,
            Upload = 0f,
            State = false,
            Download = 0f,
            PingData = new(),
            UploadData = new(),
            Name = string.Empty,
            DownloadData = new(),
            FormatUploadData = string.Empty,
            FormatDownloadData = string.Empty
        };

        public static SBSDBY BatteryData = new()
        {
            Voltage = 0f,
            State = false,
            ChargeRate = 0f,
            ChargeLevel = 0f,
            LifePercent = 0f,
            FullLifetime = 0,
            LifeRemaining = 0,
            ChargeCurrent = 0f,
            DischargeRate = 0f,
            SavingMode = false,
            DischargeLevel = 0f,
            Name = string.Empty,
            DischargeCurrent = 0f,
            DegradationLevel = 0f,
            DesignedCapacity = 0f,
            RemainingCapacity = 0f,
            FullChargedCapacity = 0f,
            ChargeDischargeRate = 0f,
            ChargeDischargeCurrent = 0f,
            RemainingTimeEstimated = 0f,
            ACPowerStatus = string.Empty,
            PowerLineStatus = PowerLineStatus.Unknown,
            ChargeStatus = BatteryChargeStatus.Unknown
        };

        public static SBSDP ProcessorData = new()
        {
            Max = 0f,
            Min = 0f,
            Now = 0f,
            Core = 0,
            Thread = 0,
            CoreMax = 0f,
            CoreMin = 0f,
            CoreNow = 0f,
            Cores = new(),
            State = false,
            ProcessorCount = 0,
            Name = string.Empty
        };

        public static SBSDMD MotherboardData = new()
        {
            State = false,
            Name = string.Empty,
            Product = string.Empty,
            Version = string.Empty,
            Manufacturer = string.Empty
        };
    }
}