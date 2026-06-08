# Portal Store — Catalog-First Hybrid Load (Design)

**Date:** 2026-06-09
**Branch:** `feature/portal-virtualization`
**Status:** Approved design → ready for implementation plan

## Problem

The virtualized Store view is fragile and feels unperformant. Each `StoreCard`, when realized
during scroll, downloads two remote files (per-wallpaper `info.json` + cover image) and coordinates
that work through **shared mutable state**: two static dictionaries (`SPMI.StoreDownloader`,
`SPMI.StoreDownloading`) plus a polling spin-wait (`WaitForCache`, up to 300 × 100 ms = 30 s).

This subsystem has produced a recurring class of bugs across many fix rounds:

- **False red error overlays:** `Cache()` runs via `Task.Run` behind a 4-permit gate, so up to 4
  threadpool threads write the SAME plain `Dictionary<string,bool>` (`StoreDownloading`) while the
  UI thread reads it in `WaitForCache`. Concurrent writes corrupt the dictionary → `"[Theme]=true"`
  writes are lost → the waiter spins to the 30 s timeout → `IsLoadFailed` → red overlay. The cover is
  actually fine, so the card recovers on re-realize (resize/scroll). (Partly mitigated by promoting
  `StoreDownloading` to `ConcurrentDictionary` in commit `6163a4a7`, but the whole coordination is
  unnecessary and brittle.)
- **Stuck-on-load / 30 s hangs** for abandoned or genuinely-failed themes.
- **Prolonged loading state** keeping a full viewport of cards in `IsLoading=true`, which previously
  drove many simultaneous indeterminate `ProgressRing` animations (per-frame UI-thread cost → stutter;
  mitigated by the static placeholder in commit `006b14e1`).

Root cause: the design couples remote per-card downloads to scroll-time realization through fragile
cross-card shared-state coordination. The **Library** view is smooth only because its covers are
already on local disk.

## Catalog data (constraint)

The store catalog (`SSSIW` wallpaper entries) carries only: `Cover` (cover filename), `Live` (GIF
filename), `Source` (repo path), `Pattern` (lowercased search text), `Adult`, `Size`. **Title,
description, and `AppVersion` are NOT in the catalog** — they live in each wallpaper's `info.json`
(`SSTHI`), which must be downloaded. Therefore the title shown on a card must come from `info.json`
(localized via `LocalizationConverter.Convert`), not from the catalog key.

## Goal

Replace the shared-state spin-wait subsystem with an independent, per-card, cancellable load that
renders the card frame instantly and fills in real content (title + description + version + cover)
together when the background fetch completes. Eliminate `StoreDownloader`, the `StoreDownloading`
read path, `WaitForCache`, and the 30 s timeout.

## Non-goals

- Changing the catalog/server format (out of our control).
- Making remote covers appear instantly on first view (impossible — they are remote). First view
  still shows a brief placeholder while the cover downloads; subsequent views are instant from disk.
- Touching the Library view (already smooth) or the shared `Cache()` download helpers' internals
  (kept as the tested download primitive).

## Decisions (from brainstorming)

1. **Hybrid, catalog-first** load (instant frame, background content).
2. **Cover + text arrive together** via a single reuse of the existing `Cache()` helper (downloads
   `info.json` + cover to disk in one fetch). No separate cover-first path.
3. **Title always from `info.json`** (localized). The catalog key is NOT used as a display title.
4. **Loading visual: plain placeholder.** Card frame + static cover placeholder icon; empty
   title/description area until loaded. No spinner, no skeleton.

## Architecture / data flow (per card)

1. **Instant (construction / bind):** card frame visible; cover area shows the static placeholder
   icon (driven by `Thumbnail == null`); title/description empty. No blocking `IsLoading` overlay.
2. **Background (`StoreCard_DataContextChanged` → short ~200 ms debounce → `LoadAsync(token)`):**
   - `await _downloadGate.WaitAsync(token)` (keep the concurrency bound) →
     `bool ok = await Task.Run(() => Cache(Wallpaper, Theme), token)` → `finally { _downloadGate.Release() }`.
   - If `ok`: `Info = SSTHI.ReadJson(<Theme>/<SucroseInfo>)`; `(Title, Description) = LocalizationConverter.Convert(Info)`;
     `IsIncompatible = Info.AppVersion.CompareTo(Entry()) > 0`; check for an in-flight install download
     (existing `StoreService.Info` scan, unchanged); then `await LoadThumbnailAsync(token)` to decode
     the cover from disk via `ImageCache` → placeholder is replaced by the cover.
   - If `!ok`: `IsLoadFailed = true` → red `Warn` overlay (placeholder + red, no title). No spin-wait,
     so this is immediate (no 30 s). Recovers on next realize (resize/scroll) — now fast (no race).
3. **Debounce** (~200 ms) is retained so fast fly-by scrolling does not trigger downloads for cards
   the user passes over; the captured token cancels stale loads on recycle.

## Component changes

- **`ViewModels/StoreCardViewModel.cs`**
  - Remove `EnsureDownloadedAsync`, `DownloadCache`, `WaitForCache`, and all use of
    `SPMI.StoreDownloader` / `SPMI.StoreDownloading`.
  - Add a single `public async Task LoadAsync(CancellationToken Token)` implementing the flow above.
  - Keep `_downloadGate` (4-permit) to bound concurrent cover downloads (tunable later).
  - Keep the install-download path unchanged: `StartDownloadAsync`, `DownloadTheme`, `SendDownload`,
    `HandleInfoChanged`, `Subscribe/UnsubscribeInfoChanged`, progress/observable state, `RefreshMenuState`.
  - `Title`/`Description` start empty and are set once when `Info` loads (no intermediate value).
- **`Views/Controls/StoreCard.xaml`**
  - Card content (cover + title + description + download button) is ALWAYS present; remove the
    `IsLoading`-driven content-hiding (`InverseBoolToVisibility` on the content grid) and the
    `IsLoading` placeholder overlay container.
  - Cover area: the static placeholder icon is shown when `Thumbnail == null` (drive via the existing
    `NullToVisibilityConverter`; verify direction), and hidden once the cover binds.
  - `Warn` (red) overlay remains, shown only on `IsLoadFailed`.
- **`Views/Controls/StoreCard.xaml.cs`**
  - `DataContextChanged`: cancel/dispose old CTS, release old VM's `Thumbnail` (RAM) and
    `UnsubscribeInfoChanged`, capture VM+token locals, `await Task.Delay(200, token)` debounce, then
    `await viewModel.LoadAsync(token)` (replaces the EnsureDownloaded/Subscribe/LoadThumbnail chain).
  - Hover GIF, cursor, rounded-clip, context-menu, unloaded cleanup: unchanged.
- **`Manage/Internal.cs`**
  - Remove `StoreDownloader` (no remaining consumers once `StoreCardViewModel` stops using it).
  - Keep `StoreDownloading` as `ConcurrentDictionary<string,bool>`: it is still written by the shared
    `Cache()` helpers (GitHub/Soferity `Download.Cache`) as their internal cache-state flag. Nothing
    reads it after this change, so it is harmless; `ConcurrentDictionary` keeps the concurrent writes
    safe. (Removing it from the shared helpers is deferred — out of scope here.)

## Preserved (validated behaviors — must not regress)

`JustifiedVirtualizingWrapPanel` virtualization; `ImageCache` bounded LRU; RAM bound via
`Thumbnail = null` on recycle (~174 MB); hover GIF deferred-swap stability; right-click + 3-dot
context menus; download-state icons + progress ring (determinate, `InfoChanged`-driven); rounded
corners; single scrollbar (viewport `MaxHeight` cap); static placeholder.

## Error handling

- `Cache()` returns `false` (genuine network/HTTP failure) → `IsLoadFailed = true` → red overlay,
  card still occupies its slot. Retries automatically on the next realize (resize/scroll), which is
  now fast because there is no shared-state race. A manual click-to-retry is a possible future
  refinement (out of scope).
- Cancellation (recycle) → `OperationCanceledException` swallowed; the next bind resets state.

## Risks & mitigations

- **Regressing validated behaviors** (thumbnails, menus, hover, download icons, RAM): the change is
  scoped to the cover-load path + the loading visual; all other regions are untouched. Verified by
  the existing manual UAT checklist after build.
- **Empty title flash on slow networks:** acceptable per the chosen plain-placeholder UX; covers +
  text arrive together, so no intermediate/incorrect title is shown.
- **`NullToVisibilityConverter` direction:** confirm whether it maps null→Visible or null→Collapsed
  and pick the correct one (or add the inverse) so the placeholder shows exactly while `Thumbnail`
  is null.

## Out of scope / future cleanup

- Removing `StoreDownloading` writes from the shared `Cache()` helpers (GitHub/Soferity `Download.cs`).
- Tuning `_downloadGate` permit count for faster first-visit cover throughput.
- Click-to-retry on failed covers.
- Task 12 (legacy `LibraryStackPanel`/`StoreStackPanel`/`SizeConstrainingContainer` cleanup) remains
  separate.
