using System.IO;
using SWHFL = Skylark.Wing.Helper.FileLocksmith;

namespace Sucrose.Shared.Space.Helper
{
    internal static class Lock
    {
        public static bool File(string filePath)
        {
            try
            {
                return !SWHFL.IsFileLocked(filePath);
            }
            catch
            {
                return false;
            }
        }

        public static bool Directory(string directoryPath)
        {
            try
            {
                string[] Files = System.IO.Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);

                foreach (string Record in Files)
                {
                    if (!File(Record))
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}