using System.IO;
using System.Net.Http;
using SMMG = Sucrose.Manager.Manage.General;
using SMMO = Sucrose.Manager.Manage.Objectionable;
using SMMP = Sucrose.Manager.Manage.Portal;
using SMMRC = Sucrose.Memory.Manage.Readonly.Content;
using SMMRGH = Sucrose.Memory.Manage.Readonly.GitHub;
using SMMRU = Sucrose.Memory.Manage.Readonly.Url;
using SPMI = Sucrose.Portal.Manage.Internal;
using SSDMI = Sucrose.Shared.Dependency.Manage.Internal;
using SSHG = Skylark.Standard.Helper.GitHub;
using SSIIC = Skylark.Standard.Interface.IContents;
using SSSHF = Sucrose.Shared.Space.Helper.Filing;
using SSSHS = Sucrose.Shared.Store.Helper.Store;
using SSSID = Sucrose.Shared.Store.Interface.Data;
using SSSIW = Sucrose.Shared.Store.Interface.Wallpaper;
using SSSMI = Sucrose.Shared.Store.Manage.Internal;

namespace Sucrose.Shared.Store.Helper.GitHub
{
    internal static class Download
    {
        public static bool Store(string Store)
        {
            string StorePath = Path.GetDirectoryName(Store);

            if (Directory.Exists(StorePath))
            {
                if (File.Exists(Store))
                {
                    DateTime CurrentTime = DateTime.Now;
                    DateTime ModificationTime = File.GetLastWriteTime(Store);

                    TimeSpan ElapsedDuration = CurrentTime - ModificationTime;

                    if (ElapsedDuration >= TimeSpan.FromHours(SMMP.StoreDuration) || !SSSHS.ReadCheck(Store))
                    {
                        SSSHF.Delete(Store);
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                Directory.CreateDirectory(StorePath);
            }

            InitializeClient();

            try
            {
                List<SSIIC> Contents = SSHG.ContentsList(SMMRGH.Owner, SMMRGH.StoreRepository, SMMRGH.StoreSource, SMMRGH.Branch);

                foreach (SSIIC Content in Contents)
                {
                    if (Content.Name == SMMRC.StoreFile)
                    {
                        using HttpResponseMessage Response = SSDMI.ClientGitHub.GetAsync(Content.DownloadUrl).Result;

                        Response.EnsureSuccessStatusCode();

                        if (Response.IsSuccessStatusCode)
                        {
                            using (Stream Stream = Response.Content.ReadAsStreamAsync().Result)
                            using (FileStream FStream = new(Store, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                            {
                                Stream.CopyTo(FStream);
                            }

                            return SSSHS.ReadCheck(Store);
                        }

                        break;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        public static bool Cache(KeyValuePair<string, SSSIW> Wallpaper, string Theme)
        {
            string InfoPath = Path.Combine(Theme, SMMRC.SucroseInfo);
            string CoverPath = Path.Combine(Theme, Wallpaper.Value.Cover);

            if (Directory.Exists(Theme))
            {
                if (File.Exists(InfoPath) && File.Exists(CoverPath))
                {
                    DateTime CurrentTime = DateTime.Now;
                    DateTime ModificationTime = File.GetLastWriteTime(Theme);

                    TimeSpan ElapsedDuration = CurrentTime - ModificationTime;

                    if (ElapsedDuration >= TimeSpan.FromHours(SMMP.StoreDuration))
                    {
                        SSSHF.Delete(InfoPath);
                        SSSHF.Delete(CoverPath);

                        SPMI.StoreDownloading[Theme] = false;
                    }
                    else
                    {
                        SPMI.StoreDownloading[Theme] = true;

                        return true;
                    }
                }
                else
                {
                    if (File.Exists(InfoPath))
                    {
                        SSSHF.Delete(InfoPath);
                    }

                    if (File.Exists(CoverPath))
                    {
                        SSSHF.Delete(CoverPath);
                    }

                    SPMI.StoreDownloading[Theme] = false;
                }
            }
            else
            {
                Directory.CreateDirectory(Theme);
            }

            if (SPMI.StoreDownloading.ContainsKey(Theme) && SPMI.StoreDownloading[Theme])
            {
                return true;
            }
            else
            {
                SPMI.StoreDownloading[Theme] = false;

                InitializeClient();

                try
                {
                    string InfoUri = EncodeSpacesOnly($"{SMMRU.RawGitHubStoreBranch}/{Wallpaper.Value.Source}/{Wallpaper.Key}/{SMMRC.SucroseInfo}");
                    string CoverUri = EncodeSpacesOnly($"{SMMRU.RawGitHubStoreBranch}/{Wallpaper.Value.Source}/{Wallpaper.Key}/{Wallpaper.Value.Cover}");

                    using HttpResponseMessage ResponseInfo = SSDMI.ClientGitHub.GetAsync(InfoUri).Result;
                    using HttpResponseMessage ResponseCover = SSDMI.ClientGitHub.GetAsync(CoverUri).Result;

                    ResponseInfo.EnsureSuccessStatusCode();
                    ResponseCover.EnsureSuccessStatusCode();

                    if (ResponseInfo.IsSuccessStatusCode && ResponseCover.IsSuccessStatusCode)
                    {
                        using (FileStream InfoFile = new(InfoPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                        using (FileStream CoverFile = new(CoverPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                        {
                            ResponseInfo.Content.CopyToAsync(InfoFile).Wait();
                            ResponseCover.Content.CopyToAsync(CoverFile).Wait();
                        }

                        SPMI.StoreDownloading[Theme] = true;

                        return true;
                    }
                }
                catch
                {
                    return false;
                }

                return false;
            }
        }

        public static async Task<bool> Theme(string Source, string Output, string Guid, string Keys, bool Sub = true)
        {
            InitializeClient();

            SSSMI.StoreService.Info[Keys] = new SSSID(0, 0, 0, "0%", "0/0", Guid);

            return await DownloadFolder(Source, Output, Keys, Sub);
        }

        private static string EncodeSpacesOnly(string Source)
        {
            return Source.Replace(" ", "%20");
        }

        private static void InitializeClient()
        {
            if (SSSMI.State)
            {
                SSSMI.State = false;

                SSDMI.ClientGitHub.DefaultRequestHeaders.Clear();

                SSDMI.ClientGitHub.DefaultRequestHeaders.Add("User-Agent", SMMG.UserAgent);

                if (!string.IsNullOrEmpty(SMMO.PersonalAccessToken))
                {
                    SSDMI.ClientGitHub.DefaultRequestHeaders.Add("Authorization", $"Bearer {SMMO.PersonalAccessToken}");
                }
            }
        }

        private static async Task<bool> DownloadFolder(string Source, string Output, string Keys, bool Sub)
        {
            SSSMI.StoreService.TotalFileCount(Keys, await GetTotalFileCount(Source, Sub));

            return await DownloadFilesRecursively(Source, Output, Keys, Sub);
        }

        private static async Task<int> GetTotalFileCount(string Source, bool Sub)
        {
            List<SSIIC> Contents = SSHG.ContentsList(SMMRGH.Owner, SMMRGH.StoreRepository, Source, SMMRGH.Branch);

            int Count = 0;

            foreach (SSIIC Content in Contents)
            {
                if (Content.Type == "file")
                {
                    Count++;
                }
                else if (Content.Type == "dir" && Sub)
                {
                    Source = Content.Path;

                    int SubTotalFileCount = await GetTotalFileCount(Source, Sub);

                    Count += SubTotalFileCount;
                }
            }

            return Count;
        }

        private static async Task<bool> DownloadFilesRecursively(string Source, string Output, string Keys, bool Sub)
        {
            List<SSIIC> Contents = SSHG.ContentsList(SMMRGH.Owner, SMMRGH.StoreRepository, Source, SMMRGH.Branch);

            foreach (SSIIC Content in Contents)
            {
                if (Content.Type == "file")
                {
                    string FilePath = Path.Combine(Output, Content.Name);

                    if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                    }

                    using HttpResponseMessage Response = await SSDMI.ClientGitHub.GetAsync(Content.DownloadUrl);

                    Response.EnsureSuccessStatusCode();

                    using (Stream Stream = await Response.Content.ReadAsStreamAsync())
                    using (FileStream FStream = new(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await Stream.CopyToAsync(FStream);
                    }

                    SSSMI.StoreService.DownloadedFileCount(Keys, SSSMI.StoreService.Info[Keys].DownloadedFileCount + 1);
                    SSSMI.StoreService.ProgressPercentage(Keys, (double)SSSMI.StoreService.Info[Keys].DownloadedFileCount / SSSMI.StoreService.Info[Keys].TotalFileCount * 100);

                    SSSMI.StoreService.Percentage(Keys, $"{SSSMI.StoreService.Info[Keys].ProgressPercentage:F2}%"); //F2 - F0
                    SSSMI.StoreService.State(Keys, $"{SSSMI.StoreService.Info[Keys].DownloadedFileCount}/{SSSMI.StoreService.Info[Keys].TotalFileCount}");
                }
                else if (Content.Type == "dir" && Sub)
                {
                    Source = Content.Path;
                    string SubOutput = Path.Combine(Output, Content.Name);

                    await DownloadFilesRecursively(Source, SubOutput, Keys, Sub);
                }
            }

            return true;
        }
    }
}