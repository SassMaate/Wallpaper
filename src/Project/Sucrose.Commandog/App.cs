using System.Globalization;
using System.Text;
using SCHA = Sucrose.Commandog.Helper.Arguments;
using SHC = Skylark.Helper.Culture;
using SMMG = Sucrose.Manager.Manage.General;
using SSDHG = Sucrose.Shared.Dependency.Helper.Graphic;
using SSDHR = Sucrose.Shared.Dependency.Helper.Runtime;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Commandog
{
    internal class App : IDisposable
    {
        public static async Task Main(string[] Args)
        {
            try
            {
                SSDHG.Configure();

                SSDHR.Configure();

                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;

                SHC.All = new CultureInfo(SMMG.Culture, true);

                await SCHA.Parse(Args);
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
            finally
            {
                Close();
            }
        }

        public static void Close()
        {
            Environment.Exit(0);
            Application.Exit();
        }

        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }
}