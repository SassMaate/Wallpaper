using Sucrose.Shared.Theme.Helper;
using SMMG = Sucrose.Manager.Manage.General;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;
using SSTHL = Sucrose.Shared.Theme.Helper.Localization;

namespace Sucrose.Shared.Theme.Converter
{
    internal class LocalizationConverter
    {
        public static string Title(SSTHI Info)
        {
            return Convert(Info).Title;
        }

        public static (string Title, string Description) Convert(SSTHI Info)
        {
            if (Info.Localization != null && Info.Localization.Any())
            {
                StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

                KeyValuePair<string, SSTHL> Match = Info.Localization.FirstOrDefault(Pair => Comparer.Equals(Pair.Key, SMMG.Culture));

                if (Match.Value is SSTHL Pairs)
                {
                    return (Pairs.Title, Pairs.Description);
                }
            }

            return (Info.Title, Info.Description);
        }

        public static string Description(SSTHI Info)
        {
            return Convert(Info).Description;
        }
    }
}