using SMMG = Sucrose.Manager.Manage.General;
using SPMI = Sucrose.Property.Manage.Internal;
using SRER = Sucrose.Resources.Extension.Resources;

namespace Sucrose.Property.Helper
{
    internal static class Localization
    {
        public static string Convert(string Key)
        {
            if (SPMI.Properties.PropertyLocalization != null && SPMI.Properties.PropertyLocalization.Any())
            {
                StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

                KeyValuePair<string, Dictionary<string, string>> Match = SPMI.Properties.PropertyLocalization.FirstOrDefault(Pair => Comparer.Equals(Pair.Key, SMMG.Culture));

                if (Match.Value is Dictionary<string, string> Pairs && Pairs.TryGetValue(Key, out string Value))
                {
                    return Value;
                }

                Dictionary<string, string> Fallback = SPMI.Properties.PropertyLocalization.First().Value;

                if (Fallback != null && Fallback.TryGetValue(Key, out Value))
                {
                    return Value;
                }
            }

            if (Key.StartsWith("Property.Localization.", StringComparison.Ordinal))
            {
                string Value = SRER.GetValue(Key);

                if (Value != $"[{Key}]")
                {
                    return Value;
                }
            }

            return Key;
        }

        public static string[] Convert(string[] Key)
        {
            for (int Count = 0; Count < Key.Length; Count++)
            {
                Key[Count] = Convert(Key[Count]);
            }

            return Key;
        }
    }
}