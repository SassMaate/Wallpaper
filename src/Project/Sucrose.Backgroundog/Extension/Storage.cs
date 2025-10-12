using System.Diagnostics;

namespace Sucrose.Backgroundog.Extension
{
    internal static class Storage
    {
        public static string[] GetInstances(string CategoryName)
        {
            PerformanceCounterCategory Category = new(CategoryName);

            return Category.GetInstanceNames();
        }

        public static void InstanceValues(List<(string Instance, PerformanceCounter Write, PerformanceCounter Read)> Counters)
        {
            foreach ((_, PerformanceCounter Write, PerformanceCounter Read) in Counters)
            {
                try
                {
                    _ = Read.NextValue();
                    _ = Write.NextValue();
                }
                catch { }
            }
        }

        public static List<(string Instance, PerformanceCounter Write, PerformanceCounter Read)> CreateCounters(string CategoryName)
        {
            List<(string, PerformanceCounter, PerformanceCounter)> Counters = new();

            foreach (string Instance in GetInstances(CategoryName))
            {
                try
                {
                    PerformanceCounter Read = new(CategoryName, "Disk Read Bytes/sec", Instance, true);
                    PerformanceCounter Write = new(CategoryName, "Disk Write Bytes/sec", Instance, true);

                    Counters.Add((Instance, Write, Read));
                }
                catch { }
            }

            return Counters;
        }

        public static List<(string Instance, float Write, float Read)> GetValues(List<(string Instance, PerformanceCounter Write, PerformanceCounter Read)> Counters)
        {
            List<(string Instance, float Write, float Read)> Values = new();

            foreach ((string Instance, PerformanceCounter Write, PerformanceCounter Read) in Counters)
            {
                try
                {
                    Values.Add((Instance, Write.NextValue(), Read.NextValue()));
                }
                catch { }
            }

            return Values;
        }
    }
}