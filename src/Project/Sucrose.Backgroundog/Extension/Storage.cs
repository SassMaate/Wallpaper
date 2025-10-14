using System.Diagnostics;

namespace Sucrose.Backgroundog.Extension
{
    internal static class Storage
    {
        public static string[] GetInstances(string CategoryName)
        {
            try
            {
                PerformanceCounterCategory Category = new(CategoryName);

                return Category.GetInstanceNames();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        public static void InstanceValues(List<(string Instance, PerformanceCounter Write, PerformanceCounter Read)> Counters)
        {
            List<(string Instance, PerformanceCounter Write, PerformanceCounter Read)> InvalidCounters = new();

            foreach ((string Instance, PerformanceCounter Write, PerformanceCounter Read) in Counters)
            {
                try
                {
                    _ = Read.NextValue();
                    _ = Write.NextValue();
                }
                catch
                {
                    InvalidCounters.Add((Instance, Write, Read));
                }
            }

            foreach ((string Instance, PerformanceCounter Write, PerformanceCounter Read) in InvalidCounters)
            {
                try
                {
                    Read?.Dispose();
                    Write?.Dispose();
                    Counters.Remove((Instance, Write, Read));
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
            List<(string Instance, PerformanceCounter Write, PerformanceCounter Read)> InvalidCounters = new();
            List<(string Instance, float Write, float Read)> Values = new();

            foreach ((string Instance, PerformanceCounter Write, PerformanceCounter Read) in Counters)
            {
                try
                {
                    Values.Add((Instance, Write.NextValue(), Read.NextValue()));
                }
                catch
                {
                    InvalidCounters.Add((Instance, Write, Read));
                }
            }

            foreach ((string Instance, PerformanceCounter Write, PerformanceCounter Read) in InvalidCounters)
            {
                try
                {
                    Read?.Dispose();
                    Write?.Dispose();
                    Counters.Remove((Instance, Write, Read));
                }
                catch { }
            }

            return Values;
        }
    }
}