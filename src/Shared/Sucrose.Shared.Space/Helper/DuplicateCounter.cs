using System.IO;
using SMMRG = Sucrose.Memory.Manage.Readonly.General;
using SMMRP = Sucrose.Memory.Manage.Readonly.Path;

namespace Sucrose.Shared.Space.Helper
{
    /// <summary>
    /// Manages an atomic counter file to assign sequential screen indices
    /// to multiple wallpaper engine processes in SameDuplicate mode.
    /// </summary>
    internal static class DuplicateCounter
    {
        private static readonly string CounterDir = Path.Combine(SMMRP.ApplicationData, SMMRG.AppName);
        private static readonly string CounterFile = Path.Combine(CounterDir, "DuplicateScreenCounter.tmp");
        private const string MutexName = "Global\\SucroseDuplicateScreenCounter";

        /// <summary>
        /// Resets the counter to 0. Called by Run.Start() before launching engines.
        /// </summary>
        public static void Reset()
        {
            try
            {
                Directory.CreateDirectory(CounterDir);
                File.WriteAllText(CounterFile, "0");
            }
            catch { }
        }

        /// <summary>
        /// Atomically reads the current counter value and increments it.
        /// Returns the screen index this process should use.
        /// </summary>
        public static int ClaimNextScreenIndex()
        {
            using Mutex mutex = new(false, MutexName);

            try
            {
                mutex.WaitOne(5000);

                int index = 0;

                if (File.Exists(CounterFile))
                {
                    string content = File.ReadAllText(CounterFile).Trim();

                    if (int.TryParse(content, out int parsed))
                    {
                        index = parsed;
                    }
                }

                File.WriteAllText(CounterFile, (index + 1).ToString());

                return index;
            }
            catch
            {
                return 0;
            }
            finally
            {
                try
                {
                    mutex.ReleaseMutex();
                }
                catch { }
            }
        }
    }
}
