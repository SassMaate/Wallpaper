using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using SMML = Sucrose.Manager.Manage.Library;
using SMMP = Sucrose.Manager.Manage.Portal;
using SMMRC = Sucrose.Memory.Manage.Readonly.Content;
using SPCJVWP = Sucrose.Portal.Controls.JustifiedVirtualizingWrapPanel;
using SPMI = Sucrose.Portal.Manage.Internal;
using SPVMIH = Sucrose.Portal.ViewModels.ILibraryCardHost;
using SPVMLC = Sucrose.Portal.ViewModels.LibraryCardViewModel;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;

namespace Sucrose.Portal.Views.Pages.Library
{
    /// <summary>
    /// FullLibraryPage.xaml etkileşim mantığı
    /// </summary>
    public partial class FullLibraryPage : Page, SPVMIH, IDisposable
    {
        private readonly Dictionary<string, string> Searches = [];

        private readonly List<string> Themes = [];

        private readonly ObservableCollection<SPVMLC> Cards = [];

        private readonly ICollectionView View;

        private string[] _search = [];

        public FullLibraryPage(Dictionary<string, string> Searches, List<string> Themes)
        {
            this.Themes.AddRange(Themes);
            this.Searches = Searches;

            InitializeComponent();

            View = CollectionViewSource.GetDefaultView(Cards);
            View.Filter = FilterCard;
            ThemeLibrary.ItemsSource = View;
        }

        /// <summary>
        /// Read by the panel's ItemMargin binding (RelativeSource AncestorType=Page) and by
        /// the post-Loaded fallback in case that binding does not resolve across the
        /// ItemsPanelTemplate namescope.
        /// </summary>
        public Thickness CardMargin => new(SMMP.AdaptiveMargin);

        /// <summary>
        /// Read by the panel's MaxItemsPerRow binding (RelativeSource AncestorType=Page) and
        /// by the post-Loaded fallback.
        /// </summary>
        public int CardsPerRow => SMMP.AdaptiveLayout;

        private bool FilterCard(object Item)
        {
            if (_search.Length == 0)
            {
                return true;
            }

            if (Item is SPVMLC Card)
            {
                string Name = Path.GetFileName(Card.Theme);
                string Haystack = Searches.TryGetValue(Name, out string Value) ? Value : $"{Card.Title} {Card.Description}";

                return _search.All(Word => Haystack.Split(' ').Any(Part => Part.Contains(Word)));
            }

            return false;
        }

        private async void FullLibraryPage_Loaded(object sender, RoutedEventArgs e)
        {
            _search = SPMI.SearchService.SearchList;

            await LoadCardsAsync();
        }

        private async Task LoadCardsAsync()
        {
            Cards.Clear();

            List<string> Valid = Themes.Where(Theme => Directory.Exists(Path.Combine(SMML.Location, Theme))).ToList();

            const int Batch = 12;

            List<SPVMLC> Pending = [];

            foreach (string Theme in Valid)
            {
                string ThemePath = Path.Combine(SMML.Location, Theme);

                // SSTHI.ReadJson is pure file IO + JSON parsing, safe off the UI thread.
                SSTHI Info = await Task.Run(() => SSTHI.ReadJson(Path.Combine(ThemePath, SMMRC.SucroseInfo)));

                Pending.Add(new SPVMLC(ThemePath, Info, this));

                if (Pending.Count >= Batch)
                {
                    foreach (SPVMLC Card in Pending)
                    {
                        Cards.Add(Card);
                    }

                    Pending.Clear();

                    await Task.Yield();
                }
            }

            foreach (SPVMLC Card in Pending)
            {
                Cards.Add(Card);
            }

            // The panel DPs are normally supplied via the RelativeSource bindings in XAML, but
            // RelativeSource AncestorType=Page can fail to resolve inside an ItemsPanelTemplate
            // namescope. Apply the values directly once the panel is realized as a guarantee.
            ApplyPanelLayout();

            UpdateEmptyState();
        }

        private void ApplyPanelLayout()
        {
            SPCJVWP Panel = FindPanel(ThemeLibrary);

            if (Panel != null)
            {
                Panel.ItemMargin = CardMargin;
                Panel.MaxItemsPerRow = CardsPerRow;
            }
        }

        private static SPCJVWP FindPanel(DependencyObject Root)
        {
            if (Root is SPCJVWP Found)
            {
                return Found;
            }

            int Count = VisualTreeHelper.GetChildrenCount(Root);

            for (int Index = 0; Index < Count; Index++)
            {
                SPCJVWP Result = FindPanel(VisualTreeHelper.GetChild(Root, Index));

                if (Result != null)
                {
                    return Result;
                }
            }

            return null;
        }

        private void UpdateEmptyState()
        {
            Empty.Visibility = View.Cast<object>().Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        public void Remove(SPVMLC ViewModel)
        {
            string Name = Path.GetFileName(ViewModel.Theme);

            Cards.Remove(ViewModel);
            Themes.Remove(Name);
            Searches.Remove(Name);

            UpdateEmptyState();
        }

        public void Refresh()
        {
            _search = SPMI.SearchService.SearchList;

            View.Refresh();

            UpdateEmptyState();
        }

        public void Dispose()
        {
            Cards.Clear();

            GC.SuppressFinalize(this);
        }
    }
}
