# Portal Store Catalog-First Load Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the fragile shared-state spin-wait that loads Store cover/metadata with an independent, cancellable, per-card `LoadAsync`, so cards render a plain placeholder instantly and reveal title + description + version + cover together when the background fetch completes — eliminating the false-red-overlay / 30 s-hang bug class.

**Architecture:** Each `StoreCardViewModel` owns its load. On bind (after a short debounce) the code-behind calls `LoadAsync(token)`, which (only when metadata is not yet loaded) downloads `info.json` + cover to disk via the existing `Cache()` helper behind the 4-permit gate, reads `Info`, sets localized title/description and version-compat, then decodes the cover from disk via `ImageCache`. No `StoreDownloader`, no `StoreDownloading` read, no `WaitForCache`, no 30 s timeout. The existing `IsLoading`-driven static placeholder overlay already delivers the approved "plain placeholder → everything together" UX, so no XAML restructure is needed.

**Tech Stack:** .NET 10 WPF, CommunityToolkit.Mvvm, WPF-UI, the existing `Sucrose.Shared.Store` `Cache()` download helpers, `ImageCache`.

---

## Testing note

This repository has **no automated test framework** (no test projects exist — see CLAUDE.md). WPF rendering/async-timing behavior cannot be unit-tested here. Each task is therefore verified by:
1. **Build:** `dotnet build src/Portal/Sucrose.Portal/Sucrose.Portal.csproj -c Release -p:PlatformTarget=x64 -p:UseSharedCompilation=false -clp:ErrorsOnly` → expect `Oluşturma başarılı oldu.` with `0 Hata`.
2. **Manual UAT** at the human checkpoint (Task 3).

Build + commit happen per task. Do not stage `-A`; stage only the listed paths. Never touch the dirty `libmpv-*.dll` files. Commit messages end with the `Co-Authored-By: Claude Opus 4.8 (1M context) <noreply@anthropic.com>` trailer.

## File structure

- `src/Portal/Sucrose.Portal/ViewModels/StoreCardViewModel.cs` — remove the spin-wait subsystem; add `LoadAsync`. (Task 1)
- `src/Portal/Sucrose.Portal/Views/Controls/StoreCard.xaml.cs` — `DataContextChanged` calls `LoadAsync`. (Task 1)
- `src/Portal/Sucrose.Portal/Manage/Internal.cs` — remove the now-unused `StoreDownloader`. (Task 2)
- `StoreCard.xaml` — **unchanged** (existing `IsLoading` overlay + static placeholder already delivers the UX).

---

### Task 1: Replace the Store load path with `LoadAsync`

**Files:**
- Modify: `src/Portal/Sucrose.Portal/ViewModels/StoreCardViewModel.cs` (remove `EnsureDownloadedAsync`, `DownloadCache`, `WaitForCache`; add `LoadAsync`)
- Modify: `src/Portal/Sucrose.Portal/Views/Controls/StoreCard.xaml.cs` (`StoreCard_DataContextChanged`)

- [ ] **Step 1: Remove the three spin-wait methods from `StoreCardViewModel.cs`**

Delete the entire block from the `EnsureDownloadedAsync` XML-comment header through the end of `WaitForCache` (the current methods `EnsureDownloadedAsync`, `DownloadCache`, and `WaitForCache`, roughly lines 186–329, i.e. everything from the comment `// ── Cover/metadata cache download ──` down to and including the closing brace of `WaitForCache`). These are the only references to `SPMI.StoreDownloader` and `SPMI.StoreDownloading` in this file, so removing them removes all VM usage of both dictionaries.

Keep everything else unchanged: the `_downloadGate` field, the install-download path (`Install`, `StartDownloadAsync`, `SendDownload`, `DownloadTheme`), `HandleInfoChanged`, `SubscribeInfoChanged`/`UnsubscribeInfoChanged`, `RefreshMenuState`, `Update`, `Report`, the observable properties, `ThumbnailPath`, `PreviewPath`, and the constructor.

- [ ] **Step 2: Add `LoadAsync` in the same place the removed block occupied**

Insert this method where `EnsureDownloadedAsync` used to be (after `UnsubscribeInfoChanged`, before the install-download region). It reuses the existing `using` aliases already present in the file (`SSTHI`, `SSTCLC`, `SHV`, `SSDMMP`, `SSDESST`, `SSSHGHD`, `SSSHSD`, `SSSTMI`, `SSSID`, `SMMRC`).

```csharp
        // ── Cover/metadata load ───────────────────────────────────────────
        // Replaces the old EnsureDownloadedAsync/DownloadCache/WaitForCache spin-wait.
        // Each card loads independently and cancellably:
        //   * first bind (Info == null): download info.json + cover to disk via the existing
        //     Cache() helper behind the 4-permit gate, then read Info and set localized
        //     title/description + version-compat;
        //   * every bind: re-detect an install download already in flight for this wallpaper;
        //   * always: decode the cover from disk (ImageCache) -> IsLoading flips false and the
        //     placeholder is replaced by the cover.
        // No shared StoreDownloader/StoreDownloading coordination, so no cross-card race and no
        // 30s timeout: a genuine download failure shows the red overlay immediately and retries
        // fast on the next realize.
        public async Task LoadAsync(CancellationToken Token)
        {
            IsLoading = true;
            IsLoadFailed = false;

            try
            {
                if (Info == null)
                {
                    await _downloadGate.WaitAsync(Token);

                    bool Result;

                    try
                    {
                        // Cache(...) is synchronous blocking I/O (HTTP via .Result); keep it off
                        // the UI thread. It downloads info.json + cover to the Theme folder and
                        // returns true when both are on disk.
                        Result = await Task.Run(() => SSDMMP.StoreServerType switch
                        {
                            SSDESST.GitHub => SSSHGHD.Cache(Wallpaper, Theme),
                            _ => SSSHSD.Cache(Wallpaper, Theme),
                        }, Token);
                    }
                    finally
                    {
                        _downloadGate.Release();
                    }

                    if (Token.IsCancellationRequested)
                    {
                        return;
                    }

                    if (!Result)
                    {
                        IsLoadFailed = true;
                        IsLoading = false;

                        return;
                    }

                    Info = SSTHI.ReadJson(Path.Combine(Theme, SMMRC.SucroseInfo));

                    (string TitleText, string DescriptionText) = SSTCLC.Convert(Info);

                    Title = TitleText;
                    Description = DescriptionText;
                    IsIncompatible = Info.AppVersion.CompareTo(SHV.Entry()) > 0;
                }

                // Pick up an install download that was already in flight for this wallpaper
                // (the user may have started it, scrolled away — unsubscribing — and scrolled back).
                if (!IsDownloading)
                {
                    KeyValuePair<string, SSSID> Matching = SSSTMI.StoreService.Info.FirstOrDefault(Pair => Pair.Value.Guid == _guid && Pair.Value.ProgressPercentage < 100);

                    if (!Matching.Equals(default(KeyValuePair<string, SSSID>)))
                    {
                        _keys = Matching.Key;
                        _state = true;

                        SubscribeInfoChanged();
                        HandleInfoChanged(_keys);

                        IsDownloading = true;
                        IsReady = false;
                    }
                    else
                    {
                        IsReady = true;
                    }
                }

                // Cover is on disk now (Info populated => ThumbnailPath valid); decode it.
                // LoadThumbnailAsync sets IsLoading = false on success.
                await LoadThumbnailAsync(Token);

                IsLoadFailed = false;
            }
            catch (OperationCanceledException)
            {
                // expected on recycle
            }
            catch
            {
                IsLoadFailed = true;
                IsLoading = false;
            }
        }
```

- [ ] **Step 3: Update `StoreCard_DataContextChanged` in `StoreCard.xaml.cs`**

Replace the current handler body (the one that does `Task.Delay(200)` → `EnsureDownloadedAsync` → `SubscribeInfoChanged` → `LoadThumbnailAsync`) with the version below. `LoadAsync` now performs the subscribe internally, so the code-behind no longer calls `SubscribeInfoChanged`.

```csharp
        private async void StoreCard_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            ClearPreview();

            if (e.OldValue is SPVMSCVM oldVm)
            {
                oldVm.UnsubscribeInfoChanged();

                // Release the scrolled-away card's bitmap. The VM stays alive in the
                // (3000+) collection, so without this every cover ever shown would stay
                // resident -> GBs of RAM. The bounded ImageCache still re-serves recent
                // covers instantly on scroll-back.
                oldVm.Thumbnail = null;
            }

            if (ViewModel is not SPVMSCVM viewModel)
            {
                return;
            }

            _cts = new CancellationTokenSource();

            // Capture the VM and token as LOCALS: this is an async handler and the card
            // recycles fast, so a newer DataContextChanged can null/replace the _cts field
            // (and the DataContext) while we await. Reading the field after an await would
            // throw NullReference; the captured token still lets a stale invocation bail out.
            CancellationToken token = _cts.Token;

            try
            {
                // Debounce: only load for cards the user actually lingers on, so fast scrolling
                // (which recycles the card within this window and cancels the token) never floods
                // thousands of cover downloads/decodes.
                await Task.Delay(200, token);

                await viewModel.LoadAsync(token);
            }
            catch (OperationCanceledException)
            {
                // expected on recycle
            }
        }
```

- [ ] **Step 4: Build**

Run: `dotnet build src/Portal/Sucrose.Portal/Sucrose.Portal.csproj -c Release -p:PlatformTarget=x64 -p:UseSharedCompilation=false -clp:ErrorsOnly`
Expected: `Oluşturma başarılı oldu.` with `0 Hata`. (If `EnsureDownloadedAsync`/`SubscribeInfoChanged` are reported as undefined, a caller was missed — fix the caller, do not re-add the methods.)

- [ ] **Step 5: Commit**

```bash
git add src/Portal/Sucrose.Portal/ViewModels/StoreCardViewModel.cs src/Portal/Sucrose.Portal/Views/Controls/StoreCard.xaml.cs
git commit -m "refactor(portal): replace Store spin-wait load with independent per-card LoadAsync"
```

---

### Task 2: Remove the now-unused `StoreDownloader`

**Files:**
- Modify: `src/Portal/Sucrose.Portal/Manage/Internal.cs`

- [ ] **Step 1: Confirm there are no remaining references**

Run a search for `StoreDownloader` across `src/`. After Task 1 the only hit must be the declaration in `Internal.cs`. (`StoreDownloading` will still appear in the shared `Cache()` helpers — that is expected and stays.)

- [ ] **Step 2: Delete the `StoreDownloader` field and its comment**

Remove these lines from `Internal.cs`:

```csharp
        // StoreDownloader is only ever touched on the UI thread (the synchronous parts of
        // StoreCardViewModel.DownloadCache/WaitForCache and their await continuations), so a
        // plain Dictionary is safe here.
        public static Dictionary<string, bool> StoreDownloader = [];

```

Keep the `StoreDownloading` `ConcurrentDictionary` field and its comment: the shared `Cache()` helpers still write it as their internal cache-state flag (now harmless dead state for the Portal, kept thread-safe). The `using System.Collections.Concurrent;` directive stays (still used by `StoreDownloading`).

- [ ] **Step 3: Build**

Run: `dotnet build src/Portal/Sucrose.Portal/Sucrose.Portal.csproj -c Release -p:PlatformTarget=x64 -p:UseSharedCompilation=false -clp:ErrorsOnly`
Expected: `Oluşturma başarılı oldu.` with `0 Hata`.

- [ ] **Step 4: Commit**

```bash
git add src/Portal/Sucrose.Portal/Manage/Internal.cs
git commit -m "chore(portal): remove unused StoreDownloader dictionary"
```

---

### Task 3: Build + manual UAT (human checkpoint)

**Files:** none (verification only)

- [ ] **Step 1: Full Portal build**

Run: `dotnet build src/Portal/Sucrose.Portal/Sucrose.Portal.csproj -c Release -p:PlatformTarget=x64 -p:UseSharedCompilation=false`
Expected: `Oluşturma başarılı oldu.` with `0 Uyarı` and `0 Hata`.

- [ ] **Step 2: Run the app and walk the UAT checklist (human)**

Open the Store, pick a category, scroll up and down (fast and slow), revisit a category:
- **No false red overlays:** cards load on the first pass without needing a window-resize/scroll to recover.
- **Plain placeholder → everything together:** during load a static (non-spinning) placeholder shows; when a card finishes, title + description + version state + cover appear together.
- **Genuine failures (if any) are immediate:** a card that truly can't fetch shows the red overlay right away (no multi-second hang) and recovers quickly on re-scroll.
- **Perf:** scrolling feels smooth; no stutter accumulating with card count.
- **RAM:** footer stays bounded (~174 MB range) after scrolling the full catalog.
- **Preserved behaviors intact:** thumbnails render; right-click + 3-dot menus work; hover GIF is stable; download button + progress ring + completion/error icons work on an actual install; rounded corners; single scrollbar.

- [ ] **Step 3: Record the result**

If all pass, the Store load rewrite is complete. If anything fails, capture the exact symptom (screenshot + which card/category) and return to systematic-debugging before further changes — do NOT stack speculative fixes.

---

## Self-review

- **Spec coverage:** Remove `StoreDownloader`/`StoreDownloading`-read/`WaitForCache`/30 s timeout (Task 1 + Task 2 ✓); independent cancellable per-card load (Task 1 `LoadAsync` ✓); title always from `info.json` localized (Task 1 `SSTCLC.Convert` ✓); cover + text together (single `Cache()` fetch then `LoadThumbnailAsync` ✓); plain placeholder / instant frame (existing `IsLoading` overlay + static placeholder, no XAML change — noted ✓); failure → immediate red + fast retry on realize (Task 1 ✓); preserved behaviors (UAT, Task 3 ✓). The spec's "remove IsLoading content-hiding / Thumbnail-null placeholder" XAML bullet is intentionally **not** implemented: the existing `IsLoading`-driven static placeholder already yields the approved UX (plain placeholder, everything-together) with lower layering risk; this is a deliberate, flagged refinement.
- **Placeholder scan:** No TBD/TODO; all code blocks are complete.
- **Type consistency:** `LoadAsync(CancellationToken)` is the only new public member; the code-behind calls exactly that. `Info`, `_guid`, `_keys`, `_state`, `_downloadGate`, `IsDownloading`, `IsReady`, `IsLoadFailed`, `IsLoading`, `Title`, `Description`, `IsIncompatible`, `SubscribeInfoChanged`, `HandleInfoChanged`, `LoadThumbnailAsync` all already exist on the VM/base. Aliases used (`SSTHI`, `SSTCLC`, `SHV`, `SSDMMP`, `SSDESST`, `SSSHGHD`, `SSSHSD`, `SSSTMI`, `SSSID`, `SMMRC`) are all already imported in `StoreCardViewModel.cs`.
