using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Wpf.Ui.Controls;
using SHG = Skylark.Helper.Generator;
using SHV = Skylark.Helper.Versionly;
using SMMB = Sucrose.Manager.Manage.Backgroundog;
using SMMCL = Sucrose.Memory.Manage.Constant.Library;
using SMME = Sucrose.Manager.Manage.Engine;
using SMMG = Sucrose.Manager.Manage.General;
using SMMI = Sucrose.Manager.Manage.Internal;
using SMML = Sucrose.Manager.Manage.Library;
using SMMRC = Sucrose.Memory.Manage.Readonly.Content;
using SMMRF = Sucrose.Memory.Manage.Readonly.Folder;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;
using SMMRS = Sucrose.Memory.Manage.Readonly.Soferity;
using SMMRU = Sucrose.Memory.Manage.Readonly.Url;
using SMMVA = Sucrose.Memory.Manage.Valuable.App;
using SPMI = Sucrose.Portal.Manage.Internal;
using SPVCTR = Sucrose.Portal.Views.Controls.ThemeReport;
using SRER = Sucrose.Resources.Extension.Resources;
using SSCHV = Sucrose.Shared.Core.Helper.Version;
using SSDECT = Sucrose.Shared.Dependency.Enum.CommandType;
using SSDESST = Sucrose.Shared.Dependency.Enum.StoreServerType;
using SSDMI = Sucrose.Shared.Dependency.Manage.Internal;
using SSDMMP = Sucrose.Shared.Dependency.Manage.Manager.Portal;
using SSLHK = Sucrose.Shared.Live.Helper.Kill;
using SSLHR = Sucrose.Shared.Live.Helper.Run;
using SSSHC = Sucrose.Shared.Space.Helper.Copy;
using SSSHF = Sucrose.Shared.Space.Helper.Filing;
using SSSHGHD = Sucrose.Shared.Store.Helper.GitHub.Download;
using SSSHL = Sucrose.Shared.Space.Helper.Live;
using SSSHN = Sucrose.Shared.Space.Helper.Network;
using SSSHP = Sucrose.Shared.Space.Helper.Processor;
using SSSHS = Sucrose.Shared.Store.Helper.Store;
using SSSHSD = Sucrose.Shared.Store.Helper.Soferity.Download;
using SSSHU = Sucrose.Shared.Space.Helper.User;
using SSSID = Sucrose.Shared.Store.Interface.Data;
using SSSIW = Sucrose.Shared.Store.Interface.Wallpaper;
using SSSMDTD = Sucrose.Shared.Space.Model.DownloadTelemetryData;
using SSSMI = Sucrose.Shared.Space.Manage.Internal;
using SSSTMI = Sucrose.Shared.Store.Manage.Internal;
using SSTCLC = Sucrose.Shared.Theme.Converter.LocalizationConverter;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Portal.ViewModels
{
    public partial class StoreCardViewModel : CardViewModelBase
    {
        private readonly IStoreCardHost _host;

        // Wallpaper entry from the store catalogue.
        internal KeyValuePair<string, SSSIW> Wallpaper { get; private set; }

        // Unique identifier used to correlate in-flight downloads.
        private readonly string _guid;

        // Random key assigned when a download is started; used to look up
        // progress in StoreService.Info.
        private string _keys = string.Empty;

        // Whether a download is currently in flight.
        private bool _state;

        // Whether the last download attempt faulted.
        private bool _error;

        // Guard for subscribe/unsubscribe against the StoreService.
        private readonly object _subscribeLock = new();

        private bool _subscribed;

        private readonly PropertyChangedEventHandler _infoChangedHandler;

        // Populated after EnsureDownloadedAsync succeeds.
        internal SSTHI Info { get; private set; }

        // ── Observable state exposed to the card ──────────────────────────

        // True while the cover cache download or initial data load is running.
        [ObservableProperty]
        private bool _isLoadFailed;

        // Download progress ring value (0-100).
        [ObservableProperty]
        private double _downloadProgress;

        // Human-readable percentage string ("12.34%").
        [ObservableProperty]
        private string _downloadPercentage = string.Empty;

        // True while an install download is in progress.
        [ObservableProperty]
        private bool _isDownloading;

        // True when install has been completed successfully (card shows checkmark).
        [ObservableProperty]
        private bool _isDownloadComplete;

        // True when a download error occurred (card shows error icon).
        [ObservableProperty]
        private bool _isDownloadError;

        // True when the download is finished or the card is in its normal state.
        [ObservableProperty]
        private bool _isReady;

        // ── Menu state ────────────────────────────────────────────────────

        [ObservableProperty]
        private bool _canInstall;

        [ObservableProperty]
        private bool _canReport;

        [ObservableProperty]
        private bool _updateVisible;

        [ObservableProperty]
        private bool _canUpdate;

        [ObservableProperty]
        private string _installHeader = string.Empty;

        [ObservableProperty]
        private string _updateHeader = string.Empty;

        internal StoreCardViewModel(string Theme, KeyValuePair<string, SSSIW> Wallpaper, IStoreCardHost Host)
        {
            _host = Host;
            this.Theme = Theme;
            this.Wallpaper = Wallpaper;
            _guid = Path.Combine(Wallpaper.Value.Source, Wallpaper.Key);

            _infoChangedHandler = (S, E) => HandleInfoChanged(_keys);
        }

        // ── CardViewModelBase abstract overrides ──────────────────────────

        // ThumbnailPath is the on-disk cached cover image path.
        // It is only valid after EnsureDownloadedAsync has succeeded and Info
        // has been populated; the base LoadThumbnailAsync should be called
        // after that point.
        public override string ThumbnailPath => Info != null
            ? Path.Combine(Theme, Info.Thumbnail)
            : string.Empty;

        // PreviewPath is the remote GIF URL built the same way StoreCard does
        // in StoreCard_MouseEnter; consumed by the card code-behind.
        public override string PreviewPath =>
            $"{SSSHS.Source(SSDMMP.StoreServerType)}/{Wallpaper.Value.Source}/{Wallpaper.Key}/{Wallpaper.Value.Live}";

        // ── StoreService InfoChanged wiring ───────────────────────────────
        // The card code-behind calls Subscribe on Loaded and Unsubscribe on
        // Unloaded (matching StoreCard's StoreCard_Unloaded pattern).

        public void SubscribeInfoChanged()
        {
            lock (_subscribeLock)
            {
                if (!_subscribed)
                {
                    _subscribed = true;
                    SSSTMI.StoreService.InfoChanged += _infoChangedHandler;
                }
            }
        }

        public void UnsubscribeInfoChanged()
        {
            lock (_subscribeLock)
            {
                if (_subscribed)
                {
                    _subscribed = false;
                    SSSTMI.StoreService.InfoChanged -= _infoChangedHandler;
                }
            }
        }

        // ── Cover/metadata cache download ─────────────────────────────────
        // Mirrors StoreCard.DownloadCache + StoreCard_Loaded logic.
        // Called by the card code-behind on Loaded; cancellable for recycling.
        //
        // Returns true when Info was populated successfully and the card can
        // be shown; returns false when the cache download failed (show Warn).
        public async Task<bool> EnsureDownloadedAsync(CancellationToken Token)
        {
            IsLoading = true;

            try
            {
                // DownloadCache is async (cover/info HTTP cache + spin-wait on
                // StoreDownloading); awaited directly so cancellation can
                // interrupt the wait loops instead of leaking a threadpool thread.
                bool Result = await DownloadCache(Token);

                if (Token.IsCancellationRequested)
                {
                    return false;
                }

                if (Result)
                {
                    (string TitleText, string DescriptionText) = SSTCLC.Convert(Info);
                    Title = TitleText;
                    Description = DescriptionText;
                    IsIncompatible = Info.AppVersion.CompareTo(SHV.Entry()) > 0;

                    // Check whether an in-flight download for this wallpaper was
                    // already started before the card was loaded (e.g. user
                    // scrolled away and back during a download).
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

                    IsLoading = false;
                    IsLoadFailed = false;
                }
                else
                {
                    IsLoadFailed = true;
                    IsLoading = false;
                }

                return Result;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch
            {
                IsLoadFailed = true;
                IsLoading = false;
                return false;
            }
        }

        // Mirrors StoreCard.DownloadCache — async (the original spin-waited with
        // await Task.Delay(100)); each wait is cancellation-aware so a recycled
        // card's load can be interrupted without leaking a thread.
        private async Task<bool> DownloadCache(CancellationToken Token)
        {
            if (SPMI.StoreDownloader.ContainsKey(Theme))
            {
                while (!SPMI.StoreDownloading.ContainsKey(Theme) || !SPMI.StoreDownloading[Theme])
                {
                    if (Token.IsCancellationRequested)
                    {
                        return false;
                    }

                    await Task.Delay(100, Token);
                }

                Info = SSTHI.ReadJson(Path.Combine(Theme, SMMRC.SucroseInfo));

                return true;
            }
            else
            {
                SPMI.StoreDownloader[Theme] = false;

                // Cache(...) is synchronous blocking I/O (HTTP via .Result); keep
                // it off the caller's thread without blocking via Task.Run.
                SPMI.StoreDownloader[Theme] = await Task.Run(() => SSDMMP.StoreServerType switch
                {
                    SSDESST.GitHub => SSSHGHD.Cache(Wallpaper, Theme),
                    _ => SSSHSD.Cache(Wallpaper, Theme),
                }, Token);

                if (SPMI.StoreDownloader[Theme])
                {
                    while (!SPMI.StoreDownloading.ContainsKey(Theme) || !SPMI.StoreDownloading[Theme])
                    {
                        if (Token.IsCancellationRequested)
                        {
                            return false;
                        }

                        await Task.Delay(100, Token);
                    }

                    Info = SSTHI.ReadJson(Path.Combine(Theme, SMMRC.SucroseInfo));

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        // ── InfoChanged handler (mirrors StoreCard.StoreService_InfoChanged) ──
        // Called on the InfoChanged event; updates observable progress
        // properties so the card's ring and tooltip can bind to them.
        // UI-only reactions (symbol swaps, Brush changes) stay in the card.
        public void HandleInfoChanged(string Keys)
        {
            if (_keys != Keys)
            {
                return;
            }

            if (!SSSTMI.StoreService.Info.TryGetValue(Keys, out SSSID Key))
            {
                return;
            }

            DownloadProgress = Key.ProgressPercentage;
            DownloadPercentage = Key.Percentage;

            if (Key.ProgressPercentage >= 100 && _state)
            {
                _state = false;

                // Notify the card that the download finished so it can animate
                // the completion icon.  The card code-behind watches
                // IsDownloadComplete and IsDownloadError to drive visual state.
                if (!_error)
                {
                    UnsubscribeInfoChanged();

                    IsDownloading = false;
                    IsDownloadComplete = true;

                    if (SSSTMI.StoreService.Info.ContainsKey(Keys))
                    {
                        SSSTMI.StoreService.Info.Remove(Keys);
                    }
                }
            }
        }

        // ── Install download (mirrors StoreCard.Start + DownloadTheme + SendDownload) ──

        [RelayCommand]
        private async Task Install()
        {
            await StartDownloadAsync();
        }

        // Public so the card's Download_Click / MouseLeftButtonUp can also invoke it.
        public async Task StartDownloadAsync()
        {
            if (Info == null || Info.AppVersion.CompareTo(SHV.Entry()) > 0)
            {
                if (Info != null)
                {
                    // Incompatible version — trigger update launcher as StoreCard.Start does.
                    if (!SSSHP.Work(SSSMI.Update))
                    {
                        SSSHP.Run(SSSMI.Commandog, $"{SMMRG.StartCommand}{SSDECT.Update}{SMMRG.ValueSeparator}{SSSMI.Update}");
                    }
                }

                return;
            }

            // Only start if not already downloading (guard matches SymbolRegular.CloudArrowDown24 check).
            if (IsDownloading)
            {
                return;
            }

            if (!await SSSHN.GetHostEntryAsync())
            {
                IsDownloadError = true;

                await Task.Delay(3000);

                IsDownloadError = false;

                return;
            }

            _state = true;
            _error = false;
            IsDownloading = true;
            IsDownloadComplete = false;
            IsDownloadError = false;
            IsReady = false;

            // Fire-and-forget on the threadpool, matching the original Start()'s
            // non-blocking offload: StartDownloadAsync must NOT block until the
            // whole download finishes — InfoChanged drives progress. Task.Run on a
            // Func<Task> tracks the full async work (unlike the prior async void),
            // and both methods carry their own try/catch so faults stay handled.
            _ = Task.Run(() => SendDownload());

            _ = Task.Run(() => DownloadTheme());
        }

        // Mirrors StoreCard.SendDownload — fire-and-forget telemetry.
        private async Task SendDownload()
        {
            try
            {
                if (SMMG.TelemetryData)
                {
                    SSSMDTD DownloadData = new()
                    {
                        AppVersion = SSCHV.GetText(),
                        WallpaperTitle = Wallpaper.Key,
                        WallpaperVersion = $"{Info.Version}",
                        WallpaperAppVersion = $"{Info.AppVersion}",
                        WallpaperLocation = $"{Wallpaper.Value.Source.Split('/').LastOrDefault()}/{Wallpaper.Key}"
                    };

                    System.Net.Http.StringContent Content = new(Newtonsoft.Json.JsonConvert.SerializeObject(DownloadData, Newtonsoft.Json.Formatting.Indented), SMMRS.Encoding, SMMRS.ApplicationJson);

                    System.Net.Http.HttpResponseMessage Response = await SSDMI.Client.PostAsync($"{SMMRU.Soferity}/{SMMRS.Version}/{SMMRS.Telemetry}/{SMMRS.Download}/{SSSHU.GetGuid()}", Content);

                    Response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        // Mirrors StoreCard.DownloadTheme.
        private async Task DownloadTheme()
        {
            try
            {
                do
                {
                    _keys = SHG.GenerateString(SMMVA.Chars, 25, SMMRG.Randomise);
                } while (Directory.Exists(Path.Combine(SMML.Location, _keys)));

                SubscribeInfoChanged();

                string LibraryPath = Path.Combine(SMML.Location, _keys);
                string TemporaryPath = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName, SMMRF.Cache, SMMRF.Store, SMMRF.Temporary, _keys);

                switch (SSDMMP.StoreServerType)
                {
                    case SSDESST.GitHub:
                        await SSSHGHD.Theme(Path.Combine(Wallpaper.Value.Source, Wallpaper.Key), TemporaryPath, _guid, _keys);
                        break;
                    default:
                        await SSSHSD.Theme(Path.Combine(Wallpaper.Value.Source, Wallpaper.Key), TemporaryPath, _guid, _keys, Wallpaper.Value.Size);
                        break;
                }

                await Task.Delay(100);

                if (Directory.Exists(TemporaryPath))
                {
                    SSSHC.Folder(TemporaryPath, LibraryPath);

                    SSSHF.WriteStream(Path.Combine(LibraryPath, SMMRC.SucroseStore), string.Empty);

                    if ((!SMMB.ClosePerformance && !SMMB.PausePerformance) || !SSSHP.Work(SSSMI.Backgroundog))
                    {
                        if (SMME.StoreStart)
                        {
                            SMMI.LibrarySettingManager.SetSetting(SMMCL.Selected, Path.GetFileName(_keys));

                            if (SSSHL.Run())
                            {
                                SSLHK.Stop();
                            }

                            SSLHR.Start();
                        }
                    }
                }
            }
            catch (Exception Exception)
            {
                _error = true;

                await SSWEW.Watch_CatchException(Exception);

                if (!string.IsNullOrEmpty(_keys) && SSSTMI.StoreService.Info.ContainsKey(_keys))
                {
                    SSSTMI.StoreService.Info.Remove(_keys);
                }

                _state = false;

                UnsubscribeInfoChanged();

                IsDownloading = false;
                IsDownloadError = true;
                IsReady = false;

                await Task.Delay(3000);

                IsDownloadError = false;
                IsReady = true;
            }
        }

        // ── Update command (MenuUpdate_Click) ────────────────────────────

        [RelayCommand]
        private void Update()
        {
            if (!SSSHP.Work(SSSMI.Update))
            {
                SSSHP.Run(SSSMI.Commandog, $"{SMMRG.StartCommand}{SSDECT.Update}{SMMRG.ValueSeparator}{SSSMI.Update}");
            }
        }

        // ── Report command (MenuReport_Click) ────────────────────────────

        [RelayCommand]
        private async Task Report()
        {
            if (Info == null)
            {
                return;
            }

            SPVCTR ThemeReport = new()
            {
                Info = Info,
                Theme = Theme,
                Wallpaper = Wallpaper
            };

            await ThemeReport.ShowAsync();

            ThemeReport.Dispose();
        }

        // ── RefreshMenuState (mirrors StoreCard.ContextMenu_Opened) ───────

        public void RefreshMenuState()
        {
            InstallHeader = SRER.GetValue("Portal", "StoreCard", "MenuInstall");

            CanReport = Info != null;

            if (!IsDownloading && Info != null && Info.AppVersion.CompareTo(SHV.Entry()) <= 0)
            {
                CanInstall = true;

                UpdateVisible = false;
            }
            else
            {
                CanInstall = false;

                if (Info != null && Info.AppVersion.CompareTo(SHV.Entry()) > 0)
                {
                    UpdateVisible = true;

                    if (SSSHP.Work(SSSMI.Update))
                    {
                        CanUpdate = false;

                        UpdateHeader = SRER.GetValue("Portal", "StoreCard", "MenuUpdating");
                    }
                    else
                    {
                        CanUpdate = true;

                        UpdateHeader = SRER.GetValue("Portal", "StoreCard", "MenuUpdate");
                    }

                    InstallHeader += $" ({SRER.GetValue("Portal", "StoreCard", "Incompatible")})";
                }
                else
                {
                    UpdateVisible = false;
                }
            }
        }
    }
}
