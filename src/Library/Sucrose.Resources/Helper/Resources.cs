using System.Globalization;
using System.Windows;
using SEAT = Skylark.Enum.AssemblyType;
using SHA = Skylark.Helper.Assemblies;
using SHC = Skylark.Helper.Culture;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;

namespace Sucrose.Resources.Helper
{
    public static class Resources
    {
        public static void SetLanguage(string Lang)
        {
            Lang = Lang.ToUpperInvariant();

            if (!CheckLanguage(Lang))
            {
                Lang = SMMRG.Language;
            }

            ResourceDictionary Resource = new()
            {
                Source = new Uri($"/Sucrose.Resources;component/Locales/Locale.{Lang}.xaml", UriKind.Relative)
            };

            RemoveResource();

            SHC.All = new CultureInfo(Lang, true);

            Application.Current.Resources.MergedDictionaries.Add(Resource);
        }

        public static bool IsRightToLeft(string Lang)
        {
            try
            {
                CultureInfo Culture = new(Lang);

                return Culture.TextInfo.IsRightToLeft;
            }
            catch
            {
                return false;
            }
        }

        private static bool CheckLanguage(string Lang)
        {
            try
            {
                return Application.LoadComponent(new Uri($"/Sucrose.Resources;component/Locales/Locale.{Lang}.xaml", UriKind.Relative)) is ResourceDictionary;
            }
            catch
            {
                return false;
            }
        }

        public static List<string> ListLanguage()
        {
            return
            [
                "AR",
                "CS",
                "DA",
                "DE",
                "EL",
                "EN",
                "ES",
                "FR",
                "HI",
                "ID",
                "IT",
                "JA",
                "KO",
                "MS",
                "NB",
                "NL",
                "PL",
                "PT",
                "RO",
                "RU",
                "SV",
                "TR",
                "UK",
                "ZH"
            ];
        }

        public static List<string> ListLanguages()
        {
            return SHA.Assemble(SEAT.Entry)
                .GetManifestResourceNames()
                .Where(Resource => Resource.Contains("Locales/Locale.") && Resource.EndsWith(".xaml"))
                .Select(Resource =>
                {
                    int StartIndex = Resource.LastIndexOf("Locale.") + "Locale.".Length;
                    int EndIndex = Resource.LastIndexOf(".xaml");

                    return StartIndex < EndIndex ? Resource[StartIndex..EndIndex] : null;
                })
                .Where(LangCode => LangCode != null)
                .ToList();
        }

        public static List<string> ListLanguageManipulated()
        {
            List<string> Languages = ListLanguage();

            Languages.Insert(0, string.Empty);

            return Languages;
        }

        private static void RemoveResource()
        {
            List<ResourceDictionary> Resources = Application.Current.Resources.MergedDictionaries
                .Where(Resource => !string.IsNullOrEmpty(Resource.Source?.ToString()) && Resource.Source.ToString().Contains("Locales/"))
                .ToList();

            foreach (ResourceDictionary Resource in Resources)
            {
                Application.Current.Resources.MergedDictionaries.Remove(Resource);
            }
        }
    }
}