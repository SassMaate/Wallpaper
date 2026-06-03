using System.Windows;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SSEWVHH = Sucrose.Shared.Engine.WebView.Helper.Handle;
using SSEWVMI = Sucrose.Shared.Engine.WebView.Manage.Internal;
using SWNM = Skylark.Wing.Native.Methods;

namespace Sucrose.Shared.Engine.WebView.Helper
{
    internal static class Surface
    {
        public static void Correct(object sender, SizeChangedEventArgs e)
        {
            try
            {
                if (SSEWVMI.WebEngine == null || SSEMI.WindowHandle == IntPtr.Zero)
                {
                    return;
                }

                SSEWVHH.GetInputHandle();

                IntPtr BrowserHandle = SSEWVMI.WebHandle;

                if (BrowserHandle == IntPtr.Zero)
                {
                    return;
                }

                SWNM.GetWindowRect(BrowserHandle, out SWNM.RECT BrowserRect);
                SWNM.GetClientRect(SSEMI.WindowHandle, out SWNM.RECT ClientRect);

                int BrowserWidth = BrowserRect.Right - BrowserRect.Left;
                int BrowserHeight = BrowserRect.Bottom - BrowserRect.Top;

                if (BrowserWidth < ClientRect.Right || BrowserHeight < ClientRect.Bottom)
                {
                    SWNM.MoveWindow(BrowserHandle, 0, 0, ClientRect.Right, ClientRect.Bottom, true);
                }
            }
            catch
            {
                //
            }
        }
    }
}