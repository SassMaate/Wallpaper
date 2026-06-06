# Portal Library/Store Virtualization — Design Spec

- **Date:** 2026-06-06
- **Status:** Approved design (pending written-spec review)
- **Area:** `src/Portal/Sucrose.Portal` (Library + Store pages, card controls, custom panels)
- **Author:** Taiizor (with Claude)

## 1. Problem

The Portal's Library and Store views stutter badly while scrolling. A root-cause investigation
(systematic debugging, evidence-backed) found the cause is **not** the images themselves — the
decode pipeline is already correct (`DecodePixelWidth=360`, `BitmapCacheOption.OnLoad`, `Freeze()`,
off-thread `Task.Run`; GIFs are hover-only and opt-in). The real causes are:

1. **No UI virtualization.** `LibraryStackPanel` / `StoreStackPanel` are plain `StackPanel`
   subclasses (not `VirtualizingPanel`), hosted in a pixel-scrolling `ui:DynamicScrollViewer` inside
   an outer `StackPanel` (infinite height). Every card on the current page is a fully-realized,
   composited `UserControl` every frame.
2. **Forced blocking `GC.Collect()` on the UI thread**, called on `Dispose`, `MouseEnter`,
   `MouseLeave`, `MediaOpened`, pagination, and per-card add (10+ sites). Each is a full blocking
   collection during interaction.
3. **Server GC** configured for the UI process (`Directory.Build.targets`), which is throughput-
   tuned and produces pause spikes on a latency-sensitive desktop UI.
4. **Heavy per-card visual tree:** thumbnail painted through a `VisualBrush` → rounded `Rectangle`
   (more expensive than a direct `Image`), default HighQuality `BitmapScalingMode`, multiple
   `ClipToBounds`+`CornerRadius` borders, an always-running indeterminate `ProgressRing`.

This is a fix-in-place problem, not a rewrite. This spec covers the **structural** fix: true UI
virtualization with the existing justified layout preserved.

## 2. Goals / Non-Goals

**Goals**
- Real UI virtualization (container recycling) for both Library and Store card lists.
- Preserve the **exact** current "justified" row layout (variable card width 260–400, leftover row
  width distributed/centered, including the 400px `MaxWidth` cap behavior).
- Remove pagination; present the full library/store as one smooth virtualized scroll with
  incremental background metadata loading.
- Convert cards to a **full-MVVM**, data-bound, recycle-safe model.
- De-duplicate: replace the two copy-pasted panels with one shared panel; share a card VM base.
- Fold in the directly-related performance fixes in files we touch (remove `GC.Collect()`, switch
  Portal to Workstation GC, drop the `VisualBrush` indirection, set `BitmapScalingMode=LowQuality`,
  fix/retire the buggy `SizeConstrainingContainer`).

**Non-Goals**
- No change to wallpaper rendering engines, services, IPC, or anything outside Portal.
- No new wallpaper features; no visual redesign of the card beyond the render-cost fixes.
- No automated test suite is introduced (none exists in the repo); verification is manual UAT.

## 3. Locked Decisions (from brainstorming)

| # | Decision | Choice |
|---|----------|--------|
| 1 | Approach | Structural — real UI virtualization |
| 2 | Layout fidelity | Preserve current **justified** layout exactly |
| 3 | Panel | **Custom** justified virtualizing panel (no off-the-shelf panel) |
| 4 | Pagination | **Remove**; virtualize the whole list, load metadata async/incrementally |
| 5 | Card refactor depth | **Full MVVM** (commands on VM, card becomes a binding-driven template) |
| 6 | GC mode | **Include** — switch Portal (UI processes) to Workstation GC |

**Key enabling fact:** cards are fixed height (`MinHeight=MaxHeight=160`) with variable width
(`MinWidth=260`, `MaxWidth=400`). Uniform row height makes total scroll extent exactly computable
without realizing any item, which makes a correct custom virtualizing panel tractable.

## 4. Architecture

### 4.1 Shared components (new)

**`JustifiedVirtualizingWrapPanel : VirtualizingPanel, IScrollInfo`**
(`src/Portal/Sucrose.Portal/Controls/`)

Single panel used by both Library and Store. Replaces and deletes `LibraryStackPanel.cs` and
`StoreStackPanel.cs`.

Layout (deterministic because row height is uniform):

```
itemsPerRow = clamp(floor((viewportW + margin) / (itemMinW + margin)), 1, MaxItemsPerRow)
rowHeight   = itemHeight + verticalMargin
totalRows   = ceil(itemCount / itemsPerRow)
extentH     = totalRows * rowHeight                         // exact, no realization
firstRow    = floor(verticalOffset / rowHeight)
lastRow     = ceil((verticalOffset + viewportH) / rowHeight) + cacheRows
realize items in [firstRow*itemsPerRow, (lastRow+1)*itemsPerRow) via ItemContainerGenerator,
  recycle all others
for each realized row: arrange with the existing DistributeExtraSpace justification
  (stretch each card from itemMinW up to itemMaxW; if a row underflows, center the remainder),
  positioned at y = row*rowHeight - verticalOffset
```

Dependency properties: `ItemMargin` (Thickness), `MaxItemsPerRow` (int, fed from
`SMMP.AdaptiveLayout`), `ItemMinWidth` (260), `ItemMaxWidth` (400), `ItemHeight` (160). Item metrics
default to the card's known constants but may be measured from the first realized container to stay
robust to future card resizing.

`IScrollInfo` responsibilities: vertical offset/extent/viewport, `LineUp/Down`, `MouseWheelUp/Down`,
`PageUp/Down`, `SetVerticalOffset`, `MakeVisible` (for keyboard focus / bring-into-view),
`ScrollOwner`. Smooth pixel scrolling is preserved via `ScrollUnit=Pixel` + `CanContentScroll=True`.
Re-measure on `MaxItemsPerRow` / `ItemMargin` change and on viewport width change (handles
`AdaptiveLayout` / window resize / DPI).

**`CardViewModelBase`** + **`LibraryCardViewModel`** / **`StoreCardViewModel`**
(`src/Portal/Sucrose.Portal/ViewModels/` — follow existing Portal VM/namespace conventions)

Lightweight, observable data carriers (CommunityToolkit.Mvvm `ObservableObject`). They hold the
theme path, `Info`, computed Title/Description, thumbnail path, preview/GIF path, compatibility flag,
wallpaper `Type`, and **all card commands** (see §4.3). No heavy `UserControl` is created until a
container is realized — the collection holds only VMs.

**Shared bounded image cache** (extend `Sucrose.Portal.Extension.ImageLoader`)

A process-wide, size-bounded (LRU) `path → frozen BitmapImage` cache so recycled/re-scrolled
containers do not re-decode. The current per-card `new ImageLoader()` is replaced by the shared
cache. Remove the `GC.Collect()` from `ImageLoader.Dispose`.

### 4.2 Page conversion

For each page (`FullLibraryPage`, `FullStorePage`):

- Replace `SizeConstrainingContainer → DynamicScrollViewer → StackPanel → LibraryStackPanel` with a
  single virtualizing **`ItemsControl`** (or `ListBox`):
  - `ItemsSource` = `ObservableCollection<CardViewModel>`
  - `ItemsPanel` = `JustifiedVirtualizingWrapPanel`
  - `VirtualizingPanel.IsVirtualizing=True`, `VirtualizationMode=Recycling`, `ScrollUnit=Pixel`
  - `ScrollViewer.CanContentScroll=True` (the panel owns scrolling)
  - `ItemTemplate` = the card `DataTemplate`
- **Remove** the outer `DynamicScrollViewer` and the redundant `SizeConstrainingContainer` from
  these pages (the latter has a `height - height = 0` measure bug; with a scroll-owning ItemsControl
  it is unnecessary).
- **Metadata loading:** enumerate themes/catalog and read each `SucroseInfo` JSON on a background
  thread, then add VMs to the `ObservableCollection` in UI-thread batches, behind a loading
  indicator. No per-item `Task.Delay(50)` staggering.
- **Search/filter:** use `ICollectionView.Filter` over the full VM collection instead of rebuilding
  pages.
- **Remove** the `Pagination` control usage from these pages and the page-rebuild lifecycle
  (`AddThemes`/`SelectPageChanged`/`MaxPage`). The `Pagination` control itself stays in the repo if
  used elsewhere; only these two pages stop using it.

### 4.3 Full-MVVM card

- The card becomes a binding-driven `DataTemplate` root. All business logic moves to the VM:
  - Commands: `UseCommand`, `FindCommand`, `CustomizeCommand`, `CyclingAddCommand`,
    `CyclingRemoveCommand`, `EditCommand`, `ShareCommand`, `ReviewCommand`, `DeleteCommand`,
    `MoreCommand` (Store has its own set: install/download/details/etc.).
  - The complex `ContextMenu_Opened` enablement logic (engine-property `File.Exists` checks,
    cycling state, performance/close/pause state, version compatibility) moves to VM observable
    properties (`CanUse`, `CanDelete`, `CanCustomize`, `IsCyclingExcluded`, `IsIncompatible`, menu
    header suffixes, …), recomputed via a `RefreshMenuState()` invoked on context-menu open.
  - Thumbnail `ImageSource`, hover-preview state, and the preview GIF source/teardown are VM-driven.
- **Recycle safety:** the card reacts to `DataContextChanged` (fires when a recycled container is
  bound to a new VM), not `Loaded`. On bind: cancel the previous image load (`CancellationToken`),
  reset visuals (progress state, clear stale image/GIF), kick off the new (cancellable) load from the
  shared cache. On recycle/unbind: tear down the hover GIF (`AnimationBehavior.SetSourceUri(...,
  null)`) and cancel any in-flight Store download.
- **Input events** (MouseEnter/Leave for hover preview, click-to-use): full-MVVM keeps logic in the
  VM; the event→command bridge uses either `Microsoft.Xaml.Behaviors.Wpf` interaction triggers or a
  small attached behavior. **Sub-decision deferred to the plan:** prefer a tiny attached behavior to
  avoid a new dependency unless `Microsoft.Xaml.Behaviors.Wpf` is judged worthwhile. No business
  logic lives in code-behind either way.
- Delete removes the VM from the `ObservableCollection` (the old `IsVisibleChanged` +
  panel-removes-invisible-children trick is removed).
- **Render fixes (folded in):** replace the `VisualBrush`→`Rectangle` thumbnail with a direct
  `Image` clipped by a `Border CornerRadius`; set `RenderOptions.BitmapScalingMode=LowQuality` on the
  thumbnail; keep the progress ring but only while a card is actually loading.

### 4.4 wpfui ScrollViewer style (issue #164)

wpfui's global `Scroll.xaml` retemplates ScrollViewers in a way that defeats virtualization. Scope a
local ScrollViewer style/template for the virtualized ItemsControl that preserves
`CanContentScroll=True` (the panel's `IScrollInfo` drives scrolling). Validate during implementation
that virtualization is actually active (e.g., realized-container count stays bounded while scrolling).

### 4.5 GC mode

In `Directory.Build.targets`, switch the UI processes (Portal at minimum) from Server GC to
**Workstation GC**, and review `ConcurrentGarbageCollection`. Because the current setting is global
to all non-Library outputs, scope the change so it targets UI processes and does not regress
throughput-oriented background services unintentionally. Combined with removing the forced
`GC.Collect()` calls, this addresses the GC-pause class of stutter.

## 5. File change inventory

**New**
- `src/Portal/Sucrose.Portal/Controls/JustifiedVirtualizingWrapPanel.cs`
- `src/Portal/Sucrose.Portal/ViewModels/CardViewModelBase.cs`
- `src/Portal/Sucrose.Portal/ViewModels/LibraryCardViewModel.cs`
- `src/Portal/Sucrose.Portal/ViewModels/StoreCardViewModel.cs`
- (optional) attached behavior for event→command, if not using `Microsoft.Xaml.Behaviors.Wpf`

**Modified**
- `src/Portal/Sucrose.Portal/Views/Pages/Library/FullLibraryPage.xaml(.cs)`
- `src/Portal/Sucrose.Portal/Views/Pages/Store/FullStorePage.xaml(.cs)`
- `src/Portal/Sucrose.Portal/Views/Controls/LibraryCard.xaml(.cs)`
- `src/Portal/Sucrose.Portal/Views/Controls/StoreCard.xaml(.cs)`
- `src/Portal/Sucrose.Portal/Extension/ImageLoader.cs` (shared bounded cache; drop `GC.Collect`)
- `Directory.Build.targets` (Workstation GC for UI processes)
- Remove all `GC.Collect()` in touched files
  (`LibraryCard`, `StoreCard`, `FullLibraryPage`, `FullStorePage`, `ImageLoader`, the deleted panels)

**Deleted**
- `src/Portal/Sucrose.Portal/Controls/LibraryStackPanel.cs`
- `src/Portal/Sucrose.Portal/Controls/StoreStackPanel.cs`
- `src/Portal/Sucrose.Portal/Controls/SizeConstrainingContainer.cs` (retire if confirmed redundant;
  otherwise fix the `height - height` bug and keep)

## 6. Scope & sequencing

1. Build shared infrastructure: `JustifiedVirtualizingWrapPanel`, `CardViewModelBase`, shared image
   cache.
2. Apply to **Library** first; verify end-to-end (scroll, justified look, search, delete, hover
   GIF, resize/AdaptiveLayout). Library is the proving ground.
3. Apply to **Store** (adds remote cover download + hover GIF from URL; ensure downloads cancel on
   recycle).
4. GC-mode change + final cleanup pass (remove remaining `GC.Collect`).

Plan phase decides whether this ships as one PR or staged (Library, then Store).

## 7. Risks

- **Custom `IScrollInfo` correctness** is the highest risk: vertical offset/extent math, mouse-wheel
  and keyboard navigation, `MakeVisible`/bring-into-view, focus, and resize re-layout must all be
  correct. Mitigated by uniform row height (simple, exact math) and incremental verification.
- **Recycling staleness:** a recycled container must fully reset to the newly bound VM (image,
  GIF, menu state, in-flight async). Mitigated by `DataContextChanged`-driven reset + per-container
  `CancellationToken`.
- **Full-MVVM rewrite of complex menu logic** (the engine-property `File.Exists` matrix) is a real
  regression surface with no automated tests. Mitigated by a thorough manual UAT checklist (§8) and
  staging Library before Store.
- **Store remote download × recycling:** cover downloads / hover-GIF fetches must be cancelled when
  a container recycles to avoid wasted work and wrong-image races.
- **wpfui Scroll.xaml override (#164)** may need iteration to keep both virtualization and the
  app's scroll look/behavior.
- **AdaptiveLayout / DPI / multi-monitor** changes must trigger correct panel re-layout.

## 8. Verification (manual UAT — no test suite)

- Scroll a large library/store top-to-bottom: smooth, no stutter; realized-container count stays
  bounded (verify via a temporary diagnostic / count log).
- Justified layout matches the old look at multiple window widths, including the sparse-wide-row
  case (cards capped at 400px and centered).
- Search filters live without rebuild; result set scrolls smoothly.
- Delete removes a card and the layout reflows correctly; selection/empty states behave.
- Hover preview GIF plays on the hovered card only and tears down on leave/recycle; no leaks.
- Store: covers load from disk cache; downloads cancel when scrolled away; details/install actions
  work via commands.
- Context menu enablement matches prior behavior across wallpaper types and engine configs
  (Web/Gif/Video/YouTube; Mpv/CefSharp/WebView), cycling on/off, performance close/pause states,
  and incompatible versions.
- Resize window / change AdaptiveLayout / move across monitors with different DPI: re-layouts
  correctly.
- (Optional) Temporary frame-time / scroll-latency logging to quantify before/after.

## 9. Open sub-decisions for the plan phase

- Event→command bridge: tiny attached behavior vs `Microsoft.Xaml.Behaviors.Wpf` dependency.
- Whether `SizeConstrainingContainer` is fully retired or fixed-and-kept (depends on other usages).
- Exact GC-mode scoping in `Directory.Build.targets` (which outputs get Workstation GC).
- One PR vs staged (Library then Store).
