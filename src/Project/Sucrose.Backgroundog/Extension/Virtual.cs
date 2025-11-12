using System.Diagnostics;
using SSSHV = Sucrose.Shared.Space.Helper.Virtual;

namespace Sucrose.Backgroundog.Extension
{
    internal static class Virtual
    {
        public static bool VirtualityActive()
        {
            try
            {
                HashSet<string> Names = new(SSSHV.GetApp(), StringComparer.OrdinalIgnoreCase);

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