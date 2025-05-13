using CefSharp;
using System.Windows;
using SEIT = Skylark.Enum.InputType;
using SMME = Sucrose.Manager.Manage.Engine;
using SSECSEI = Sucrose.Shared.Engine.CefSharp.Extension.Interaction;
using SSECSHCCM = Sucrose.Shared.Engine.CefSharp.Handler.CustomContextMenu;
using SSECSHCK = Sucrose.Shared.Engine.CefSharp.Handler.CustomKeyboard;
using SSECSHH = Sucrose.Shared.Engine.CefSharp.Helper.Handle;
using SSECSHM = Sucrose.Shared.Engine.CefSharp.Helper.Management;
using SSECSMI = Sucrose.Shared.Engine.CefSharp.Manage.Internal;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;

namespace Sucrose.Shared.Engine.CefSharp.Event
{
    internal static class Url
    {
        public static void CefEngineInitialized(object sender, EventArgs e)
        {
            SSECSHM.SetProcesses();
        }

        public static void CefEngineLoaded(object sender, RoutedEventArgs e)
        {
            SSECSMI.CefEngine.Address = SSEMI.Info.Source;
        }

        public static void CefEngineInitializedChanged(object sender, EventArgs e)
        {
            SSECSHH.GetInputHandle();

            SSECSHH.GetIntermediateHandle();

            if (SMME.InputType != SEIT.Close)
            {
                SSECSEI.Register();
            }

            if (SMME.DeveloperMode)
            {
                SSECSMI.CefEngine.ShowDevTools();
            }
            else
            {
                SSECSMI.CefEngine.MenuHandler = new SSECSHCCM();
                SSECSMI.CefEngine.KeyboardHandler = new SSECSHCK();
            }

            SSEMI.Initialized = SSECSMI.CefEngine.IsBrowserInitialized;
        }

        public static void CefEngineFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            SSECSHM.SetProcesses();
        }
    }
}