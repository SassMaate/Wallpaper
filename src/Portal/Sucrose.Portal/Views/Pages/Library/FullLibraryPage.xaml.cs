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

        private CancellationTokenSource _loadCts;

        private ScrollViewer _outerScroll;

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

            HookViewportConstraint();

            await LoadCardsAsync();
        }

        // The wpfui NavigationView hosts pages inside an infinite-height scroll surface, which both
        // defeats virtualization (the panel would be measured unbounded) and adds a second scrollbar.
        // Cap the ItemsControl to the OUTERMOST ancestor ScrollViewer's viewport so the panel is
        // measured with a finite height and the content stays within the visible area.
        private void HookViewportConstraint()
        {
            _outerScroll = FindOutermostScrollViewer(ThemeLibrary);

            if (_outerScroll != null)
            {
                _outerScroll.ScrollChanged += OuterScroll_ScrollChanged;
            }

            Dispatcher.BeginInvoke(new Action(ApplyViewportConstraint), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void OuterScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ViewportHeightChange != 0)
            {
                ApplyViewportConstraint();
            }
        }

        private void ApplyViewportConstraint()
        {
            if (_outerScroll != null && _outerScroll.ViewportHeight > 0)
            {
                ThemeLibrary.MaxHeight = _outerScroll.ViewportHeight;
            }
        }

        private static ScrollViewer FindOutermostScrollViewer(DependencyObject Start)
        {
            ScrollViewer Found = null;
            DependencyObject Current = VisualTreeHelper.GetParent(Start);

            while (Current != null)
            {
                if (Current is ScrollViewer Scroll)
                {
                    Found = Scroll;
                }

                Current = VisualTreeHelper.GetParent(Current);
            }

            return Found;
        }

        private async Task LoadCardsAsync()
        {
            // Cancel any in-flight load: the parent LibraryPage disposes and rebuilds this
            // page on every search-text change, so a stale load must stop adding cards.
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = new CancellationTokenSource();
            CancellationToken Token = _loadCts.Token;

            Cards.Clear();

            List<string> Valid = Themes.Where(Theme => Directory.Exists(Path.Combine(SMML.Location, Theme))).ToList();

            const int Batch = 12;

            List<SPVMLC> Pending = [];

            foreach (string Theme in Valid)
            {
                if (Token.IsCancellationRequested)
                {
                    return;
                }

                string ThemePath = Path.Combine(SMML.Location, Theme);

                // SSTHI.ReadJson is pure file IO + JSON parsing, safe off the UI thread.
                SSTHI Info = await Task.Run(() => SSTHI.ReadJson(Path.Combine(ThemePath, SMMRC.SucroseInfo)));

                if (Token.IsCancellationRequested)
                {
                    return;
                }

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

            if (Token.IsCancellationRequested)
            {
                return;
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
            if (_outerScroll != null)
            {
                _outerScroll.ScrollChanged -= OuterScroll_ScrollChanged;
                _outerScroll = null;
            }

            _loadCts?.Cancel();
            _loadCts?.Dispose();

            Cards.Clear();

            GC.SuppressFinalize(this);
        }
    }
}
