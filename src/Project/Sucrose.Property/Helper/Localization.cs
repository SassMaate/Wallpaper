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
                if (SPMI.Properties.PropertyLocalization.TryGetValue(SMMG.Culture, out Dictionary<string, string> Pairs) || SPMI.Properties.PropertyLocalization.TryGetValue(SMMG.Culture.ToLower(), out Pairs) || SPMI.Properties.PropertyLocalization.TryGetValue(SMMG.Culture.ToUpper(), out Pairs) || SPMI.Properties.PropertyLocalization.TryGetValue(SMMG.Culture.ToLower(), out Pairs) || SPMI.Properties.PropertyLocalization.TryGetValue(SMMG.Culture.ToUpperInvariant(), out Pairs))
                {
                    if (Pairs != null && Pairs.TryGetValue(Key, out string Value))
                    {
                        return Value;
                    }
                }

                if (SPMI.Properties.PropertyLocalization.TryGetValue(SPMI.Properties.PropertyLocalization.First().Key, out Pairs))
                {
                    if (Pairs != null && Pairs.TryGetValue(Key, out string Value))
                    {
                        return Value;
                    }
                }
            }

            if (Key.StartsWith("Property.Localization."))
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