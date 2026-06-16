using SSEVPMI = Sucrose.Shared.Engine.VlcPlayer.Manage.Internal;

namespace Sucrose.Shared.Engine.VlcPlayer.Helper
{
    internal static class Aspect
    {
        public static string? GetRatio()
        {
            try
            {
                return SSEVPMI.MediaView.ActualWidth > 0 && SSEVPMI.MediaView.ActualHeight > 0 ? $"{SSEVPMI.MediaView.ActualWidth}:{SSEVPMI.MediaView.ActualHeight}" : null;
            }
            catch
            {
                return null;
            }
        }
    }
}