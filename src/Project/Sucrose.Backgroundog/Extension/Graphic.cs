using System.Diagnostics;

namespace Sucrose.Backgroundog.Extension
{
    internal static class Graphic
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

        public static float GetValue(List<PerformanceCounter> Counters)
        {
            float Value = 0f;
            List<PerformanceCounter> InvalidCounters = new();

            foreach (PerformanceCounter Counter in Counters)
            {
                try
                {
                    Value += Counter.NextValue();
                }
                catch
                {
                    InvalidCounters.Add(Counter);
                }
            }

            foreach (PerformanceCounter InvalidCounter in InvalidCounters)
            {
                try
                {
                    InvalidCounter?.Dispose();
                    Counters.Remove(InvalidCounter);
                }
                catch { }
            }

            return Value;
        }

        public static void InstanceValues(List<PerformanceCounter> Counters)
        {
            List<PerformanceCounter> InvalidCounters = new();

            foreach (PerformanceCounter Counter in Counters)
            {
                try
                {
                    _ = Counter.NextValue();
                }
                catch
                {
                    InvalidCounters.Add(Counter);
                }
            }

            foreach (PerformanceCounter InvalidCounter in InvalidCounters)
            {
                try
                {
                    InvalidCounter?.Dispose();
                    Counters.Remove(InvalidCounter);
                }
                catch { }
            }
        }

        public static List<PerformanceCounter> CreateCounters(string CategoryName, string Luid)
        {
            List<PerformanceCounter> Counters = new();

            foreach (string Instance in GetInstances(CategoryName))
            {
                try
                {
                    if (Instance.EndsWith("engtype_3D") && Instance.Contains(Luid))
                    {
                        PerformanceCounterCategory Category = new(CategoryName);

                        foreach (PerformanceCounter Counter in Category.GetCounters(Instance))
                        {
                            if (Counter.CounterName.Equals("Utilization Percentage"))
                            {
                                Counters.Add(Counter);
                            }
                        }
                    }
                }
                catch { }
            }

            return Counters;
        }

        public static void UpdateCounters(List<PerformanceCounter> Counters, ref string[] SavedInstances, string CategoryName, string Luid)
        {
            try
            {
                string[] CurrentInstances = GetInstances(CategoryName);

                List<string> RemovedInstances = SavedInstances.Except(CurrentInstances).ToList();

                if (RemovedInstances.Any())
                {
                    List<PerformanceCounter> InvalidCounters = new();
                    foreach (PerformanceCounter Counter in Counters)
                    {
                        try
                        {
                            if (RemovedInstances.Any(Counter.InstanceName.Contains))
                            {
                                InvalidCounters.Add(Counter);
                            }
                        }
                        catch { }
                    }

                    foreach (PerformanceCounter InvalidCounter in InvalidCounters)
                    {
                        try
                        {
                            InvalidCounter?.Dispose();
                            Counters.Remove(InvalidCounter);
                        }
                        catch { }
                    }
                }

                List<string> NewInstances = CurrentInstances.Except(SavedInstances).ToList();

                if (NewInstances.Any())
                {
                    PerformanceCounterCategory Category = new(CategoryName);

                    foreach (string Instance in NewInstances)
                    {
                        try
                        {
                            if (Instance.EndsWith("engtype_3D") && Instance.Contains(Luid))
                            {
                                foreach (PerformanceCounter Counter in Category.GetCounters(Instance))
                                {
                                    if (Counter.CounterName.Equals("Utilization Percentage"))
                                    {
                                        Counters.Add(Counter);
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    InstanceValues(Counters);
                }

                SavedInstances = CurrentInstances;
            }
            catch { }
        }
    }
}