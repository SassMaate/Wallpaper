using System.Diagnostics;
using SMME = Sucrose.Manager.Manage.Engine;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SSDECT = Sucrose.Shared.Dependency.Enum.CommandType;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SSSHP = Sucrose.Shared.Space.Helper.Processor;
using SSSMI = Sucrose.Shared.Space.Manage.Internal;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;
using SWHWAPI = Skylark.Wing.Helper.WinAPI;
using Timer = System.Timers.Timer;

namespace Sucrose.Shared.Engine.Helper
{
    internal static class Crashing
    {
        public static void Start()
        {
            int Second = 5;

            Timer Crasher = new(Second * 1000);

            Crasher.Elapsed += async (s, e) =>
            {
                try
                {
                    if (SMME.CrashExplorer)
                    {
                        IntPtr Progman = SWHWAPI.FindWindow("Progman", "Program Manager");

                        if (Progman == IntPtr.Zero)
                        {
                            Process.Start("explorer.exe");

                            await Task.Delay(3000);

                            SSSHP.Run(SSSMI.Commandog, $"{SMMRG.StartCommand}{SSDECT.RestartLive}{SMMRG.ValueSeparator}{SMMRG.Unknown}");
                        }
                        else
                        {
                            if (SSEMI.Progman == IntPtr.Zero)
                            {
                                SSEMI.Progman = Progman;
                            }
                            else if (SSEMI.Progman != Progman)
                            {
                                SSEMI.Progman = Progman;

                                SSSHP.Run(SSSMI.Commandog, $"{SMMRG.StartCommand}{SSDECT.RestartLive}{SMMRG.ValueSeparator}{SMMRG.Unknown}");
                            }
                        }
                    }
                }
                catch (Exception Exception)
                {
                    await SSWEW.Watch_CatchException(Exception);
                }
            };

            Crasher.AutoReset = true;

            Crasher.Start();
        }
    }
}