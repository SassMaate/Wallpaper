using System.Runtime.InteropServices;

namespace Sucrose.Mpv.NET.API.Interop
{
    public static class PlatformDll
    {
        public static IDllLoadUtils Utils { get; private set; } = SelectDllLoadUtils();

        private static IDllLoadUtils SelectDllLoadUtils()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsDllLoadUtils();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxDllLoadUtils();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }
    }
}