# Portal Library/Store Virtualization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the non-virtualized Portal Library/Store card lists with a custom justified UI-virtualizing panel and full-MVVM data-bound cards, removing pagination and forced GC, to eliminate scroll stutter.

**Architecture:** A single custom `VirtualizingPanel`+`IScrollInfo` panel renders only the visible rows of an `ObservableCollection<CardViewModel>` (uniform 160px row height makes scroll extent exact). Cards become binding-driven `DataTemplate`s with all logic on the VM. Theme metadata loads incrementally in the background; search uses an `ICollectionView` filter.

**Tech Stack:** .NET 10.0-windows, WPF, WPF-UI 4.3.0, CommunityToolkit.Mvvm 8.4.2, XamlAnimatedGif, Skylark.Wing.

---

## Conventions for this plan

- **No test suite exists** (confirmed in CLAUDE.md). "Verify" in each task means: `dotnet build` succeeds **and** the listed manual UAT passes. TDD unit-test steps are intentionally replaced by build-verify + UAT, plus temporary diagnostic logging for non-visual logic.
- **Build verify command** (run from repo root):
  `dotnet build src/Portal/Sucrose.Portal/Sucrose.Portal.csproj -c Release -p:PlatformTarget=x64`
  Expected: `Build succeeded`. Style violations fail the build (`EnforceCodeStyleInBuild=true`), so match `.editorconfig` (4-space indent, CRLF, block-scoped namespaces, namespace alias `using`s).
- **Namespace alias convention is mandatory** (see CLAUDE.md). Every Sucrose `using` is aliased by first-letters, e.g. `using SPVCLC = Sucrose.Portal.Views.Controls.LibraryCard;`.
- **Run the app for UAT:** build the solution and launch the Portal exe, or use the publish script. Open Settings → set a large library/store to scroll.
- **Branch:** work on `feature/portal-virtualization` (already created; the spec is committed there).
- **Commit after every task.** Keep the unrelated `libmpv-*.dll` working-tree changes unstaged — never `git add -A`; stage explicit paths only.

## Shared interface contract (use these names verbatim across all tasks)

```
Sucrose.Portal.Controls.JustifiedVirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
  DP Thickness ItemMargin      (default new Thickness(0),  AffectsMeasure)
  DP int       MaxItemsPerRow  (default int.MaxValue,      AffectsMeasure)
  DP double    ItemMinWidth    (default 260d,              AffectsMeasure)
  DP double    ItemMaxWidth    (default 400d,              AffectsMeasure)
  DP double    ItemHeight      (default 160d,              AffectsMeasure)

Sucrose.Portal.Extension.ImageCache  (static)
  static Task<ImageSource> GetAsync(string path, int decodeWidth = 360, CancellationToken ct = default)

Sucrose.Portal.ViewModels.CardViewModelBase : ObservableObject     // CommunityToolkit.Mvvm
  [ObservableProperty] string theme
  [ObservableProperty] string title
  [ObservableProperty] string description
  [ObservableProperty] ImageSource thumbnail
  [ObservableProperty] bool isLoading            // starts true
  [ObservableProperty] bool isIncompatible
  abstract string ThumbnailPath { get; }
  abstract string PreviewPath   { get; }         // hover GIF source
  Task LoadThumbnailAsync(CancellationToken ct)

Sucrose.Portal.ViewModels.ILibraryCardHost
  void Remove(LibraryCardViewModel vm)

Sucrose.Portal.ViewModels.LibraryCardViewModel : CardViewModelBase
  ctor(string theme, SSTHI info, ILibraryCardHost host)
  SSTHI Info; bool Delete
  [ObservableProperty] bool canUse, canDelete, canCustomize, cyclingAddVisible, cyclingRemoveVisible
  [ObservableProperty] string useHeader, deleteHeader, customizeHeader
  void RefreshMenuState()
  [RelayCommand] Use, Find, Preview, Customize, CyclingAdd, CyclingRemove, Edit, Share, Review, DeleteEntry, Update

Sucrose.Portal.ViewModels.IStoreCardHost  { void Remove(StoreCardViewModel vm); }
Sucrose.Portal.ViewModels.StoreCardViewModel : CardViewModelBase   // analogous, remote-aware
```

---

## Task 1: GC quick wins (Workstation GC + remove forced GC.Collect)

Lowest-risk, highest-immediate-impact change; shippable on its own.

**Files:**
- Modify: `Directory.Build.targets` (GC settings)
- Modify (remove `GC.Collect();` lines): `src/Portal/Sucrose.Portal/Extension/ImageLoader.cs:45`,
  `src/Portal/Sucrose.Portal/Views/Controls/LibraryCard.xaml.cs:436,454,512,536`,
  `src/Portal/Sucrose.Portal/Views/Controls/StoreCard.xaml.cs` (the 4 `GC.Collect()` sites ~450,464,554,575),
  `src/Portal/Sucrose.Portal/Views/Pages/Library/FullLibraryPage.xaml.cs:159`,
  `src/Portal/Sucrose.Portal/Views/Pages/Store/FullStorePage.xaml.cs` (the `GC.Collect()` in `Dispose`),
  `src/Portal/Sucrose.Portal/Controls/LibraryStackPanel.cs:163`,
  `src/Portal/Sucrose.Portal/Controls/StoreStackPanel.cs:163`,
  `src/Portal/Sucrose.Portal/Extension/ThumbnailLoader.cs:116`

- [ ] **Step 1: Inspect current GC settings.** Open `Directory.Build.targets`, find the
  `<ServerGarbageCollection>true</ServerGarbageCollection>` and
  `<ConcurrentGarbageCollection>true</ConcurrentGarbageCollection>` block (~lines 7-17) and read the
  condition that scopes it (it currently applies to all non-Library outputs).

- [ ] **Step 2: Scope Workstation GC to UI processes.** Add an override so the Portal (and other
  WPF UI exes) use Workstation GC. Preferred: add to the Portal csproj a property that wins for that
  output. Edit `src/Portal/Sucrose.Portal/Sucrose.Portal.csproj`, inside the main
  `<PropertyGroup>`, add:

```xml
<ServerGarbageCollection>false</ServerGarbageCollection>
<ConcurrentGarbageCollection>false</ConcurrentGarbageCollection>
```

  (csproj-level properties override `Directory.Build.targets` defaults. Concurrent GC is irrelevant
  once Server GC is off; setting it false keeps the runtimeconfig explicit. Do **not** edit the
  global default in `Directory.Build.targets` — that would change background services too.)

- [ ] **Step 3: Remove every forced `GC.Collect();` in the Portal files listed above.** Delete the
  `GC.Collect();` line; keep any surrounding `GC.SuppressFinalize(this);` in `Dispose` methods. For
  example in `ImageLoader.cs`:

```csharp
// BEFORE
public void Dispose()
{
    GC.Collect();
    GC.SuppressFinalize(this);
}

// AFTER
public void Dispose()
{
    GC.SuppressFinalize(this);
}
```

  Apply the same deletion at each listed site. Use Grep to confirm none remain:
  `Grep pattern "GC\.Collect" path "src/Portal/Sucrose.Portal"` → expect zero matches.

- [ ] **Step 4: Build verify.**
  Run: `dotnet build src/Portal/Sucrose.Portal/Sucrose.Portal.csproj -c Release -p:PlatformTarget=x64`
  Expected: `Build succeeded`.

- [ ] **Step 5: UAT.** Launch Portal, open Library and Store, hover and scroll. Expected:
  noticeably fewer hitches than before (this is a partial fix; full smoothness comes after
  virtualization). Confirm no functional regressions (cards load, hover preview works, delete works).

- [ ] **Step 6: Commit.**

```bash
git add Directory.Build.targets src/Portal/Sucrose.Portal/Sucrose.Portal.csproj src/Portal/Sucrose.Portal/Extension/ImageLoader.cs src/Portal/Sucrose.Portal/Extension/ThumbnailLoader.cs src/Portal/Sucrose.Portal/Views/Controls/LibraryCard.xaml.cs src/Portal/Sucrose.Portal/Views/Controls/StoreCard.xaml.cs src/Portal/Sucrose.Portal/Views/Pages/Library/FullLibraryPage.xaml.cs src/Portal/Sucrose.Portal/Views/Pages/Store/FullStorePage.xaml.cs src/Portal/Sucrose.Portal/Controls/LibraryStackPanel.cs src/Portal/Sucrose.Portal/Controls/StoreStackPanel.cs
git commit -m "perf(portal): use Workstation GC and remove forced UI-thread GC.Collect"
```

---

## Task 2: Shared bounded image cache (`ImageCache`)

**Files:**
- Create: `src/Portal/Sucrose.Portal/Extension/ImageCache.cs`

- [ ] **Step 1: Write the cache.** A process-wide, size-bounded (LRU) cache returning frozen
  `BitmapImage`s decoded off-thread, mirroring the proven decode options already in `ImageLoader`
  (`OnLoad`, `DecodePixelWidth`, `Freeze`). Concurrent in-flight loads for the same key are shared.

```csharp
using System.Collections.Concurrent;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sucrose.Portal.Extension
{
    public static class ImageCache
    {
        private const int Capacity = 240; // ~enough for several screens of cards

        private static readonly object Gate = new();
        private static readonly LinkedList<string> Lru = new();
        private static readonly Dictionary<string, (LinkedListNode<string> Node, ImageSource Image)> Map = [];
        private static readonly ConcurrentDictionary<string, Task<ImageSource>> InFlight = [];

        public static async Task<ImageSource> GetAsync(string Path, int DecodeWidth = 360, CancellationToken Token = default)
        {
            if (string.IsNullOrEmpty(Path) || !File.Exists(Path))
            {
                return null;
            }

            string Key = $"{Path}|{DecodeWidth}";

            lock (Gate)
            {
                if (Map.TryGetValue(Key, out (LinkedListNode<string> Node, ImageSource Image) Hit))
                {
                    Lru.Remove(Hit.Node);
                    Lru.AddFirst(Hit.Node);
                    return Hit.Image;
                }
            }

            Task<ImageSource> Load = InFlight.GetOrAdd(Key, _ => Task.Run(() => Decode(Path, DecodeWidth), Token));

            try
            {
                ImageSource Image = await Load.WaitAsync(Token);
                Store(Key, Image);
                return Image;
            }
            finally
            {
                InFlight.TryRemove(Key, out _);
            }
        }

        private static ImageSource Decode(string Path, int DecodeWidth)
        {
            using FileStream Stream = new(Path, FileMode.Open, FileAccess.Read, FileShare.Read);

            BitmapImage Image = new();
            Image.BeginInit();
            Image.CacheOption = BitmapCacheOption.OnLoad;
            Image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            Image.DecodePixelWidth = DecodeWidth;
            Image.StreamSource = Stream;
            Image.EndInit();

            if (Image.CanFreeze)
            {
                Image.Freeze();
            }

            return Image;
        }

        private static void Store(string Key, ImageSource Image)
        {
            if (Image == null)
            {
                return;
            }

            lock (Gate)
            {
                if (Map.ContainsKey(Key))
                {
                    return;
                }

                LinkedListNode<string> Node = Lru.AddFirst(Key);
                Map[Key] = (Node, Image);

                while (Map.Count > Capacity && Lru.Last != null)
                {
                    string Evict = Lru.Last.Value;
                    Lru.RemoveLast();
                    Map.Remove(Evict);
                }
            }
        }
    }
}
```

- [ ] **Step 2: Build verify.**
  Run: `dotnet build src/Portal/Sucrose.Portal/Sucrose.Portal.csproj -c Release -p:PlatformTarget=x64`
  Expected: `Build succeeded`.

- [ ] **Step 3: Commit.**

```bash
git add src/Portal/Sucrose.Portal/Extension/ImageCache.cs
git commit -m "perf(portal): add bounded LRU image cache for card thumbnails"
```

---

## Task 3: `CardViewModelBase` + `LibraryCardViewModel`

Port the data and all action/menu logic out of `LibraryCard.xaml.cs` into view models. **Read
`src/Portal/Sucrose.Portal/Views/Controls/LibraryCard.xaml.cs` in full first** — port its methods
faithfully (the alias `using`s at the top of that file are the exact ones to reuse).

**Files:**
- Create: `src/Portal/Sucrose.Portal/ViewModels/CardViewModelBase.cs`
- Create: `src/Portal/Sucrose.Portal/ViewModels/ILibraryCardHost.cs`
- Create: `src/Portal/Sucrose.Portal/ViewModels/LibraryCardViewModel.cs`

- [ ] **Step 1: `CardViewModelBase`.**

```csharp
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sucrose.Portal.ViewModels
{
    public abstract partial class CardViewModelBase : ObservableObject
    {
        [ObservableProperty]
        private string _theme = string.Empty;

        [ObservableProperty]
        private string _title = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private ImageSource _thumbnail;

        [ObservableProperty]
        private bool _isLoading = true;

        [ObservableProperty]
        private bool _isIncompatible;

        public abstract string ThumbnailPath { get; }

        public abstract string PreviewPath { get; }

        public async Task LoadThumbnailAsync(CancellationToken Token)
        {
            IsLoading = true;

            ImageSource Image = await Sucrose.Portal.Extension.ImageCache.GetAsync(ThumbnailPath, 360, Token);

            if (!Token.IsCancellationRequested)
            {
                Thumbnail = Image;
                IsLoading = false;
            }
        }
    }
}
```

- [ ] **Step 2: `ILibraryCardHost`.** The page implements this so a VM can remove itself from the
  bound `ObservableCollection`.

```csharp
namespace Sucrose.Portal.ViewModels
{
    public interface ILibraryCardHost
    {
        void Remove(LibraryCardViewModel ViewModel);
    }
}
```

- [ ] **Step 3: `LibraryCardViewModel`.** Port every handler from `LibraryCard.xaml.cs`. Reuse the
  same alias `using`s. Commands replace `*_Click`; `RefreshMenuState()` replaces
  `ContextMenu_Opened`; `Use()` is copied verbatim; `DeleteEntry()` ports `MenuDelete_Click` +
  removes itself via the host instead of the `Visibility`/`IsVisibleChanged` trick.

```csharp
using System.IO;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Controls;
using SHV = Skylark.Helper.Versionly;
using SMMB = Sucrose.Manager.Manage.Backgroundog;
using SMMC = Sucrose.Manager.Manage.Cycling;
using SMMCC = Sucrose.Memory.Manage.Constant.Cycling;
using SMMCL = Sucrose.Memory.Manage.Constant.Library;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMML = Sucrose.Manager.Manage.Library;
using SMMRC = Sucrose.Memory.Manage.Readonly.Content;
using SMMRF = Sucrose.Memory.Manage.Readonly.Folder;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;
using SPVCTD = Sucrose.Portal.Views.Controls.ThemeDelete;
using SPVCTE = Sucrose.Portal.Views.Controls.ThemeEdit;
using SPVCTR = Sucrose.Portal.Views.Controls.ThemeReview;
using SPVCTS = Sucrose.Portal.Views.Controls.ThemeShare;
using SRER = Sucrose.Resources.Extension.Resources;
using SSDECT = Sucrose.Shared.Dependency.Enum.CommandType;
using SSDEET = Sucrose.Shared.Dependency.Enum.EngineType;
using SSDEWT = Sucrose.Shared.Dependency.Enum.WallpaperType;
using SSDMME = Sucrose.Shared.Dependency.Manage.Manager.Engine;
using SSLHK = Sucrose.Shared.Live.Helper.Kill;
using SSLHR = Sucrose.Shared.Live.Helper.Run;
using SSSHF = Sucrose.Shared.Space.Helper.Filing;
using SSSHL = Sucrose.Shared.Space.Helper.Live;
using SSSHP = Sucrose.Shared.Space.Helper.Processor;
using SSSMI = Sucrose.Shared.Space.Manage.Internal;
using SSTCLC = Sucrose.Shared.Theme.Converter.LocalizationConverter;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;

namespace Sucrose.Portal.ViewModels
{
    public partial class LibraryCardViewModel : CardViewModelBase
    {
        private readonly ILibraryCardHost _host;

        public SSTHI Info { get; private set; }

        public bool Delete { get; private set; }

        [ObservableProperty]
        private bool _canUse;

        [ObservableProperty]
        private bool _canDelete;

        [ObservableProperty]
        private bool _canCustomize;

        [ObservableProperty]
        private bool _cyclingAddVisible;

        [ObservableProperty]
        private bool _cyclingRemoveVisible;

        [ObservableProperty]
        private string _useHeader = string.Empty;

        [ObservableProperty]
        private string _deleteHeader = string.Empty;

        [ObservableProperty]
        private string _customizeHeader = string.Empty;

        public LibraryCardViewModel(string Theme, SSTHI Info, ILibraryCardHost Host)
        {
            _host = Host;
            this.Info = Info;
            this.Theme = Theme;

            (Title, Description) = SSTCLC.Convert(Info);
            IsIncompatible = Info.AppVersion.CompareTo(SHV.Entry()) > 0;
        }

        public override string ThumbnailPath => Path.Combine(Theme, Info.Thumbnail);

        public override string PreviewPath => Path.Combine(Theme, Info.Preview);

        // Ported verbatim from LibraryCard.Use()
        private void Use()
        {
            if (Directory.Exists(Theme))
            {
                if ((!SMMB.ClosePerformance && !SMMB.PausePerformance) || !SSSHP.Work(SSSMI.Backgroundog))
                {
                    if (SMML.Selected != Path.GetFileName(Theme) || !SSSHL.Run())
                    {
                        SMMI.LibrarySettingManager.SetSetting(SMMCL.Selected, Path.GetFileName(Theme));

                        if (SSSHL.Run())
                        {
                            SSLHK.Stop();
                        }

                        SSLHR.Start();
                    }
                }
            }
        }

        [RelayCommand]
        private void UseEntry()
        {
            if (!IsIncompatible)
            {
                Use();
            }
        }

        [RelayCommand]
        private void Find()
        {
            if (Directory.Exists(Theme))
            {
                SSSHP.Run(Theme);
            }
        }

        [RelayCommand]
        private void Customize()
        {
            if (Directory.Exists(Theme))
            {
                SSSHP.Run(SSSMI.Commandog, $"{SMMRG.StartCommand}{SSDECT.PropertyA}{SMMRG.ValueSeparator}{SSSMI.Property}{SMMRG.ValueSeparator}{Path.GetFileName(Theme)}");
            }
        }

        [RelayCommand]
        private void CyclingAdd()
        {
            List<string> Exclusion = SMMC.Exclusion;

            if (Exclusion.Contains(Path.GetFileName(Theme)))
            {
                Exclusion.Remove(Path.GetFileName(Theme));
                SMMI.CyclingSettingManager.SetSetting(SMMCC.Exclusion, Exclusion);
            }
        }

        [RelayCommand]
        private void CyclingRemove()
        {
            List<string> Exclusion = SMMC.Exclusion;

            if (!Exclusion.Contains(Path.GetFileName(Theme)))
            {
                Exclusion.Add(Path.GetFileName(Theme));
                SMMI.CyclingSettingManager.SetSetting(SMMCC.Exclusion, Exclusion);
            }
        }

        [RelayCommand]
        private async Task Edit()
        {
            if (Directory.Exists(Theme))
            {
                SPVCTE ThemeEdit = new() { Info = Info, Theme = Theme };
                ContentDialogResult Result = await ThemeEdit.ShowAsync();

                if (Result == ContentDialogResult.Primary)
                {
                    Info = SSTHI.ReadJson(Path.Combine(Theme, SMMRC.SucroseInfo));
                    (Title, Description) = SSTCLC.Convert(Info);
                }

                ThemeEdit.Dispose();
            }
        }

        [RelayCommand]
        private async Task Share()
        {
            if (Directory.Exists(Theme))
            {
                SPVCTS ThemeShare = new() { Info = Info, Theme = Theme };
                await ThemeShare.ShowAsync();
                ThemeShare.Dispose();
            }
        }

        [RelayCommand]
        private async Task Review()
        {
            if (Directory.Exists(Theme))
            {
                SPVCTR ThemeReview = new() { Info = Info, Theme = Theme };
                await ThemeReview.ShowAsync();
                ThemeReview.Dispose();
            }
        }

        [RelayCommand]
        private void Update()
        {
            if (!SSSHP.Work(SSSMI.Update))
            {
                SSSHP.Run(SSSMI.Commandog, $"{SMMRG.StartCommand}{SSDECT.Update}{SMMRG.ValueSeparator}{SSSMI.Update}");
            }
        }

        [RelayCommand]
        private async Task DeleteEntry()
        {
            bool Confirm = SMML.DeleteConfirm;
            ContentDialogResult Result = ContentDialogResult.None;

            if (Confirm)
            {
                SPVCTD ThemeDelete = new() { Info = Info, Theme = Theme };
                Result = await ThemeDelete.ShowAsync();
                ThemeDelete.Dispose();
            }

            if (!Confirm || Result == ContentDialogResult.Primary)
            {
                Delete = true;
                _host.Remove(this);

                await Task.Run(() =>
                {
                    string PropertiesCache = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.Properties);

                    if (Directory.Exists(PropertiesCache))
                    {
                        foreach (string Record in Directory.GetFiles(PropertiesCache, ""))
                        {
                            if (File.Exists(Record) && Record.Contains(Path.GetFileName(Theme)))
                            {
                                SSSHF.Delete(Record);
                            }
                        }
                    }
                });

                await Task.Run(() =>
                {
                    if (Directory.Exists(Theme))
                    {
                        Directory.Delete(Theme, true);
                    }
                });
            }
        }

        // Ported from ContextMenu_Opened. Call from the template's ContextMenu.Opened (code-behind
        // forwarder) or a behavior. Read state live; set the observable menu-state properties.
        public void RefreshMenuState()
        {
            UseHeader = SRER.GetValue("Portal", "LibraryCard", "MenuUse");
            DeleteHeader = SRER.GetValue("Portal", "LibraryCard", "MenuDelete");
            CustomizeHeader = SRER.GetValue("Portal", "LibraryCard", "MenuCustomize");

            string PropertiesPath = Path.Combine(Theme, SMMRC.SucroseProperties);

            // --- CanCustomize: port the exact nested matrix from LibraryCard.ContextMenu_Opened ---
            if (Info.Type == SSDEWT.Web && File.Exists(PropertiesPath))
            {
                CanCustomize = true;
            }
            else if (Info.Type is SSDEWT.Gif or SSDEWT.Video or SSDEWT.YouTube)
            {
                CanCustomize = ResolveEngineCustomize(PropertiesPath);
            }
            else
            {
                CanCustomize = false;
            }

            // --- Cycling visibility ---
            if (SMMC.Active)
            {
                bool Excluded = SMMC.Exclusion.Contains(Path.GetFileName(Theme));
                CyclingAddVisible = Excluded;
                CyclingRemoveVisible = !Excluded;
            }
            else
            {
                CyclingAddVisible = false;
                CyclingRemoveVisible = false;
            }

            // --- Use/Delete enablement + header suffixes ---
            if ((!SMMB.ClosePerformance && !SMMB.PausePerformance) || !SSSHP.Work(SSSMI.Backgroundog))
            {
                if (SMML.Selected == Path.GetFileName(Theme) && SSSHL.Run())
                {
                    CanUse = false;
                    CanDelete = false;
                    string Tag = $" ({SRER.GetValue("Portal", "LibraryCard", "Selected")})";
                    UseHeader += Tag;
                    DeleteHeader += Tag;
                }
                else
                {
                    if (!IsIncompatible)
                    {
                        CanUse = true;
                    }
                    else
                    {
                        CanUse = false;
                        UseHeader += $" ({SRER.GetValue("Portal", "LibraryCard", "Incompatible")})";
                    }

                    CanDelete = true;
                }
            }
            else
            {
                CanUse = false;
                CanDelete = false;
                CanCustomize = false;

                string Key = SMMB.ClosePerformance ? "Closed" : "Paused";
                string Tag = $" ({SRER.GetValue("Portal", "LibraryCard", Key)})";
                UseHeader += Tag;
                DeleteHeader += Tag;
                CustomizeHeader += Tag;
            }
        }

        private bool ResolveEngineCustomize(string PropertiesPath)
        {
            string Cache(string Engine) => Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, Engine, SMMRC.SucroseProperties);

            if (Info.Type == SSDEWT.Gif)
            {
                return (SSDMME.Gif == SSDEET.MpvPlayerLive && (File.Exists(PropertiesPath) || File.Exists(Cache(SMMRF.MpvPlayer))))
                    || (SSDMME.Gif == SSDEET.CefSharpLive && File.Exists(Cache(SMMRF.CefSharp)))
                    || (SSDMME.Gif == SSDEET.WebViewLive && File.Exists(Cache(SMMRF.WebView2)));
            }

            if (Info.Type == SSDEWT.Video)
            {
                return (SSDMME.Video == SSDEET.MpvPlayerLive && (File.Exists(PropertiesPath) || File.Exists(Cache(SMMRF.MpvPlayer))))
                    || (SSDMME.Video == SSDEET.CefSharpLive && File.Exists(Cache(SMMRF.CefSharp)))
                    || (SSDMME.Video == SSDEET.WebViewLive && File.Exists(Cache(SMMRF.WebView2)));
            }

            if (Info.Type == SSDEWT.YouTube)
            {
                return (SSDMME.YouTube == SSDEET.CefSharpLive && File.Exists(Cache(SMMRF.CefSharp)))
                    || (SSDMME.YouTube == SSDEET.WebViewLive && File.Exists(Cache(SMMRF.WebView2)));
            }

            return false;
        }
    }
}
```

> Note: `[RelayCommand] UseEntry` generates `UseEntryCommand`; `DeleteEntry` → `DeleteEntryCommand`.
> Cross-check the generated command names when binding in Task 5.

- [ ] **Step 4: Build verify.**
  Run: `dotnet build src/Portal/Sucrose.Portal/Sucrose.Portal.csproj -c Release -p:PlatformTarget=x64`
  Expected: `Build succeeded`. (Resolve any alias/signature mismatches against the real helpers.)

- [ ] **Step 5: Commit.**

```bash
git add src/Portal/Sucrose.Portal/ViewModels/CardViewModelBase.cs src/Portal/Sucrose.Portal/ViewModels/ILibraryCardHost.cs src/Portal/Sucrose.Portal/ViewModels/LibraryCardViewModel.cs
git commit -m "feat(portal): add card view models with ported library card logic"
```

---

## Task 4: `JustifiedVirtualizingWrapPanel`

The crux. A recycling `VirtualizingPanel` implementing `IScrollInfo`, reproducing
`LibraryStackPanel.DistributeExtraSpace` per realized row. Uniform row height (`ItemHeight`) makes
extent and the visible-row range exact.

**Files:**
- Create: `src/Portal/Sucrose.Portal/Controls/JustifiedVirtualizingWrapPanel.cs`

- [ ] **Step 1: Write the panel.** This is a full implementation; expect to iterate it against the
  compiler and live scrolling (Step 2-4). Read the WPF `IScrollInfo`/`VirtualizingPanel`
  realization pattern if unfamiliar.

```csharp
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Sucrose.Portal.Controls
{
    public class JustifiedVirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        private const int CacheRows = 2;

        private Size _extent;
        private Size _viewport;
        private Point _offset;

        public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.Register(nameof(ItemMargin), typeof(Thickness), typeof(JustifiedVirtualizingWrapPanel), new FrameworkPropertyMetadata(new Thickness(0), FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty MaxItemsPerRowProperty = DependencyProperty.Register(nameof(MaxItemsPerRow), typeof(int), typeof(JustifiedVirtualizingWrapPanel), new FrameworkPropertyMetadata(int.MaxValue, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty ItemMinWidthProperty = DependencyProperty.Register(nameof(ItemMinWidth), typeof(double), typeof(JustifiedVirtualizingWrapPanel), new FrameworkPropertyMetadata(260d, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty ItemMaxWidthProperty = DependencyProperty.Register(nameof(ItemMaxWidth), typeof(double), typeof(JustifiedVirtualizingWrapPanel), new FrameworkPropertyMetadata(400d, FrameworkPropertyMetadataOptions.AffectsMeasure));
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(JustifiedVirtualizingWrapPanel), new FrameworkPropertyMetadata(160d, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Thickness ItemMargin { get => (Thickness)GetValue(ItemMarginProperty); set => SetValue(ItemMarginProperty, value); }
        public int MaxItemsPerRow { get => (int)GetValue(MaxItemsPerRowProperty); set => SetValue(MaxItemsPerRowProperty, value); }
        public double ItemMinWidth { get => (double)GetValue(ItemMinWidthProperty); set => SetValue(ItemMinWidthProperty, value); }
        public double ItemMaxWidth { get => (double)GetValue(ItemMaxWidthProperty); set => SetValue(ItemMaxWidthProperty, value); }
        public double ItemHeight { get => (double)GetValue(ItemHeightProperty); set => SetValue(ItemHeightProperty, value); }

        private double RowHeight => ItemHeight + ItemMargin.Top + ItemMargin.Bottom;
        private double CellWidth => ItemMinWidth + ItemMargin.Left + ItemMargin.Right;

        private int ItemsPerRow(double availableWidth)
        {
            int perRow = Math.Max(1, (int)Math.Floor(availableWidth / CellWidth));

            if (MaxItemsPerRow > 0)
            {
                perRow = Math.Min(perRow, MaxItemsPerRow);
            }

            return perRow;
        }

        private int ItemCount
        {
            get
            {
                ItemsControl owner = ItemsControl.GetItemsOwner(this);
                return owner?.Items.Count ?? 0;
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (double.IsInfinity(availableSize.Width))
            {
                availableSize.Width = ItemMaxWidth;
            }

            if (double.IsInfinity(availableSize.Height))
            {
                availableSize.Height = RowHeight;
            }

            int count = ItemCount;
            int perRow = ItemsPerRow(availableSize.Width);
            int rows = perRow > 0 ? (count + perRow - 1) / perRow : 0;

            Size extent = new(availableSize.Width, rows * RowHeight);
            UpdateScrollInfo(availableSize, extent);

            int firstRow = Math.Max(0, (int)Math.Floor(_offset.Y / RowHeight) - CacheRows);
            int lastRow = Math.Min(Math.Max(0, rows - 1), (int)Math.Ceiling((_offset.Y + availableSize.Height) / RowHeight) + CacheRows);

            int firstItem = firstRow * perRow;
            int lastItem = Math.Min(count - 1, ((lastRow + 1) * perRow) - 1);

            double cellInnerWidth = availableSize.Width / perRow;

            IItemContainerGenerator generator = ItemContainerGenerator;
            GeneratorPosition startPos = generator.GeneratorPositionFromIndex(firstItem);
            int childIndex = startPos.Offset == 0 ? startPos.Index : startPos.Index + 1;

            using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                for (int itemIndex = firstItem; itemIndex <= lastItem && itemIndex < count; itemIndex++, childIndex++)
                {
                    UIElement child = (UIElement)generator.GenerateNext(out bool newlyRealized);

                    if (newlyRealized)
                    {
                        if (childIndex >= InternalChildren.Count)
                        {
                            AddInternalChild(child);
                        }
                        else
                        {
                            InsertInternalChild(childIndex, child);
                        }

                        generator.PrepareItemContainer(child);
                    }

                    child.Measure(new Size(cellInnerWidth, ItemHeight));
                }
            }

            CleanupContainers(firstItem, lastItem, generator);

            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int count = ItemCount;
            int perRow = ItemsPerRow(finalSize.Width);
            IItemContainerGenerator generator = ItemContainerGenerator;

            foreach (UIElement child in InternalChildren)
            {
                int itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(InternalChildren.IndexOf(child), 0));

                if (itemIndex < 0 || itemIndex >= count)
                {
                    continue;
                }

                int row = itemIndex / perRow;
                int col = itemIndex % perRow;

                int itemsInThisRow = Math.Min(perRow, count - (row * perRow));
                ArrangeChildInRow(child, col, itemsInThisRow, finalSize.Width, row);
            }

            return finalSize;
        }

        // Port of DistributeExtraSpace: each card gets ItemMinWidth + equal extra, capped at
        // ItemMaxWidth; if the row underflows the cap, center the remainder.
        private void ArrangeChildInRow(UIElement child, int col, int itemsInRow, double rowWidth, int row)
        {
            double baseWidth = ItemMinWidth + ItemMargin.Left + ItemMargin.Right;
            double totalBase = baseWidth * itemsInRow;
            double extraPerItem = itemsInRow > 0 ? Math.Max(0, (rowWidth - totalBase) / itemsInRow) : 0;

            double cellWidth = Math.Min(ItemMaxWidth + ItemMargin.Left + ItemMargin.Right, baseWidth + extraPerItem);
            double usedWidth = cellWidth * itemsInRow;
            double startX = usedWidth < rowWidth ? (rowWidth - usedWidth) / 2 : 0;

            double x = startX + (col * cellWidth);
            double y = (row * RowHeight) - _offset.Y;

            child.Arrange(new Rect(
                x + ItemMargin.Left,
                y + ItemMargin.Top,
                Math.Max(0, cellWidth - ItemMargin.Left - ItemMargin.Right),
                ItemHeight));
        }

        private void CleanupContainers(int firstItem, int lastItem, IItemContainerGenerator generator)
        {
            for (int i = InternalChildren.Count - 1; i >= 0; i--)
            {
                GeneratorPosition pos = new(i, 0);
                int itemIndex = generator.IndexFromGeneratorPosition(pos);

                if (itemIndex < firstItem || itemIndex > lastItem)
                {
                    generator.Remove(pos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        private void UpdateScrollInfo(Size viewport, Size extent)
        {
            bool changed = false;

            if (extent != _extent)
            {
                _extent = extent;
                changed = true;
            }

            if (viewport != _viewport)
            {
                _viewport = viewport;
                changed = true;
            }

            if (_offset.Y > Math.Max(0, _extent.Height - _viewport.Height))
            {
                _offset.Y = Math.Max(0, _extent.Height - _viewport.Height);
                changed = true;
            }

            if (changed)
            {
                ScrollOwner?.InvalidateScrollInfo();
            }
        }

        // ---- IScrollInfo ----
        public ScrollViewer ScrollOwner { get; set; }
        public bool CanHorizontallyScroll { get; set; }
        public bool CanVerticallyScroll { get; set; }

        public double ExtentWidth => _extent.Width;
        public double ExtentHeight => _extent.Height;
        public double ViewportWidth => _viewport.Width;
        public double ViewportHeight => _viewport.Height;
        public double HorizontalOffset => _offset.X;
        public double VerticalOffset => _offset.Y;

        public void LineUp() => SetVerticalOffset(VerticalOffset - (RowHeight / 3));
        public void LineDown() => SetVerticalOffset(VerticalOffset + (RowHeight / 3));
        public void WheelUp() => SetVerticalOffset(VerticalOffset - RowHeight);
        public void WheelDown() => SetVerticalOffset(VerticalOffset + RowHeight);
        public void MouseWheelUp() => SetVerticalOffset(VerticalOffset - RowHeight);
        public void MouseWheelDown() => SetVerticalOffset(VerticalOffset + RowHeight);
        public void PageUp() => SetVerticalOffset(VerticalOffset - ViewportHeight);
        public void PageDown() => SetVerticalOffset(VerticalOffset + ViewportHeight);

        public void LineLeft() { }
        public void LineRight() { }
        public void WheelLeft() { }
        public void WheelRight() { }
        public void MouseWheelLeft() { }
        public void MouseWheelRight() { }
        public void PageLeft() { }
        public void PageRight() { }
        public void SetHorizontalOffset(double offset) { }

        public void SetVerticalOffset(double offset)
        {
            offset = Math.Max(0, Math.Min(offset, Math.Max(0, _extent.Height - _viewport.Height)));

            if (Math.Abs(offset - _offset.Y) > 0.001)
            {
                _offset.Y = offset;
                ScrollOwner?.InvalidateScrollInfo();
                InvalidateMeasure();
            }
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            UIElement child = visual as UIElement;
            int idx = child != null ? InternalChildren.IndexOf(child) : -1;

            if (idx < 0)
            {
                return rectangle;
            }

            int itemIndex = ItemContainerGenerator.IndexFromGeneratorPosition(new GeneratorPosition(idx, 0));
            int perRow = ItemsPerRow(_viewport.Width);
            int row = perRow > 0 ? itemIndex / perRow : 0;
            double top = row * RowHeight;

            if (top < _offset.Y)
            {
                SetVerticalOffset(top);
            }
            else if (top + RowHeight > _offset.Y + _viewport.Height)
            {
                SetVerticalOffset(top + RowHeight - _viewport.Height);
            }

            return rectangle;
        }
    }
}
```

- [ ] **Step 2: Build verify.**
  Run: `dotnet build src/Portal/Sucrose.Portal/Sucrose.Portal.csproj -c Release -p:PlatformTarget=x64`
  Expected: `Build succeeded`. Fix any `IScrollInfo` member signature mismatches the compiler flags.

- [ ] **Step 3: Diagnostic harness (temporary).** Add a temporary `System.Diagnostics.Debug.WriteLine`
  in `MeasureOverride` printing `InternalChildren.Count`, `firstItem`, `lastItem`. This proves
  virtualization after Task 6 (realized count must stay bounded while scrolling, not grow to the full
  item count). Remove it in Task 12.

- [ ] **Step 4: Commit.**

```bash
git add src/Portal/Sucrose.Portal/Controls/JustifiedVirtualizingWrapPanel.cs
git commit -m "feat(portal): add justified virtualizing wrap panel (IScrollInfo, recycling)"
```

---

## Task 5: Convert `LibraryCard` to a full-MVVM `DataTemplate`

The card becomes binding-driven. Code-behind is reduced to recycle-safety plumbing (DataContext
changes drive load/teardown) and a thin ContextMenu.Opened forwarder; **no business logic** remains.

**Files:**
- Modify: `src/Portal/Sucrose.Portal/Views/Controls/LibraryCard.xaml`
- Modify: `src/Portal/Sucrose.Portal/Views/Controls/LibraryCard.xaml.cs`

- [ ] **Step 1: Rewrite `LibraryCard.xaml`.** Bind everything to `LibraryCardViewModel`. Replace the
  `VisualBrush`→`Rectangle` thumbnail with a direct `Image` clipped by the rounded `Border`, with
  `RenderOptions.BitmapScalingMode="LowQuality"`. Bind menu items to commands and the menu-state
  properties. Keep the hover GIF `Image` (`Imaginer`).

```xml
<UserControl
    x:Class="Sucrose.Portal.Views.Controls.LibraryCard"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:ui="http://schemas.lepo.co/wpfui/2022/xaml"
    xmlns:xag="https://github.com/XamlAnimatedGif/XamlAnimatedGif"
    xmlns:xf="clr-namespace:XamlFlair;assembly=XamlFlair.WPF"
    xmlns:vm="clr-namespace:Sucrose.Portal.ViewModels"
    d:DataContext="{d:DesignInstance vm:LibraryCardViewModel}"
    MinWidth="260" MinHeight="160" MaxWidth="400" MaxHeight="160"
    xf:Animations.Primary="{xf:Animate BasedOn={StaticResource Entered}, Event=MouseEnter}"
    xf:Animations.Secondary="{xf:Animate BasedOn={StaticResource Leaved}, Event=MouseLeave}"
    MouseEnter="LibraryCard_MouseEnter"
    MouseLeave="LibraryCard_MouseLeave"
    MouseLeftButtonUp="LibraryCard_MouseLeftButtonUp"
    DataContextChanged="LibraryCard_DataContextChanged"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
    mc:Ignorable="d">

    <UserControl.ContextMenu>
        <ContextMenu Opened="ContextMenu_Opened"
                     Background="{DynamicResource SolidBackgroundFillColorQuarternaryBrush}" Opacity="0.9">
            <ui:MenuItem Command="{Binding ReviewCommand}" Cursor="Hand" Header="{DynamicResource Portal.LibraryCard.MenuReview}" Icon="{ui:SymbolIcon Info24}" />
            <ui:MenuItem Command="{Binding UseEntryCommand}" Cursor="Hand" Header="{Binding UseHeader}" IsEnabled="{Binding CanUse}" Icon="{ui:SymbolIcon Checkmark24}" />
            <ui:MenuItem Command="{Binding CustomizeCommand}" Cursor="Hand" Header="{Binding CustomizeHeader}" IsEnabled="{Binding CanCustomize}" Icon="{ui:SymbolIcon Color24}" />
            <ui:MenuItem Command="{Binding CyclingAddCommand}" Cursor="Hand" Header="{DynamicResource Portal.LibraryCard.MenuCyclingAdd}" Visibility="{Binding CyclingAddVisible, Converter={StaticResource BoolToVisibility}}" Icon="{ui:SymbolIcon AddCircle24}" />
            <ui:MenuItem Command="{Binding CyclingRemoveCommand}" Cursor="Hand" Header="{DynamicResource Portal.LibraryCard.MenuCyclingRemove}" Visibility="{Binding CyclingRemoveVisible, Converter={StaticResource BoolToVisibility}}" Icon="{ui:SymbolIcon SubtractCircle24}" />
            <ui:MenuItem Command="{Binding FindCommand}" Cursor="Hand" Header="{DynamicResource Portal.LibraryCard.MenuFind}" Icon="{ui:SymbolIcon Folder24}" />
            <ui:MenuItem Command="{Binding ShareCommand}" Cursor="Hand" Header="{DynamicResource Portal.LibraryCard.MenuShare}" Icon="{ui:SymbolIcon Share24}" />
            <ui:MenuItem Command="{Binding DeleteEntryCommand}" Cursor="Hand" Header="{Binding DeleteHeader}" IsEnabled="{Binding CanDelete}" Icon="{ui:SymbolIcon Delete24}" />
            <ui:MenuItem Command="{Binding EditCommand}" Cursor="Hand" Header="{DynamicResource Portal.LibraryCard.MenuEdit}" Icon="{ui:SymbolIcon Pen24}" />
        </ContextMenu>
    </UserControl.ContextMenu>

    <Border ClipToBounds="True" CornerRadius="10">
        <Grid>
            <Border x:Name="Progress" ClipToBounds="True" CornerRadius="10"
                    Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibility}}">
                <Border.Background>
                    <SolidColorBrush Opacity="0.5" Color="{DynamicResource ApplicationBackgroundColor}" />
                </Border.Background>
                <ui:ProgressRing Width="120" Height="120" IsIndeterminate="True" />
            </Border>

            <Grid>
                <Image x:Name="Imagine"
                       RenderOptions.BitmapScalingMode="LowQuality"
                       Source="{Binding Thumbnail}" Stretch="UniformToFill" />
                <Image x:Name="Imaginer"
                       RenderOptions.BitmapScalingMode="LowQuality"
                       Stretch="UniformToFill" StretchDirection="Both"
                       Visibility="Hidden"
                       xag:AnimationBehavior.AutoStart="True"
                       xag:AnimationBehavior.CacheFramesInMemory="False"
                       xag:AnimationBehavior.RepeatBehavior="Forever"
                       xag:AnimationBehavior.SourceUri="{x:Null}" />

                <Border x:Name="Preview" VerticalAlignment="Bottom" ClipToBounds="True" CornerRadius="0,0,10,10">
                    <Border.Background>
                        <SolidColorBrush Opacity="0.75" Color="{DynamicResource SolidBackgroundFillColorQuarternary}" />
                    </Border.Background>
                    <Grid>
                        <Grid.RowDefinitions><RowDefinition Height="Auto" /><RowDefinition Height="Auto" /></Grid.RowDefinitions>
                        <Grid.ColumnDefinitions><ColumnDefinition Width="*" /><ColumnDefinition Width="Auto" /></Grid.ColumnDefinitions>
                        <ui:TextBlock Grid.Row="0" Margin="5,5,5,0" FontSize="16" FontWeight="Bold"
                                      Foreground="{DynamicResource TextFillColorPrimaryBrush}"
                                      Text="{Binding Title}" TextTrimming="CharacterEllipsis" TextWrapping="NoWrap" />
                        <ui:TextBlock Grid.Row="1" Margin="5,0,5,5" FontSize="12" FontWeight="SemiBold"
                                      Foreground="{DynamicResource TextPlaceholderColorBrush}"
                                      Text="{Binding Description}" TextTrimming="CharacterEllipsis" TextWrapping="NoWrap" />
                        <ui:Button x:Name="ThemeMore" Grid.Row="0" Grid.RowSpan="2" Grid.Column="1"
                                   HorizontalAlignment="Right" Appearance="Transparent" BorderBrush="Transparent"
                                   Click="ThemeMore_Click" Content="{ui:SymbolIcon MoreHorizontal24}" Cursor="Hand" FontSize="25"
                                   Visibility="{Binding IsIncompatible, Converter={StaticResource InverseBoolToVisibility}}" />
                        <ui:Button Grid.Row="0" Grid.RowSpan="2" Grid.Column="1"
                                   HorizontalAlignment="Right" Appearance="Transparent" BorderBrush="Transparent"
                                   Command="{Binding UpdateCommand}" Content="{ui:SymbolIcon BoxDismiss24}" Cursor="Hand" FontSize="17"
                                   Foreground="{DynamicResource PaletteRedBrush}"
                                   Visibility="{Binding IsIncompatible, Converter={StaticResource BoolToVisibility}}" />
                    </Grid>
                </Border>
            </Grid>
        </Grid>
    </Border>
</UserControl>
```

> The `BoolToVisibility` / `InverseBoolToVisibility` converters must exist in Portal resources. If
> not present, add them (or reuse wpfui/existing converters) and register in `App.xaml`. Verify the
> exact converter keys already used elsewhere in the Portal before inventing new ones.

- [ ] **Step 2: Rewrite `LibraryCard.xaml.cs`.** Strip all ported logic (now in the VM). Keep only:
  parameterless ctor; `DataContextChanged` → cancel previous load, start new load; mouse
  enter/leave hover GIF (reading the VM's `PreviewPath`); click-to-use forwarding to the VM command;
  ContextMenu.Opened → `vm.RefreshMenuState()`; teardown on `Unloaded`.

```csharp
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SMMP = Sucrose.Manager.Manage.Portal;
using SPVMLC = Sucrose.Portal.ViewModels.LibraryCardViewModel;
using SXAGAB = Sucrose.XamlAnimatedGif.AnimationBehavior;

namespace Sucrose.Portal.Views.Controls
{
    public partial class LibraryCard : UserControl
    {
        private CancellationTokenSource _cts;

        public LibraryCard()
        {
            InitializeComponent();
            Unloaded += LibraryCard_Unloaded;
        }

        private SPVMLC ViewModel => DataContext as SPVMLC;

        private async void LibraryCard_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _cts?.Cancel();
            _cts?.Dispose();

            ClearPreview();
            Imagine.Source = null;

            if (ViewModel == null)
            {
                return;
            }

            _cts = new CancellationTokenSource();

            try
            {
                await ViewModel.LoadThumbnailAsync(_cts.Token);
            }
            catch (OperationCanceledException)
            {
                // expected on recycle
            }
        }

        private void LibraryCard_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ViewModel == null)
            {
                return;
            }

            Cursor = ViewModel.IsIncompatible ? Cursors.Arrow : Cursors.Hand;

            if (SMMP.LibraryPreview && File.Exists(ViewModel.PreviewPath))
            {
                SXAGAB.SetSourceUri(Imaginer, new Uri(ViewModel.PreviewPath));
                Imaginer.Visibility = Visibility.Visible;
                Imagine.Visibility = Visibility.Hidden;
            }
        }

        private void LibraryCard_MouseLeave(object sender, MouseEventArgs e)
        {
            if (SMMP.LibraryPreview)
            {
                ClearPreview();
            }
        }

        private void ClearPreview()
        {
            Imaginer.Source = null;
            SXAGAB.SetSourceUri(Imaginer, null);
            Imaginer.Visibility = Visibility.Hidden;
            Imagine.Visibility = Visibility.Visible;
        }

        private void LibraryCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel?.UseEntryCommand.CanExecute(null) == true)
            {
                ViewModel.UseEntryCommand.Execute(null);
            }
        }

        private void ThemeMore_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu.IsOpen = true;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ViewModel?.RefreshMenuState();
        }

        private void LibraryCard_Unloaded(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            ClearPreview();
        }
    }
}
```

- [ ] **Step 3: Build verify.**
  Run: `dotnet build src/Portal/Sucrose.Portal/Sucrose.Portal.csproj -c Release -p:PlatformTarget=x64`
  Expected: `Build succeeded`. (LibraryCard is not yet hosted — wired in Task 6.)

- [ ] **Step 4: Commit.**

```bash
git add src/Portal/Sucrose.Portal/Views/Controls/LibraryCard.xaml src/Portal/Sucrose.Portal/Views/Controls/LibraryCard.xaml.cs
git commit -m "refactor(portal): make LibraryCard a binding-driven recycle-safe template"
```

---

## Task 6: Convert `FullLibraryPage` to a virtualized ItemsControl

Replace the imperative panel + pagination with a virtualizing `ItemsControl` bound to an
`ObservableCollection<LibraryCardViewModel>`; load metadata incrementally; filter via `ICollectionView`.

**Files:**
- Modify: `src/Portal/Sucrose.Portal/Views/Pages/Library/FullLibraryPage.xaml`
- Modify: `src/Portal/Sucrose.Portal/Views/Pages/Library/FullLibraryPage.xaml.cs`

- [ ] **Step 1: Rewrite `FullLibraryPage.xaml`.** Remove `SizeConstrainingContainer`,
  `DynamicScrollViewer`, the wrapping `StackPanel`, `LibraryStackPanel`, and `Pagination`. Use an
  `ItemsControl` with the custom panel and a scoped scroll template that keeps `CanContentScroll=True`
  (defeating wpfui's `Scroll.xaml` per issue #164).

```xml
<Page
    x:Class="Sucrose.Portal.Views.Pages.Library.FullLibraryPage"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:controls="clr-namespace:Sucrose.Portal.Controls"
    xmlns:local="clr-namespace:Sucrose.Portal.Views.Pages.Library"
    xmlns:vm="clr-namespace:Sucrose.Portal.ViewModels"
    AllowDrop="True" Loaded="FullLibraryPage_Loaded"
    xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
    xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" mc:Ignorable="d">

    <Grid>
        <Frame x:Name="Empty" Visibility="Collapsed">
            <Frame.Content><local:SearchLibraryPage /></Frame.Content>
        </Frame>

        <ItemsControl x:Name="ThemeLibrary"
                      Margin="10,0,10,0"
                      VirtualizingPanel.IsVirtualizing="True"
                      VirtualizingPanel.VirtualizationMode="Recycling"
                      VirtualizingPanel.ScrollUnit="Pixel"
                      ScrollViewer.CanContentScroll="True"
                      ScrollViewer.VerticalScrollBarVisibility="Auto"
                      ScrollViewer.HorizontalScrollBarVisibility="Disabled">
            <ItemsControl.Template>
                <ControlTemplate TargetType="ItemsControl">
                    <ScrollViewer CanContentScroll="True" Focusable="False"
                                  VerticalScrollBarVisibility="{TemplateBinding ScrollViewer.VerticalScrollBarVisibility}"
                                  HorizontalScrollBarVisibility="Disabled">
                        <ItemsPresenter />
                    </ScrollViewer>
                </ControlTemplate>
            </ItemsControl.Template>
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <controls:JustifiedVirtualizingWrapPanel
                        x:Name="Panel" ItemMinWidth="260" ItemMaxWidth="400" ItemHeight="160" />
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            <ItemsControl.ItemTemplate>
                <DataTemplate DataType="{x:Type vm:LibraryCardViewModel}">
                    <controls:LibraryCardHostBorder>
                        <local:OrControlPlaceholder />
                    </controls:LibraryCardHostBorder>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </Grid>
</Page>
```

> Simplify the `ItemTemplate` to just the card:
> ```xml
> <ItemsControl.ItemTemplate>
>     <DataTemplate DataType="{x:Type vm:LibraryCardViewModel}">
>         <vcontrols:LibraryCard xmlns:vcontrols="clr-namespace:Sucrose.Portal.Views.Controls" />
>     </DataTemplate>
> </ItemsControl.ItemTemplate>
> ```
> Use this simpler form (declare `vcontrols` in the Page root instead of inline). The
> `LibraryCardHostBorder`/`OrControlPlaceholder` names above are illustrative only — do not create them.

  Apply `ItemMargin` and `MaxItemsPerRow` from settings in code-behind (Step 2), matching the old
  `ThemeLibrary.ItemMargin`/`MaxItemsPerRow` assignment.

- [ ] **Step 2: Rewrite `FullLibraryPage.xaml.cs`.** Implement `ILibraryCardHost`; build the VM
  collection with incremental background metadata loading; wire search via `ICollectionView.Filter`;
  drop pagination and the `IsVisibleChanged` machinery.

```csharp
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using SMML = Sucrose.Manager.Manage.Library;
using SMMP = Sucrose.Manager.Manage.Portal;
using SMMRC = Sucrose.Memory.Manage.Readonly.Content;
using SPMI = Sucrose.Portal.Manage.Internal;
using SPVMLC = Sucrose.Portal.ViewModels.LibraryCardViewModel;
using SPVMIH = Sucrose.Portal.ViewModels.ILibraryCardHost;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;

namespace Sucrose.Portal.Views.Pages.Library
{
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

        private bool FilterCard(object Item)
        {
            if (_search.Length == 0)
            {
                return true;
            }

            if (Item is SPVMLC Card)
            {
                string Haystack = $"{Card.Title} {Card.Description}";
                return _search.All(Word => Haystack.Split(' ').Any(Part => Part.Contains(Word)));
            }

            return false;
        }

        private async void FullLibraryPage_Loaded(object sender, RoutedEventArgs e)
        {
            ((JustifiedVirtualizingWrapPanelHolder)null)?.ToString(); // no-op; panel config below

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

            Empty.Visibility = Cards.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public void Remove(SPVMLC ViewModel)
        {
            Cards.Remove(ViewModel);
            Themes.Remove(Path.GetFileName(ViewModel.Theme));
            Searches.Remove(Path.GetFileName(ViewModel.Theme), out _);

            if (Cards.Count <= 0)
            {
                Empty.Visibility = Visibility.Visible;
            }
        }

        public void Refresh()
        {
            _search = SPMI.SearchService.SearchList;
            View.Refresh();
        }

        public void Dispose()
        {
            Cards.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
```

> Remove the placeholder no-op line and the `JustifiedVirtualizingWrapPanelHolder` reference — they
> are illustrative. Apply `ItemMargin`/`MaxItemsPerRow` to the panel: either set them as XAML
> bindings to settings, or find the panel via the visual tree after load and set
> `Panel.ItemMargin = new Thickness(SMMP.AdaptiveMargin); Panel.MaxItemsPerRow = SMMP.AdaptiveLayout;`.
> Prefer naming the panel in XAML (`x:Name="Panel"`) and accessing it once realized. Confirm how the
> existing code reads `SMMP.AdaptiveMargin`/`AdaptiveLayout` and keep parity.

- [ ] **Step 3: Build verify.**
  Run: `dotnet build src/Portal/Sucrose.Portal/Sucrose.Portal.csproj -c Release -p:PlatformTarget=x64`
  Expected: `Build succeeded`.

- [ ] **Step 4: Commit.**

```bash
git add src/Portal/Sucrose.Portal/Views/Pages/Library/FullLibraryPage.xaml src/Portal/Sucrose.Portal/Views/Pages/Library/FullLibraryPage.xaml.cs
git commit -m "feat(portal): virtualize FullLibraryPage and remove pagination"
```

---

## Task 7: Library UAT checkpoint

**Files:** none (verification only).

- [ ] **Step 1: Build the solution & run Portal.**
  `dotnet build src/Sucrose.slnx -c Release -p:PlatformTarget=x64`, then launch the Portal exe and
  open the Library.

- [ ] **Step 2: Verify virtualization.** With the temporary diagnostic from Task 4 Step 3, scroll a
  large library top-to-bottom. Expected: `InternalChildren.Count` stays bounded (≈ visible rows ×
  perRow + cache), does NOT grow to the full item count. Scrolling is smooth.

- [ ] **Step 3: Verify layout fidelity.** Resize the window narrow→wide. Rows stay justified; in
  wide windows with few items per row, cards cap at 400px and center (matches old behavior).

- [ ] **Step 4: Verify functionality.** Hover → preview GIF plays on that card only and clears on
  leave; click → applies wallpaper; right-click → context menu enablement correct across wallpaper
  types and performance/cycling states; delete → card disappears and layout reflows; search box
  filters live without rebuild; empty state shows when no matches.

- [ ] **Step 5: If any check fails**, return to the relevant task (4/5/6) and fix; re-verify. Do not
  proceed to Store until Library passes.

- [ ] **Step 6: Commit** (only if fixes were made; otherwise skip).

---

## Task 8: `StoreCardViewModel`

Mirror Task 3 for the Store. **Read `src/Portal/Sucrose.Portal/Views/Controls/StoreCard.xaml.cs`
fully first** and port its logic (remote cover download, hover GIF from URL, install/details
actions, info-changed subscription).

**Files:**
- Create: `src/Portal/Sucrose.Portal/ViewModels/IStoreCardHost.cs`
- Create: `src/Portal/Sucrose.Portal/ViewModels/StoreCardViewModel.cs`

- [ ] **Step 1:** Write `IStoreCardHost` (`void Remove(StoreCardViewModel ViewModel);`) and
  `StoreCardViewModel : CardViewModelBase`, porting `StoreCard.xaml.cs` methods into `[RelayCommand]`s
  and a `RefreshMenuState()`. `ThumbnailPath` returns the disk-cached cover path
  (`Path.Combine(Theme, Info.Thumbnail)` where `Theme` is the cache dir). `PreviewPath` is the remote
  GIF URL string built as in `StoreCard.xaml.cs`. Expose a cancellable
  `EnsureDownloadedAsync(CancellationToken)` for the card to call on bind and cancel on recycle.
  Keep the same alias `using`s as the original file.

- [ ] **Step 2: Build verify.**
  Run: `dotnet build src/Portal/Sucrose.Portal/Sucrose.Portal.csproj -c Release -p:PlatformTarget=x64`
  Expected: `Build succeeded`.

- [ ] **Step 3: Commit.**

```bash
git add src/Portal/Sucrose.Portal/ViewModels/IStoreCardHost.cs src/Portal/Sucrose.Portal/ViewModels/StoreCardViewModel.cs
git commit -m "feat(portal): add store card view model with ported store logic"
```

---

## Task 9: Convert `StoreCard` to a full-MVVM `DataTemplate`

Mirror Task 5. Additionally cancel any in-flight remote cover download / hover-GIF fetch when the
container recycles (on `DataContextChanged` and `Unloaded`).

**Files:**
- Modify: `src/Portal/Sucrose.Portal/Views/Controls/StoreCard.xaml`
- Modify: `src/Portal/Sucrose.Portal/Views/Controls/StoreCard.xaml.cs`

- [ ] **Step 1:** Rewrite `StoreCard.xaml` binding to `StoreCardViewModel`: direct `Image` +
  `BitmapScalingMode=LowQuality` instead of `VisualBrush`; bind menu items to commands/menu-state;
  keep hover GIF `Imaginer`.
- [ ] **Step 2:** Rewrite `StoreCard.xaml.cs` to the recycle-safe pattern from Task 5, plus: on
  `DataContextChanged`, start `ViewModel.EnsureDownloadedAsync(_cts.Token)` then
  `LoadThumbnailAsync`; on recycle/`Unloaded`, cancel. Keep the info-changed subscription scoped to
  Loaded/Unloaded.
- [ ] **Step 3: Build verify.**
  Run: `dotnet build src/Portal/Sucrose.Portal/Sucrose.Portal.csproj -c Release -p:PlatformTarget=x64`
  Expected: `Build succeeded`.
- [ ] **Step 4: Commit.**

```bash
git add src/Portal/Sucrose.Portal/Views/Controls/StoreCard.xaml src/Portal/Sucrose.Portal/Views/Controls/StoreCard.xaml.cs
git commit -m "refactor(portal): make StoreCard a binding-driven recycle-safe template"
```

---

## Task 10: Convert `FullStorePage` to a virtualized ItemsControl

Mirror Task 6 for `src/Portal/Sucrose.Portal/Views/Pages/Store/FullStorePage.xaml(.cs)`. Implement
`IStoreCardHost`; load the catalog into `ObservableCollection<StoreCardViewModel>` incrementally;
filter via `ICollectionView`; remove `StoreStackPanel`, `DynamicScrollViewer`,
`SizeConstrainingContainer`, and `Pagination`. Use the same `ItemsControl` + scoped scroll template +
`JustifiedVirtualizingWrapPanel` as Task 6.

- [ ] **Step 1:** Rewrite `FullStorePage.xaml` (copy Task 6's ItemsControl structure; item template
  is `StoreCard`).
- [ ] **Step 2:** Rewrite `FullStorePage.xaml.cs` (copy Task 6's pattern; `Remove` removes from the
  collection; keep store-specific catalog/category/search wiring; ensure category/search changes call
  `View.Refresh()` instead of rebuilding pages).
- [ ] **Step 3: Build verify.**
  Run: `dotnet build src/Portal/Sucrose.Portal/Sucrose.Portal.csproj -c Release -p:PlatformTarget=x64`
  Expected: `Build succeeded`.
- [ ] **Step 4: Commit.**

```bash
git add src/Portal/Sucrose.Portal/Views/Pages/Store/FullStorePage.xaml src/Portal/Sucrose.Portal/Views/Pages/Store/FullStorePage.xaml.cs
git commit -m "feat(portal): virtualize FullStorePage and remove pagination"
```

---

## Task 11: Store UAT checkpoint

**Files:** none (verification only).

- [ ] **Step 1:** Build the solution & run Portal; open the Store.
- [ ] **Step 2:** Verify virtualization (bounded realized count), smooth scroll, justified layout
  at multiple widths.
- [ ] **Step 3:** Verify covers load from disk cache; scrolling fast does not leave wrong images on
  recycled cards; downloads cancel when scrolled away (no UI stalls).
- [ ] **Step 4:** Verify category switch + search filter live; install/details actions work; hover
  GIF plays only on hovered card.
- [ ] **Step 5:** Fix and re-verify if needed.

---

## Task 12: Cleanup & final verification

**Files:**
- Delete: `src/Portal/Sucrose.Portal/Controls/LibraryStackPanel.cs`
- Delete: `src/Portal/Sucrose.Portal/Controls/StoreStackPanel.cs`
- Delete or fix: `src/Portal/Sucrose.Portal/Controls/SizeConstrainingContainer.cs`
- Modify: `src/Portal/Sucrose.Portal/Controls/JustifiedVirtualizingWrapPanel.cs` (remove Task 4
  diagnostic)

- [ ] **Step 1:** Confirm `LibraryStackPanel`/`StoreStackPanel` have no remaining references
  (`Grep pattern "LibraryStackPanel|StoreStackPanel" path "src/Portal"` → only the files themselves).
  Delete both files.
- [ ] **Step 2:** Check `SizeConstrainingContainer` references
  (`Grep pattern "SizeConstrainingContainer" path "src/Portal"`). If none remain, delete it; if used
  elsewhere, fix the `MeasureOverride` `height - height` → return the real measured height. Decide and
  do one.
- [ ] **Step 3:** Remove the temporary `Debug.WriteLine` diagnostic from the panel.
- [ ] **Step 4:** Final `GC.Collect` sweep: `Grep pattern "GC\.Collect" path "src/Portal"` → expect
  zero.
- [ ] **Step 5: Build the full solution.**
  `dotnet build src/Sucrose.slnx -c Release -p:PlatformTarget=x64` → `Build succeeded`.
- [ ] **Step 6: Full UAT** — repeat Task 7 + Task 11 checklists end-to-end; confirm scroll is smooth
  in both Library and Store with no regressions.
- [ ] **Step 7: Commit.**

```bash
git add -u src/Portal/Sucrose.Portal/Controls
git add src/Portal/Sucrose.Portal/Controls/JustifiedVirtualizingWrapPanel.cs
git commit -m "chore(portal): remove legacy panels and finalize virtualization"
```

---

## Self-review notes (author)

- **Spec coverage:** custom justified panel (T4), full-MVVM cards (T3/T5/T8/T9), remove pagination +
  incremental load + filter (T6/T10), shared image cache (T2), shared panel replacing both
  duplicates (T4 + T12 deletions), GC.Collect removal + Workstation GC (T1), render fixes /
  VisualBrush removal / BitmapScalingMode (T5/T9), #164 scroll style (T6/T10),
  SizeConstrainingContainer retire/fix (T12), Store remote-download recycle cancellation (T9). All
  spec sections map to tasks.
- **No-test reality:** TDD steps replaced by build-verify + UAT + a temporary panel diagnostic, per
  project constraints (overrides the skill's default TDD).
- **Type consistency:** command names follow CommunityToolkit generation (`UseEntry` →
  `UseEntryCommand`, `DeleteEntry` → `DeleteEntryCommand`); the XAML in T5 binds those exact names.
- **Known iteration points (flagged, not placeholders):** the `JustifiedVirtualizingWrapPanel`
  realization loop and `IScrollInfo` need compile + live-scroll iteration (T4 Step 2-3, T7);
  converter keys (`BoolToVisibility`/`InverseBoolToVisibility`) must be confirmed against existing
  Portal resources before use; event→command bridge kept as thin code-behind forwarders (no business
  logic) to avoid a new dependency.
