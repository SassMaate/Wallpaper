using Microsoft.Web.WebView2.Core;
using System.Collections;
using SEIT = Skylark.Enum.InputType;
using SELLT = Skylark.Enum.LevelLogType;
using SMME = Sucrose.Manager.Manage.Engine;
using SMMG = Sucrose.Manager.Manage.General;
using SMMI = Sucrose.Manager.Manage.Internal;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SSEWVEI = Sucrose.Shared.Engine.WebView.Extension.Interaction;
using SSEWVHH = Sucrose.Shared.Engine.WebView.Helper.Handle;
using SSEWVHM = Sucrose.Shared.Engine.WebView.Helper.Management;
using SSEWVMI = Sucrose.Shared.Engine.WebView.Manage.Internal;
using SSWHD = Sucrose.Shared.Watchdog.Helper.Dataset;

namespace Sucrose.Shared.Engine.WebView.Event
{
    internal static class Url
    {
        public static void WebEngineProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs e)
        {
            SSWHD.Add("WebEngine Process Failed", new Hashtable()
            {
                { "Exit Code", e.ExitCode },
                { "Reason", $"{e.Reason}" },
                { "Process Description", e.ProcessDescription },
                { "Process Failed Kind", $"{e.ProcessFailedKind}" },
                { "Failure Source Module Path", e.FailureSourceModulePath },
                { "Frame Infos For Failed Process", e.FrameInfosForFailedProcess }
            });

            SMMI.WebViewLiveLogManager.Log(SELLT.Fatal, $"Reason: {e.Reason}");
            SMMI.WebViewLiveLogManager.Log(SELLT.Fatal, $"Exit Code: {e.ExitCode}");
            SMMI.WebViewLiveLogManager.Log(SELLT.Fatal, $"Process Failed Kind: {e.ProcessFailedKind}");
            SMMI.WebViewLiveLogManager.Log(SELLT.Fatal, $"Process Description: {e.ProcessDescription}");
            SMMI.WebViewLiveLogManager.Log(SELLT.Fatal, $"Failure Source Module Path: {e.FailureSourceModulePath}");

            if (e.FrameInfosForFailedProcess != null && e.FrameInfosForFailedProcess.Any())
            {
                foreach (CoreWebView2FrameInfo FrameInfo in e.FrameInfosForFailedProcess)
                {
                    SMMI.WebViewLiveLogManager.Log(SELLT.Fatal, $"Failed Process; Frame ID: {FrameInfo.FrameId}, Frame Kind: {FrameInfo.FrameKind}, Name: {FrameInfo.Name}, Source: {FrameInfo.Source}");
                }
            }
        }

        public static void WebEngineContentLoading(object sender, CoreWebView2ContentLoadingEventArgs e)
        {
            SSEWVHM.SetProcesses();
        }

        public static void WebEngineDOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            SSEWVHM.SetProcesses();

            SSEMI.Initialized = true;

            SSEWVHH.GetInputHandle();

            SSEWVHH.GetIntermediateHandle();

            if (SMME.InputType != SEIT.Close)
            {
                SSEWVEI.Register();
            }
        }

        private static void WebEngineDownloadStarting(object sender, CoreWebView2DownloadStartingEventArgs e)
        {
            e.Cancel = true;
        }

        public static void WebEngineInitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            SSEWVMI.WebEngine.CoreWebView2.ServerCertificateErrorDetected += WebEngineServerCertificateErrorDetected;

            SSEWVMI.WebEngine.CoreWebView2.DownloadStarting += WebEngineDownloadStarting;

            SSEWVMI.WebEngine.CoreWebView2.ProcessFailed += WebEngineProcessFailed;

            SSEWVMI.WebEngine.CoreWebView2.Settings.UserAgent = SMMG.UserAgent;

            SSEWVMI.WebEngine.Source = new(SSEMI.Info.Source);

            SSEWVMI.WebEngine.CoreWebView2.ContentLoading += WebEngineContentLoading;
            SSEWVMI.WebEngine.CoreWebView2.DOMContentLoaded += WebEngineDOMContentLoaded;

            if (SMME.DeveloperMode)
            {
                SSEWVMI.WebEngine.CoreWebView2.OpenDevToolsWindow();

                SSEWVMI.WebEngine.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                SSEWVMI.WebEngine.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;
            }
            else
            {
                SSEWVMI.WebEngine.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                SSEWVMI.WebEngine.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
            }
        }

        public static void WebEngineServerCertificateErrorDetected(object sender, CoreWebView2ServerCertificateErrorDetectedEventArgs e)
        {
            CoreWebView2Certificate Certificate = e.ServerCertificate;

            e.Action = CoreWebView2ServerCertificateErrorAction.AlwaysAllow;
        }
    }
}