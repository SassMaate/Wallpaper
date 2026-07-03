using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using SEAT = Skylark.Enum.AssemblyType;
using SHA = Skylark.Helper.Assemblies;
using SHC = Skylark.Helper.Culture;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;

namespace Sucrose.Resources.Helper
{
    public static class Resources
    {
        private static bool FlowDirectionRegistered;

        private static FlowDirection CurrentFlowDirection = FlowDirection.LeftToRight;

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

            SetFlowDirection(Lang);
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

        /// <summary>
        /// Applies the correct layout direction (RTL for languages such as Arabic/Hebrew, LTR otherwise)
        /// to every Sucrose UI window of the current process, now and for any window opened later.
        /// Registered once via a Window class handler so no individual window needs to opt in.
        /// </summary>
        public static void SetFlowDirection(string Lang)
        {
            CurrentFlowDirection = IsRightToLeft(Lang) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

            RegisterFlowDirection();

            if (Application.Current is null)
            {
                return;
            }

            foreach (Window Window in Application.Current.Windows)
            {
                ApplyFlowDirection(Window);
            }
        }

        private static void RegisterFlowDirection()
        {
            if (FlowDirectionRegistered)
            {
                return;
            }

            FlowDirectionRegistered = true;

            EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.LoadedEvent, new RoutedEventHandler((Sender, Args) =>
            {
                if (Sender is FrameworkElement Element)
                {
                    Window Window = Element is Window W ? W : Window.GetWindow(Element);

                    if (Window != null && !FlowableWindow(Window))
                    {
                        return;
                    }

                    Element.FlowDirection = CurrentFlowDirection;
                }
            }));

        }

        private static void ApplyFlowDirection(Window Window)
        {
            ApplyFlowDirectionToTree(Window);
        }

        private static void ApplyFlowDirectionToTree(DependencyObject Element)
        {
            if (Element is Window Window && !FlowableWindow(Window))
            {
                return;
            }

            if (Element is FrameworkElement FE)
            {
                FE.FlowDirection = CurrentFlowDirection;

                if (FE.ContextMenu != null)
                {
                    FE.ContextMenu.FlowDirection = CurrentFlowDirection;
                }

                if (FE.ToolTip is FrameworkElement ToolTipFE)
                {
                    ToolTipFE.FlowDirection = CurrentFlowDirection;
                }
            }

            if (Element is Visual or Visual3D)
            {
                int ChildrenCount = VisualTreeHelper.GetChildrenCount(Element);

                for (int i = 0; i < ChildrenCount; i++)
                {
                    ApplyFlowDirectionToTree(VisualTreeHelper.GetChild(Element, i));
                }
            }
        }

        private static bool FlowableWindow(Window Window)
        {
            // Engine render surfaces host user content (web/video/gif/image) and must never be mirrored
            // by an RTL FlowDirection. They all live in "Sucrose.Shared.Engine.<Engine>.View", whereas the
            // shared localized dialogs live in "Sucrose.Shared.Engine.View" (no engine segment).
            string Namespace = Window.GetType().Namespace ?? string.Empty;

            return !(Namespace.StartsWith("Sucrose.Shared.Engine.", StringComparison.Ordinal)
                && Namespace.EndsWith(".View", StringComparison.Ordinal)
                && Namespace != "Sucrose.Shared.Engine.View");
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