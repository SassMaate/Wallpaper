using Microsoft.Web.WebView2.Core;
using System.Collections;
using System.IO;
using Application = System.Windows.Application;
using SELLT = Skylark.Enum.LevelLogType;
using SMME = Sucrose.Manager.Manage.Engine;
using SMMG = Sucrose.Manager.Manage.General;
using SMMI = Sucrose.Manager.Manage.Internal;
using SSEHP = Sucrose.Shared.Engine.Helper.Properties;
using SSEHS = Sucrose.Shared.Engine.Helper.Source;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SSEWVHG = Sucrose.Shared.Engine.WebView.Helper.Gif;
using SSEWVMI = Sucrose.Shared.Engine.WebView.Manage.Internal;
using SSTHP = Sucrose.Shared.Theme.Helper.Properties;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;
using SSWHD = Sucrose.Shared.Watchdog.Helper.Dataset;

namespace Sucrose.Shared.Engine.WebView.Event
{
    internal static class Gif
    {
        private static async void PropertiesWatcher(object sender, FileSystemEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    SSEMI.Properties = SSTHP.ReadJson(e.FullPath);

                    if (SSEWVMI.WebEngine.IsInitialized)
                    {
                        SSEHP.ExecuteTask(SSEWVMI.WebEngine.CoreWebView2.ExecuteScriptAsync);
                    }
                }
                catch (Exception Exception)
                {
                    await SSWEW.Watch_CatchException(Exception);
                }
            });
        }

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

        public static void WebEngineDOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            SSEWVHG.Load();

            SSEMI.Initialized = true;

            if (!string.IsNullOrEmpty(SSEMI.PropertiesFile))
            {
                SSEMI.Properties = SSTHP.ReadJson(SSEMI.PropertiesFile);
                SSEMI.Properties.State = true;
            }

            if (SSEMI.Properties.State)
            {
                if (SSEMI.PropertiesWatcher)
                {
                    SSEHP.CreatedEventHandler += PropertiesWatcher;
                }

                SSEHP.StartWatcher();

                SSEHP.ExecuteTask(SSEWVMI.WebEngine.CoreWebView2.ExecuteScriptAsync);
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

            Uri Gif = SSEHS.GetSource(SSEMI.Info.Source, SSEMI.Host);

            string Path = SSEHS.GetGifContentPath();

            SSEHS.WriteGifContent(Path, Gif);

            SSEWVMI.WebEngine.Source = SSEHS.GetSource(Path);

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