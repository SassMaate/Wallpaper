using System.Diagnostics;
using System.Management;
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

        public static bool RemotelyActive2()
        {
            try
            {
                List<string> Names = SSSHR.GetApp();

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