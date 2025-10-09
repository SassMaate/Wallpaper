using System.IO;
using SMML = Sucrose.Manager.Manage.Library;
using SMMRC = Sucrose.Memory.Manage.Readonly.Content;
using SSDESKT = Sucrose.Shared.Dependency.Enum.SortKindType;
using SSDESMT = Sucrose.Shared.Dependency.Enum.SortModeType;
using SSDMMP = Sucrose.Shared.Dependency.Manage.Manager.Portal;
using SSSHR = Sucrose.Shared.Space.Helper.Regexer;
using SSTCLC = Sucrose.Shared.Theme.Converter.LocalizationConverter;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;

namespace Sucrose.Shared.Space.Helper
{
    internal static class Sort
    {
        public static List<string> Theme(List<string> Themes)
        {
            Dictionary<string, object> SortThemes = new();

            foreach (string Theme in Themes.ToList())
            {
                string ThemePath = Path.Combine(SMML.Location, Theme);
                string InfoPath = Path.Combine(ThemePath, SMMRC.SucroseInfo);

                if (Directory.Exists(ThemePath) && File.Exists(InfoPath))
                {
                    SSTHI Info = SSTHI.ReadJson(InfoPath);

                    if (SSDMMP.LibrarySortMode == SSDESMT.Name)
                    {
                        SortThemes.TryAdd(Theme, SSTCLC.TitleConvert(Info));
                    }
                    else if (SSDMMP.LibrarySortMode == SSDESMT.Creation)
                    {
                        SortThemes.TryAdd(Theme, Directory.GetCreationTime(Path.Combine(SMML.Location, Theme)));
                    }
                    else if (SSDMMP.LibrarySortMode == SSDESMT.Modification)
                    {
                        SortThemes.TryAdd(Theme, File.GetLastWriteTime(InfoPath));
                    }
                }
            }

            if (SortThemes != null && SortThemes.Any())
            {
                if (SSDMMP.LibrarySortKind == SSDESKT.Ascending)
                {
                    SortThemes = SortThemes.OrderBy(Theme => Theme.Value).ToDictionary(Theme => Theme.Key, Theme => Theme.Value);
                }
                else
                {
                    SortThemes = SortThemes.OrderByDescending(Theme => Theme.Value).ToDictionary(Theme => Theme.Key, Theme => Theme.Value);
                }

                Themes.Clear();
                Themes.AddRange(SortThemes.Select(Theme => Theme.Key));
            }

            return Themes;
        }

        public static (List<string>, Dictionary<string, string>) Theme(List<string> Themes, Dictionary<string, string> Searches)
        {
            Dictionary<string, object> SortThemes = new();

            foreach (string Theme in Themes.ToList())
            {
                string ThemePath = Path.Combine(SMML.Location, Theme);
                string InfoPath = Path.Combine(ThemePath, SMMRC.SucroseInfo);

                if (Directory.Exists(ThemePath) && File.Exists(InfoPath))
                {
                    SSTHI Info = SSTHI.ReadJson(InfoPath);

                    IEnumerable<string> SearchText = SSTCLC.TitleConvertCombine(Info).Split(' ')
                        .Concat(SSTCLC.DescriptionConvertCombine(Info).Split(' '))
                        .Concat(Info.Tags?.SelectMany(Tag => Tag.Split(' ')) ?? Array.Empty<string>());

                    Searches.TryAdd(Theme, SSSHR.RemoveExtraSpaces(string.Join(" ", SearchText.Select(Word => SSSHR.RemovePunctuation(Word).ToLowerInvariant()).Distinct())));

                    if (SSDMMP.LibrarySortMode == SSDESMT.Name)
                    {
                        SortThemes.TryAdd(Theme, SSTCLC.TitleConvert(Info));
                    }
                    else if (SSDMMP.LibrarySortMode == SSDESMT.Creation)
                    {
                        SortThemes.TryAdd(Theme, Directory.GetCreationTime(Path.Combine(SMML.Location, Theme)));
                    }
                    else if (SSDMMP.LibrarySortMode == SSDESMT.Modification)
                    {
                        SortThemes.TryAdd(Theme, File.GetLastWriteTime(InfoPath));
                    }
                }
            }

            if (SortThemes != null && SortThemes.Any())
            {
                if (SSDMMP.LibrarySortKind == SSDESKT.Ascending)
                {
                    SortThemes = SortThemes.OrderBy(Theme => Theme.Value).ToDictionary(Theme => Theme.Key, Theme => Theme.Value);
                }
                else
                {
                    SortThemes = SortThemes.OrderByDescending(Theme => Theme.Value).ToDictionary(Theme => Theme.Key, Theme => Theme.Value);
                }

                Themes.Clear();
                Themes.AddRange(SortThemes.Select(Theme => Theme.Key));
            }

            return (Themes, Searches);
        }
    }
}