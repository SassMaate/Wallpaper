using SMMG = Sucrose.Manager.Manage.General;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;
using SSTHL = Sucrose.Shared.Theme.Helper.Localization;

namespace Sucrose.Portal.Helpers
{
    internal static class Localization
    {
        public static (string, string) Convert(SSTHI Info)
        {
            if (Info.Localization != null && Info.Localization.Any())
            {
                if (Info.Localization.TryGetValue(SMMG.Culture, out SSTHL Pairs) || Info.Localization.TryGetValue(SMMG.Culture.ToLower(), out Pairs) || Info.Localization.TryGetValue(SMMG.Culture.ToUpper(), out Pairs) || Info.Localization.TryGetValue(SMMG.Culture.ToLower(), out Pairs) || Info.Localization.TryGetValue(SMMG.Culture.ToUpperInvariant(), out Pairs))
                {
                    if (Pairs != null)
                    {
                        return (Pairs.Title, Pairs.Description);
                    }
                }
            }

            return (Info.Title, Info.Description);
        }
    }
}