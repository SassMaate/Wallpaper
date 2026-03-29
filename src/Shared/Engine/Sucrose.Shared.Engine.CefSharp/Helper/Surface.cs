using SSECSMI = Sucrose.Shared.Engine.CefSharp.Manage.Internal;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SWNM = Skylark.Wing.Native.Methods;

namespace Sucrose.Shared.Engine.CefSharp.Helper
{
    internal static class Surface
    {
        private static bool Registered;

        public static void Start()
        {
            if (!Registered)
            {
                Registered = true;

                SSECSMI.CefEngine.SizeChanged += (s, e) => Correct();
            }

            Correct();
        }

        private static void Correct()
        {
            try
            {
                if (SSECSMI.CefHost == null || SSEMI.WindowHandle == IntPtr.Zero)
                {
                    return;
                }

                IntPtr BrowserHandle = SSECSMI.CefHost.GetWindowHandle();

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