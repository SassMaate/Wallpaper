using CefSharp;

namespace Sucrose.Shared.Engine.CefSharp.Handler
{
    internal class CustomKeyboard : IKeyboardHandler
    {
        public bool OnPreKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey, ref bool isKeyboardShortcut)
        {
            if (type == KeyType.RawKeyDown)
            {
                if ((modifiers.HasFlag(CefEventFlags.ControlDown) && windowsKeyCode == (int)Keys.F) || windowsKeyCode == (int)Keys.F3)
                {
                    return true;
                }

                if ((modifiers.HasFlag(CefEventFlags.ControlDown) && windowsKeyCode == (int)Keys.P) || windowsKeyCode == (int)Keys.PrintScreen)
                {
                    return true;
                }

                if ((modifiers.HasFlag(CefEventFlags.ControlDown) && windowsKeyCode == (int)Keys.R) || windowsKeyCode == (int)Keys.F5)
                {
                    return true;
                }

                if (modifiers.HasFlag(CefEventFlags.ControlDown) && (windowsKeyCode == (int)Keys.Oemplus || windowsKeyCode == (int)Keys.OemMinus))
                {
                    return true;
                }

                if ((modifiers.HasFlag(CefEventFlags.ControlDown | CefEventFlags.ShiftDown) && windowsKeyCode == (int)Keys.C) || windowsKeyCode == (int)Keys.F12)
                {
                    return true;
                }

                if (windowsKeyCode is ((int)Keys.BrowserBack) or ((int)Keys.BrowserForward) or ((int)Keys.BrowserSearch))
                {
                    return true;
                }
            }

            return false;
        }

        public bool OnKeyEvent(IWebBrowser chromiumWebBrowser, IBrowser browser, KeyType type, int windowsKeyCode, int nativeKeyCode, CefEventFlags modifiers, bool isSystemKey)
        {
            return false;
        }
    }
}