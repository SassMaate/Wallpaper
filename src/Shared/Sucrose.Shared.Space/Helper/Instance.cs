using SEDST = Skylark.Enum.DisplayScreenType;
using SMME = Sucrose.Manager.Manage.Engine;
using SSSHP = Sucrose.Shared.Space.Helper.Processor;
using SWUSI = Skylark.Wing.Utility.SingleInstance;
using SWUS = Skylark.Wing.Utility.Screene;

namespace Sucrose.Shared.Space.Helper
{
    internal static class Instance
    {
        private static Mutex _Mutex = null;

        public static bool Basic(string Name, string Application)
        {
            try
            {
                // In SameDuplicate mode, allow multiple Live engine instances
                if (SMME.DisplayScreenType == SEDST.SameDuplicate)
                {
                    SWUS.Initialize();

                    int screenCount = SWUS.Screens.Count();

                    // Use a unique Mutex name per instance so each process can acquire its own
                    string uniqueName = $"{Name}-{Guid.NewGuid()}";

                    _Mutex = new Mutex(true, uniqueName, out bool createdNew);

                    return createdNew && SSSHP.WorkCount(Application) <= screenCount;
                }

                _Mutex = new Mutex(true, Name, out bool created);

                return created && SSSHP.WorkCount(Application) <= 1;
            }
            catch
            {
                try
                {
                    _Mutex = new Mutex(true, Name, out bool createdNew);

                    return createdNew && SSSHP.WorkCount(Application) <= 1;
                }
                catch
                {
                    try
                    {
                        _Mutex = new Mutex(true, Name, out bool createdNew);

                        return createdNew && SSSHP.WorkCount(Application) <= 1;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
        }

        public static bool Single(string Name, string Application)
        {
            return SWUSI.IsAppMutexRunning(Name) && SSSHP.WorkCount(Application) <= 1;
        }
    }
}
