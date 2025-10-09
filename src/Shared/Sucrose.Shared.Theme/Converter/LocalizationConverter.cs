using SMMG = Sucrose.Manager.Manage.General;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;
using SSTHL = Sucrose.Shared.Theme.Helper.Localization;

namespace Sucrose.Shared.Theme.Converter
{
    internal class LocalizationConverter
    {
        public static string TitleConvert(SSTHI Info)
        {
            return Convert(Info).Title;
        }

        public static string DescriptionConvert(SSTHI Info)
        {
            return Convert(Info).Description;
        }

        public static string TitleConvertCombine(SSTHI Info)
        {
            return ConvertCombine(Info).Title;
        }

        public static string DescriptionConvertCombine(SSTHI Info)
        {
            return ConvertCombine(Info).Description;
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

        public static (string Title, string Description) Convert(SSTHI Info, string Culture)
        {
            if (string.IsNullOrEmpty(Culture))
            {
                return (Info.Title, Info.Description);
            }
            else
            {
                if (Info.Localization != null && Info.Localization.Any())
                {
                    StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

                    KeyValuePair<string, SSTHL> Match = Info.Localization.FirstOrDefault(Pair => Comparer.Equals(Pair.Key, Culture));

                    if (Match.Value is SSTHL Pairs)
                    {
                        return (Pairs.Title, Pairs.Description);
                    }
                }
            }

            return (string.Empty, string.Empty);
        }

        public static (string Title, string Description) ConvertCombine(SSTHI Info, char Split = ' ')
        {
            if (Info.Localization != null && Info.Localization.Any())
            {
                foreach (SSTHL Localization in Info.Localization.Values)
                {
                    if (!string.IsNullOrWhiteSpace(Localization.Title) && !string.IsNullOrWhiteSpace(Localization.Description))
                    {
                        Info.Title = string.Concat(Info.Title, Split, Localization.Title);
                        Info.Description = string.Concat(Info.Description, Split, Localization.Description);
                    }
                }
            }

            return (Info.Title, Info.Description);
        }
    }
}