using SBSCS = Sucrose.Backgroundog.Struct.CoreSensor;

namespace Sucrose.Backgroundog.Extension
{
    internal static class Core
    {
        public static List<SBSCS> UpdateExtremes(List<SBSCS> Sensors)
        {
            if (Sensors?.Count > 0)
            {
                IEnumerable<float> Values = Sensors.Where(Sensor => Sensor.Now.HasValue).Select(Sensor => Sensor.Now.Value);

                if (Values.Any())
                {
                    float Max = Values.Max(), Min = Values.Min();

                    for (int Index = 0; Index < Sensors.Count; Index++)
                    {
                        SBSCS Sensor = Sensors[Index];

                        Sensor.IsMax = Sensor.Now >= Max;
                        Sensor.IsMin = Sensor.Now <= Min;

                        Sensors[Index] = Sensor;
                    }
                }
            }

            return Sensors;
        }
    }
}