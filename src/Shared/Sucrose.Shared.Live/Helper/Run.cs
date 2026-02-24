using System.IO;
using SEDST = Skylark.Enum.DisplayScreenType;
using SHV = Skylark.Helper.Versionly;
using SMMB = Sucrose.Manager.Manage.Backgroundog;
using SMME = Sucrose.Manager.Manage.Engine;
using SMML = Sucrose.Manager.Manage.Library;
using SMMRC = Sucrose.Memory.Manage.Readonly.Content;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SSDECT = Sucrose.Shared.Dependency.Enum.CommandType;
using SSDEWT = Sucrose.Shared.Dependency.Enum.WallpaperType;
using SSDMME = Sucrose.Shared.Dependency.Manage.Manager.Engine;
using SSEHDC = Sucrose.Shared.Space.Helper.DuplicateCounter;
using SSSHB = Sucrose.Shared.Space.Helper.Background;
using SSSHP = Sucrose.Shared.Space.Helper.Processor;
using SSSMI = Sucrose.Shared.Space.Manage.Internal;
using SSTHI = Sucrose.Shared.Theme.Helper.Info;
using SWUD = Skylark.Wing.Utility.Desktop;
using SWUS = Skylark.Wing.Utility.Screene;

namespace Sucrose.Shared.Live.Helper
{
    internal static class Run
    {
        private static void LaunchLiveEngine(SSDECT CommandType, string EnginePath)
        {
            string command = $"{SMMRG.StartCommand}{CommandType}{SMMRG.ValueSeparator}{EnginePath}";

            if (SMME.DisplayScreenType == SEDST.SameDuplicate)
            {
                SWUS.Initialize();

                int screenCount = SWUS.Screens.Count();

                SSEHDC.Reset();

                for (int i = 0; i < screenCount; i++)
                {
                    SSSHP.Run(SSSMI.Commandog, command);
                }
            }
            else
            {
                SSSHP.Run(SSSMI.Commandog, command);
            }
        }

        public static void Start()
        {
            if ((!SMMB.ClosePerformance && !SMMB.PausePerformance) || !SSSHP.Work(SSSMI.Backgroundog))
            {
                string InfoPath = Path.Combine(SMML.Location, SMML.Selected, SMMRC.SucroseInfo);

                if (File.Exists(InfoPath) && SSTHI.ReadCheck(InfoPath))
                {
                    SSTHI Info = SSTHI.ReadJson(InfoPath);

                    if (Info.AppVersion.CompareTo(SHV.Entry()) <= 0)
                    {
                        SWUD.RefreshDesktop();

                        if (SMME.BackgroundImage)
                        {
                            SSSHB.SetWallpaper(Path.Combine(SMML.Location, SMML.Selected, Info.Thumbnail));
                        }

                        if (SMMB.PerformanceCounter)
                        {
                            SSSHP.Run(SSSMI.Commandog, $"{SMMRG.StartCommand}{SSDECT.Backgroundog}{SMMRG.ValueSeparator}{SSSMI.Backgroundog}");
                        }

                        switch (Info.Type)
                        {
                            case SSDEWT.Gif:
                                LaunchLiveEngine(SSDECT.Live, SSSMI.EngineLive[SSDMME.Gif]);
                                break;
                            case SSDEWT.Url:
                                LaunchLiveEngine(SSDECT.Live, SSSMI.EngineLive[SSDMME.Url]);
                                break;
                            case SSDEWT.Web:
                                LaunchLiveEngine(SSDECT.Live, SSSMI.EngineLive[SSDMME.Web]);
                                break;
                            case SSDEWT.Video:
                                LaunchLiveEngine(SSDECT.Live, SSSMI.EngineLive[SSDMME.Video]);
                                break;
                            case SSDEWT.YouTube:
                                LaunchLiveEngine(SSDECT.Live, SSSMI.EngineLive[SSDMME.YouTube]);
                                break;
                            case SSDEWT.Application:
                                SSSHP.Run(SSSMI.Commandog, $"{SMMRG.StartCommand}{SSDECT.Live}{SMMRG.ValueSeparator}{SSSMI.EngineLive[SSDMME.Application]}");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
    }
}