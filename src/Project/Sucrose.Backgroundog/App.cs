using System.Globalization;
using System.Text;
using SBMI = Sucrose.Backgroundog.Manage.Internal;
using SHC = Skylark.Helper.Culture;
using SMMG = Sucrose.Manager.Manage.General;
using SMMRA = Sucrose.Memory.Manage.Readonly.App;
using SMMRM = Sucrose.Memory.Manage.Readonly.Mutex;
using SSDHG = Sucrose.Shared.Dependency.Helper.Graphic;
using SSDHR = Sucrose.Shared.Dependency.Helper.Runtime;
using SSSHI = Sucrose.Shared.Space.Helper.Instance;
using SSSHS = Sucrose.Shared.Space.Helper.Security;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Backgroundog
{
    internal class App : IDisposable
    {
        public static async Task Main()
        {
            // Add global exception handlers to catch unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            try
            {
                SSDHG.Configure();

                SSDHR.Configure();

                Console.InputEncoding = Encoding.UTF8;
                Console.OutputEncoding = Encoding.UTF8;

                SHC.All = new CultureInfo(SMMG.Culture, true);

                if (SSSHI.Basic(SMMRM.Backgroundog, SMMRA.Backgroundog))
                {
                    SSSHS.Apply();

                    SBMI.Initialize.Start();

                    do
                    {
                        try
                        {
                            SBMI.Initialize.Dispose();

                            await Task.Delay(SBMI.AppTime);
                        }
                        catch (Exception Exception)
                        {
                            // Log and continue - don't let individual loop iterations crash the app
                            await SSWEW.Watch_CatchException(Exception);
                        }
                    } while (SBMI.Exit);

                    await SBMI.Initialize.Stop();
                }
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

        private static async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is Exception exception)
                {
                    await SSWEW.Watch_CatchException(exception);
                }
            }
            catch
            {
                // Last resort - don't let exception handler throw
            }
        }

        private static async void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                e.SetObserved();
                if (e.Exception != null)
                {
                    await SSWEW.Watch_CatchException(e.Exception);
                }
            }
            catch
            {
                // Last resort - don't let exception handler throw
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