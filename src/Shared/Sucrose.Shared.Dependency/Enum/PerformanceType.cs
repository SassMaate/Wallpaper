namespace Sucrose.Shared.Dependency.Enum
{
    internal enum PerformanceType
    {
        Close,
        Pause,
        Resume
    }

    internal enum PausePerformanceType
    {
        Heavy,
        Light
    }

    internal enum NetworkPerformanceType
    {
        Not,
        Ping,
        Upload,
        Download
    }

    internal enum CategoryPerformanceType
    {
        Not,
        Lock,
        Focus,
        Sleep,
        Memory,
        Remote,
        Battery,
        Console,
        Graphic,
        Network,
        Session,
        Virtual,
        Processor,
        FullScreen,
        ScreenSaver,
        BatterySaver
    }
}