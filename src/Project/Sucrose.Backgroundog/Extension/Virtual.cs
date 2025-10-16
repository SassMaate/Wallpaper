using System.Diagnostics;
using System.Management;
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

        public static bool VirtualityActive2()
        {
            try
            {
                List<string> Names = SSSHV.GetApp();

                string Conditions = string.Join(" OR ", Names.Select(Name => $"Name = '{Name}'"));
                string Query = $"SELECT * FROM Win32_Process WHERE {Conditions}";

                using ManagementObjectSearcher Searcher = new(Query);

                using ManagementObjectCollection Collection = Searcher.Get();

                return Collection.Count > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}