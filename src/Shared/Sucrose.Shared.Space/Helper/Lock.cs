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
    }
}