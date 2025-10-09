using SMMG = Sucrose.Manager.Manage.General;
using SRER = Sucrose.Resources.Extension.Resources;
using SSTHL = Sucrose.Shared.Theme.Helper.Localization;

namespace Sucrose.Portal.Helpers
{
    internal static class Localization
    {
        public static string Convert(string Key)
        {
            //if (SPMI.Properties.PropertyLocalization != null && SPMI.Properties.PropertyLocalization.Any())
            //{
            //    if (SPMI.Properties.PropertyLocalization.TryGetValue(SMMG.Culture, out Dictionary<string, string> Pairs) || SPMI.Properties.PropertyLocalization.TryGetValue(SMMG.Culture.ToLower(), out Pairs) || SPMI.Properties.PropertyLocalization.TryGetValue(SMMG.Culture.ToUpper(), out Pairs) || SPMI.Properties.PropertyLocalization.TryGetValue(SMMG.Culture.ToLower(), out Pairs) || SPMI.Properties.PropertyLocalization.TryGetValue(SMMG.Culture.ToUpperInvariant(), out Pairs))
            //    {
            //        if (Pairs != null && Pairs.TryGetValue(Key, out string Value))
            //        {
            //            return Value;
            //        }
            //    }

            //    if (SPMI.Properties.PropertyLocalization.TryGetValue(SPMI.Properties.PropertyLocalization.First().Key, out Pairs))
            //    {
            //        if (Pairs != null && Pairs.TryGetValue(Key, out string Value))
            //        {
            //            return Value;
            //        }
            //    }
            //}

            return Key;
        }
    }
}