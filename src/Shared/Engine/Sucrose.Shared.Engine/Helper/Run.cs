using SEDST = Skylark.Enum.DisplayScreenType;
using SMME = Sucrose.Manager.Manage.Engine;
using SMMB = Sucrose.Manager.Manage.Backgroundog;
using SMMRA = Sucrose.Memory.Manage.Readonly.App;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SSDECT = Sucrose.Shared.Dependency.Enum.CommandType;
using SSDEET = Sucrose.Shared.Dependency.Enum.EngineType;
using SSSHP = Sucrose.Shared.Space.Helper.Processor;
using SSSMI = Sucrose.Shared.Space.Manage.Internal;
using SWUS = Skylark.Wing.Utility.Screene;

namespace Sucrose.Shared.Engine.Helper
{
    internal static class Run
    {
        public static bool Check()
        {
            int Result = 0;

            foreach (KeyValuePair<SSDEET, string> Pair in SSSMI.EngineLive)
            {
                if (SSSHP.Work(Pair.Value))
                {
                    Result += SSSHP.WorkCount(SSSMI.EngineLive[Pair.Key]);
                }
            }

            // In SameDuplicate mode, allow one engine per screen
            if (SMME.DisplayScreenType == SEDST.SameDuplicate)
            {
                SWUS.Initialize();

                int screenCount = SWUS.Screens.Count();

                return Result <= screenCount;
            }

            return Result <= 1;
        }

        public static void Control()
        {
            if (!SSSHP.Work(SMMRA.Backgroundog) && SMMB.PerformanceCounter)
            {
                SSSHP.Run(SSSMI.Commandog, $"{SMMRG.StartCommand}{SSDECT.Backgroundog}{SMMRG.ValueSeparator}{SSSMI.Backgroundog}");
            }
        }
    }
}
