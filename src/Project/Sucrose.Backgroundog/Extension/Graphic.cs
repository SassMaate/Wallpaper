using System.Diagnostics;

namespace Sucrose.Backgroundog.Extension
{
    internal static class Graphic
    {
        public static string[] GetInstances(string CategoryName)
        {
            PerformanceCounterCategory Category = new(CategoryName);

            return Category.GetInstanceNames();
        }

        public static float GetValue(List<PerformanceCounter> Counters)
        {
            float Value = 0f;

            foreach (PerformanceCounter Counter in Counters)
            {
                try
                {
                    Value += Counter.NextValue();
                }
                catch { }
            }

            return Value;
        }

        public static void InstanceValues(List<PerformanceCounter> Counters)
        {
            foreach (PerformanceCounter Counter in Counters)
            {
                try
                {
                    _ = Counter.NextValue();
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
            string[] CurrentInstances = GetInstances(CategoryName);

            List<string> NewInstances = CurrentInstances.Except(SavedInstances).ToList();

            if (NewInstances.Any())
            {
                PerformanceCounterCategory Category = new(CategoryName);

                foreach (string Instance in NewInstances)
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

                SavedInstances = SavedInstances.Concat(NewInstances).ToArray();

                InstanceValues(Counters);
            }
        }
    }
}