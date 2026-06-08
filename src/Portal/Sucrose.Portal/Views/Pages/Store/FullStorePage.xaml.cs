using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Wpf.Ui.Controls;
using SMMP = Sucrose.Manager.Manage.Portal;
using SMMRF = Sucrose.Memory.Manage.Readonly.Folder;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;
using SPCJVWP = Sucrose.Portal.Controls.JustifiedVirtualizingWrapPanel;
using SPMI = Sucrose.Portal.Manage.Internal;
using SPVMSCVM = Sucrose.Portal.ViewModels.StoreCardViewModel;
using SRER = Sucrose.Resources.Extension.Resources;
using SSSHC = Sucrose.Shared.Space.Helper.Clean;
using SSSIC = Sucrose.Shared.Store.Interface.Category;
using SSSIS = Sucrose.Shared.Store.Interface.Store;
using SSSIW = Sucrose.Shared.Store.Interface.Wallpaper;

namespace Sucrose.Portal.Views.Pages.Store
{
    /// <summary>
    /// FullStorePage.xaml etkileşim mantığı
    /// </summary>
    public partial class FullStorePage : Page, IDisposable
    {
        public static ICollection<NavigationViewItem> MenuItems { get; set; }

        // Auxiliary per-card data used by the ICollectionView filter/sort: the catalog
        // category the wallpaper belongs to, the lower-cased search pattern, and whether
        // the wallpaper is adult-only. Keyed by the card VM so the boolean Filter can
        // resolve category/search/adult without changing the VM's public surface.
        private readonly Dictionary<SPVMSCVM, CardMeta> Meta = [];

        private readonly ObservableCollection<SPVMSCVM> Cards = [];

        private readonly ICollectionView View;

        private CancellationTokenSource _loadCts;

        private ScrollViewer _outerScroll;

        private SSSIS Store = new();

        private string[] _search = [];

        private string _category = string.Empty;

        internal FullStorePage(SSSIS Store)
        {
            this.Store = Store;
            DataContext = this;

            ToolTip SymbolTip = new()
            {
                Content = SRER.GetValue("Portal", "Category", "All")
            };

            ObservableCollection<NavigationViewItem> Categories = [];

            NavigationViewItem AllMenu = new(SRER.GetValue("Portal", "Category", "All"), SPMI.AllIcon, null)
            {
                Tag = string.Empty,
                ToolTip = SymbolTip,
                IsActive = SPMI.CategoryService.CategoryTag == string.Empty
            };

            AllMenu.Click += (s, e) => CategoryClick(s);

            Categories.Add(AllMenu);

            if (Store != null && Store.Categories != null && Store.Categories.Any())
            {
                foreach (KeyValuePair<string, SSSIC> Category in Store.Categories)
                {
                    if (Category.Value.Wallpapers.Any() && (SMMP.StoreAdult || Category.Value.Wallpapers.Count(Wallpaper => Wallpaper.Value.Adult) != Category.Value.Wallpapers.Count()))
                    {
                        SymbolRegular Symbol = SPMI.DefaultIcon;

                        SymbolTip = new()
                        {
                            Content = SRER.GetValue("Portal", "Category", Category.Key.Replace(" ", ""))
                        };

                        if (SPMI.CategoryIcons.TryGetValue(Category.Key, out SymbolRegular Icon))
                        {
                            Symbol = Icon;
                        }

                        NavigationViewItem Menu = new(SRER.GetValue("Portal", "Category", Category.Key.Replace(" ", "")), Symbol, null)
                        {
                            Tag = Category.Key,
                            ToolTip = SymbolTip,
                            IsActive = SPMI.CategoryService.CategoryTag == Category.Key
                        };

                        Menu.Click += (s, e) => CategoryClick(s);

                        Categories.Add(Menu);
                    }
                }
            }

            Categories = [.. Categories.OrderBy(Menu => Menu.Content)];

            Categories.Move(Categories.IndexOf(Categories.FirstOrDefault(Menu => Menu == AllMenu)), 0);

            MenuItems = Categories;

            InitializeComponent();

            View = CollectionViewSource.GetDefaultView(Cards);
            View.Filter = FilterCard;
            ThemeStore.ItemsSource = View;

            Category();
            Search();
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

        private void Search()
        {
            string Search = SPMI.SearchService.SearchText;

            SPMI.SearchService.Dispose();

            SPMI.SearchService = new()
            {
                SearchText = Search
            };

            SPMI.SearchService.SearchTextChanged += SearchService_SearchTextChanged;
        }

        private void Category()
        {
            string Tag = SPMI.CategoryService.CategoryTag;

            SPMI.CategoryService.Dispose();

            SPMI.CategoryService = new()
            {
                CategoryTag = Tag
            };

            SPMI.CategoryService.CategoryTagChanged += CategoryService_CategoryTagChanged;
        }

        private void CategoryClick(object s)
        {
            NavigationViewItem sender = s as NavigationViewItem;

            sender.IsActive = true;

            SPMI.CategoryService.CategoryTag = sender.Tag.ToString();

            CategoryView.MenuItems
                .OfType<NavigationViewItem>()
                .Where(Item => Item.IsActive)
                .ToList()
                .ForEach(Item =>
                {
                    if (Item != sender)
                    {
                        Item.IsActive = false;
                    }
                });
        }

        // The catalog (category + search) is one set filtered in place: category and search
        // are both predicates over the single Store catalog rather than separate catalogs,
        // so a category switch / search-text change re-filters the existing collection via
        // ICollectionView.Refresh() instead of rebuilding the page (mirrors FullLibraryPage).
        private bool FilterCard(object Item)
        {
            if (Item is not SPVMSCVM Card || !Meta.TryGetValue(Card, out CardMeta Info))
            {
                return false;
            }

            // Adult-only wallpapers are hidden unless StoreAdult is enabled (matches the
            // original AddThemes adult guard).
            if (Info.Adult && !SMMP.StoreAdult)
            {
                return false;
            }

            // Category predicate: empty tag ("All") matches everything, otherwise the card's
            // catalog category must equal the active tag.
            if (!string.IsNullOrEmpty(_category) && Info.CategoryKey != _category)
            {
                return false;
            }

            // Search predicate: when there is no search text, everything passes; otherwise the
            // wallpaper's pattern must match every search word (mirrors CountMatchingWords > 0).
            if (_search.Length == 0)
            {
                return true;
            }

            return MatchCount(Info.Pattern, _search) > 0;
        }

        // Mirrors the original CountMatchingWords: counts how many search words appear as a
        // substring of any whitespace-delimited token of the wallpaper pattern.
        private static int MatchCount(string Text, string[] Pattern)
        {
            return Pattern.Count(Word => Text.Split(' ').Any(TextWord => TextWord.Contains(Word)));
        }

        private async void FullStorePage_Loaded(object sender, RoutedEventArgs e)
        {
            _category = SPMI.CategoryService.CategoryTag;
            _search = SPMI.SearchService.SearchList;

            HookViewportConstraint();

            ApplySort();

            await LoadCardsAsync();
        }

        // The wpfui NavigationView hosts pages inside an infinite-height scroll surface, which both
        // defeats virtualization (the panel would be measured unbounded) and adds a second scrollbar.
        // Cap the ItemsControl to the OUTERMOST ancestor ScrollViewer's viewport so the panel is
        // measured with a finite height and the content stays within the visible area.
        private void HookViewportConstraint()
        {
            _outerScroll = FindOutermostScrollViewer(ThemeStore);

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
                ThemeStore.MaxHeight = _outerScroll.ViewportHeight;
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
            // Cancel any in-flight load before rebuilding the catalog collection.
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = new CancellationTokenSource();
            CancellationToken Token = _loadCts.Token;

            Cards.Clear();
            Meta.Clear();

            const int Batch = 12;

            List<SPVMSCVM> Pending = [];

            if (Store != null && Store.Categories != null)
            {
                foreach (KeyValuePair<string, SSSIC> Category in Store.Categories)
                {
                    if (Category.Value?.Wallpapers == null)
                    {
                        continue;
                    }

                    foreach (KeyValuePair<string, SSSIW> Wallpaper in Category.Value.Wallpapers)
                    {
                        if (Token.IsCancellationRequested)
                        {
                            return;
                        }

                        // On-disk cache path FullStorePage previously computed per wallpaper:
                        // <AppData>/<AppName>/Cache/Store/<CategoryKey>/<CleanWallpaperKey>.
                        string Theme = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.Store, Category.Key, SSSHC.FileName(Wallpaper.Key));

                        SPVMSCVM Card = new(Theme, Wallpaper);

                        Meta[Card] = new CardMeta
                        {
                            CategoryKey = Category.Key,
                            Adult = Wallpaper.Value.Adult,
                            Pattern = Wallpaper.Value.Pattern ?? Wallpaper.Key.ToLowerInvariant()
                        };

                        Pending.Add(Card);

                        if (Pending.Count >= Batch)
                        {
                            foreach (SPVMSCVM Item in Pending)
                            {
                                Cards.Add(Item);
                            }

                            Pending.Clear();

                            await Task.Yield();
                        }
                    }
                }
            }

            if (Token.IsCancellationRequested)
            {
                return;
            }

            foreach (SPVMSCVM Item in Pending)
            {
                Cards.Add(Item);
            }

            // The panel DPs are normally supplied via the RelativeSource bindings in XAML, but
            // RelativeSource AncestorType=Page can fail to resolve inside an ItemsPanelTemplate
            // namescope. Apply the values directly once the panel is realized as a guarantee.
            ApplyPanelLayout();

            UpdateEmptyState();
        }

        private void ApplyPanelLayout()
        {
            SPCJVWP Panel = FindPanel(ThemeStore);

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

        // Relevance ordering: while a search is active the original ranked results by descending
        // match count; with no search the catalog keeps its natural order. ListCollectionView
        // CustomSort reproduces that without rebuilding the collection.
        private void ApplySort()
        {
            if (View is ListCollectionView List)
            {
                List.CustomSort = _search.Length == 0 ? null : new RelevanceComparer(Meta, _search);
            }
        }

        private void UpdateEmptyState()
        {
            Empty.Visibility = View.Cast<object>().Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void SearchService_SearchTextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SPMI.SearchService.SearchText) || !string.IsNullOrWhiteSpace(SPMI.SearchService.SearchText))
            {
                _search = SPMI.SearchService.SearchList;

                ApplySort();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    View.Refresh();

                    ScrollToTop();

                    UpdateEmptyState();
                });
            }
        }

        private async void CategoryService_CategoryTagChanged(object sender, EventArgs e)
        {
            _category = SPMI.CategoryService.CategoryTag;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                View.Refresh();

                ScrollToTop();

                UpdateEmptyState();
            });
        }

        private void ScrollToTop()
        {
            ScrollViewer Scroll = ThemeStore.Template?.FindName("PageScroll", ThemeStore) as ScrollViewer;

            Scroll?.ScrollToVerticalOffset(0);
        }

        public void Dispose()
        {
            SPMI.SearchService.SearchTextChanged -= SearchService_SearchTextChanged;
            SPMI.CategoryService.CategoryTagChanged -= CategoryService_CategoryTagChanged;

            if (_outerScroll != null)
            {
                _outerScroll.ScrollChanged -= OuterScroll_ScrollChanged;
                _outerScroll = null;
            }

            _loadCts?.Cancel();
            _loadCts?.Dispose();

            Cards.Clear();
            Meta.Clear();

            GC.SuppressFinalize(this);
        }

        // Per-card data the filter/sort needs without touching the VM's public surface.
        private sealed class CardMeta
        {
            public string CategoryKey { get; init; } = string.Empty;

            public string Pattern { get; init; } = string.Empty;

            public bool Adult { get; init; }
        }

        // Orders cards by descending search-match count (relevance), matching the original
        // GetSortedWallpapers ordering. Ties keep their existing order.
        private sealed class RelevanceComparer(Dictionary<SPVMSCVM, CardMeta> Meta, string[] Search) : System.Collections.IComparer
        {
            private readonly Dictionary<SPVMSCVM, CardMeta> _meta = Meta;

            private readonly string[] _search = Search;

            public int Compare(object X, object Y)
            {
                int CountX = Score(X);
                int CountY = Score(Y);

                return CountY.CompareTo(CountX);
            }

            private int Score(object Item)
            {
                if (Item is SPVMSCVM Card && _meta.TryGetValue(Card, out CardMeta Info))
                {
                    return MatchCount(Info.Pattern, _search);
                }

                return 0;
            }
        }
    }
}
