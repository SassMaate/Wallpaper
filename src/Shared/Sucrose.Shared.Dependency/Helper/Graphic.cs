using System.IO;
using SSSEGP = Sucrose.Shared.Space.Extension.GraphicPreference;
using SSSMI = Sucrose.Shared.Space.Manage.Internal;

namespace Sucrose.Shared.Dependency.Helper
{
    internal static class Graphic
    {
        public static void Configure(string App = null)
        {
            if (string.IsNullOrWhiteSpace(App))
            {
                App = SSSMI.App;
            }

            if (Path.GetExtension(App) == ".exe")
            {
                SSSEGP.EnsureHighPerformance(App);
            }
        }
    }
}