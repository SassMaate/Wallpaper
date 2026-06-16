using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SSEMPMI = Sucrose.Shared.Engine.MpvPlayer.Manage.Internal;
using SWNM = Skylark.Wing.Native.Methods;

namespace Sucrose.Shared.Engine.MpvPlayer.Helper
{
    internal static class Surface
    {
        public static void Correct()
        {
            try
            {
                if (SSEMPMI.MediaEngine == null || SSEMI.WindowHandle == IntPtr.Zero)
                {
                    return;
                }

                IntPtr PlayerHandle = SSEMPMI.MediaHandle;

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