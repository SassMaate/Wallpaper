using SSDSH = Sucrose.Shared.Dependency.Struct.Handle;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;
using SWEACAM = Skylark.Wing.Extension.AudioController.AudioManager;
using SWEVPCAM = Skylark.Wing.Extension.VideoPlayerController.AudioManager;

namespace Sucrose.Shared.Engine.Aurora.Helper
{
    internal static class Application
    {
        public static async void SetVolume(int Volume)
        {
            try
            {
                if (SSEMI.Applications.Any())
                {
                    SWEVPCAM.SetApplicationVolume(SSEMI.Applications.FirstOrDefault().Process.Id, Volume);

                    if (SSEMI.Applications.Count > 1)
                    {
                        foreach (SSDSH Application in SSEMI.Applications.Skip(1))
                        {
                            SWEVPCAM.SetApplicationVolume(Application.Process.Id, 0);
                        }
                    }
                }
            }
            catch
            {
                try
                {
                    if (SSEMI.Applications.Any())
                    {
                        SWEACAM.SetApplicationVolume(SSEMI.Applications.FirstOrDefault().Process.Id, Volume);

                        if (SSEMI.Applications.Count > 1)
                        {
                            foreach (SSDSH Application in SSEMI.Applications.Skip(1))
                            {
                                SWEACAM.SetApplicationVolume(Application.Process.Id, 0);
                            }
                        }
                    }
                }
                catch (Exception Exception)
                {
                    await SSWEW.Watch_CatchException(Exception);
                }
            }
        }
    }
}