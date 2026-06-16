using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SSEVPMI = Sucrose.Shared.Engine.VlcPlayer.Manage.Internal;
using SWNM = Skylark.Wing.Native.Methods;

namespace Sucrose.Shared.Engine.VlcPlayer.Helper
{
    internal static class Surface
    {
        public static void Correct()
        {
            try
            {
                if (SSEVPMI.MediaEngine == null || SSEMI.WindowHandle == IntPtr.Zero)
                {
                    return;
                }

                IntPtr PlayerHandle = SSEVPMI.MediaHandle;

                if (PlayerHandle == IntPtr.Zero)
                {
                    return;
                }

                SWNM.GetWindowRect(PlayerHandle, out SWNM.RECT PlayerRect);
                SWNM.GetClientRect(SSEMI.WindowHandle, out SWNM.RECT ClientRect);

                int PlayerWidth = PlayerRect.Right - PlayerRect.Left;
                int PlayerHeight = PlayerRect.Bottom - PlayerRect.Top;

                if (PlayerWidth < ClientRect.Right || PlayerHeight < ClientRect.Bottom)
                {
                    SWNM.MoveWindow(PlayerHandle, 0, 0, ClientRect.Right, ClientRect.Bottom, true);
                }
            }
            catch
            {
                //
            }
        }
    }
}