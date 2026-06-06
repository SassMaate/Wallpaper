using System.IO;
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

        internal SSTHI Info { get; private set; }

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

        internal LibraryCardViewModel(string Theme, SSTHI Info, ILibraryCardHost Host)
        {
            _host = Host;
            this.Info = Info;
            this.Theme = Theme;

            (string TitleText, string DescriptionText) = SSTCLC.Convert(Info);
            Title = TitleText;
            Description = DescriptionText;
            IsIncompatible = Info.AppVersion.CompareTo(SHV.Entry()) > 0;
        }

        public override string ThumbnailPath => Path.Combine(Theme, Info.Thumbnail);

        public override string PreviewPath => Path.Combine(Theme, Info.Preview);

        public bool IsSelectedAndRunning()
        {
            return SMML.Selected == Path.GetFileName(Theme) && SSSHL.Run();
        }

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
                SPVCTE ThemeEdit = new()
                {
                    Info = Info,
                    Theme = Theme
                };

                ContentDialogResult Result = await ThemeEdit.ShowAsync();

                if (Result == ContentDialogResult.Primary)
                {
                    Info = SSTHI.ReadJson(Path.Combine(Theme, SMMRC.SucroseInfo));

                    (string TitleText, string DescriptionText) = SSTCLC.Convert(Info);
                    Title = TitleText;
                    Description = DescriptionText;
                }

                ThemeEdit.Dispose();
            }
        }

        [RelayCommand]
        private async Task Share()
        {
            if (Directory.Exists(Theme))
            {
                SPVCTS ThemeShare = new()
                {
                    Info = Info,
                    Theme = Theme
                };

                await ThemeShare.ShowAsync();

                ThemeShare.Dispose();
            }
        }

        [RelayCommand]
        private async Task Review()
        {
            if (Directory.Exists(Theme))
            {
                SPVCTR ThemeReview = new()
                {
                    Info = Info,
                    Theme = Theme
                };

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
                SPVCTD ThemeDelete = new()
                {
                    Info = Info,
                    Theme = Theme
                };

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

        public void RefreshMenuState()
        {
            UseHeader = SRER.GetValue("Portal", "LibraryCard", "MenuUse");
            DeleteHeader = SRER.GetValue("Portal", "LibraryCard", "MenuDelete");
            CustomizeHeader = SRER.GetValue("Portal", "LibraryCard", "MenuCustomize");

            string PropertiesPath = Path.Combine(Theme, SMMRC.SucroseProperties);

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

            if ((!SMMB.ClosePerformance && !SMMB.PausePerformance) || !SSSHP.Work(SSSMI.Backgroundog))
            {
                if (SMML.Selected == Path.GetFileName(Theme) && SSSHL.Run())
                {
                    CanUse = false;
                    CanDelete = false;

                    UseHeader += $" ({SRER.GetValue("Portal", "LibraryCard", "Selected")})";
                    DeleteHeader += $" ({SRER.GetValue("Portal", "LibraryCard", "Selected")})";
                }
                else
                {
                    if (Info.AppVersion.CompareTo(SHV.Entry()) <= 0)
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

                if (SMMB.ClosePerformance)
                {
                    UseHeader += $" ({SRER.GetValue("Portal", "LibraryCard", "Closed")})";
                    DeleteHeader += $" ({SRER.GetValue("Portal", "LibraryCard", "Closed")})";
                    CustomizeHeader += $" ({SRER.GetValue("Portal", "LibraryCard", "Closed")})";
                }
                else if (SMMB.PausePerformance)
                {
                    UseHeader += $" ({SRER.GetValue("Portal", "LibraryCard", "Paused")})";
                    DeleteHeader += $" ({SRER.GetValue("Portal", "LibraryCard", "Paused")})";
                    CustomizeHeader += $" ({SRER.GetValue("Portal", "LibraryCard", "Paused")})";
                }
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
