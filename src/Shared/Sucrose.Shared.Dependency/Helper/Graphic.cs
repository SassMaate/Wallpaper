using SSSEGP = Sucrose.Shared.Space.Extension.GraphicPreference;
using System.IO;
using SSSMI = Sucrose.Shared.Space.Manage.Internal;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;

namespace Sucrose.Shared.Dependency.Helper
{
    internal static class Graphic
    {
        public static void Configure(string App = null)
        {
            if (string.IsNullOrWhiteSpace(App))
            {
                App = SSSMI.App;

                if (App.Contains(Path.Combine(SMMRP.LocalApplicationData, SMMRG.AppName)) && App.Contains(SSSMI.Folder) && Path.GetExtension(App) == ".exe")
                {
                    SSSEGP.EnsureHighPerformance(App);
                }
            }
            else
            {
                if (Path.GetExtension(App) == ".exe")
                {
                    SSSEGP.EnsureHighPerformance(App);
                }
            }
        }
    }
}