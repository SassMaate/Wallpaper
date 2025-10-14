namespace Sucrose.Backgroundog.Extension
{
    internal static class Numeric
    {
        public static (T Min, T Max) UpdateMinMax<T>(T Min, T Max, T Value) where T : IComparable<T>
        {
            if (Value.CompareTo(Max) > 0)
            {
                Max = Value;
            }

            if (Value.CompareTo(Min) < 0)
            {
                Min = Value;
            }

            return (Min, Max);
        }

        public static void UpdateMinMax<T>(this ref T Min, ref T Max, T Value) where T : struct, IComparable<T>
        {
            if (Value.CompareTo(Max) > 0)
            {
                Max = Value;
            }

            if (Value.CompareTo(Min) < 0)
            {
                Min = Value;
            }
        }
    }
}