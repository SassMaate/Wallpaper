using System.Diagnostics;
using SSSHR = Sucrose.Shared.Space.Helper.Remote;

namespace Sucrose.Backgroundog.Extension
{
    internal static class Remote
    {
        public static bool RemotelyActive()
        {
            try
            {
                HashSet<string> Names = new(SSSHR.GetApp(), StringComparer.OrdinalIgnoreCase);

                foreach (Process Process in Process.GetProcesses())
                {
                    if (Names.Contains(Process?.ProcessName + ".exe"))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}