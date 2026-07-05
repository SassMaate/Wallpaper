using System.Net;

namespace Sucrose.Memory.Manage.Readonly
{
    public static class General
    {
        public static readonly string Language = "EN";

        public static readonly string Bundle = "Bundle";

        public static readonly Random Randomise = new();

        public static readonly string AppName = "Sucrose";

        public static readonly string Default = "Default";

        public static readonly string Unknown = "Unknown";

        public static readonly string PipeServerName = ".";

        public static readonly char StartCommandChar = '✔';

        public static readonly string ExceptionSplit = " -> ";

        public static readonly char ValueSeparatorChar = '✖';

        public static readonly IPAddress Loopback = IPAddress.Loopback;

        public static readonly string Local = "sucrose.wallpaper.engine";

        public static readonly string StartCommand = $"{StartCommandChar}";

        public static readonly string AppLongName = "Sucrose Wallpaper Engine";

        public static readonly string ValueSeparator = $"{ValueSeparatorChar}";

        public static readonly string UserAgent = "Sucrose/2.4 (Windows NT 10.0; Wallpaper Engine) SucroseWebKit";
    }
}