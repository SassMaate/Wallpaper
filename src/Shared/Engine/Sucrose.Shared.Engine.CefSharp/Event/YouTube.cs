using CefSharp;
using System.IO;
using System.Windows;
using Application = System.Windows.Application;
using SMME = Sucrose.Manager.Manage.Engine;
using SMMRC = Sucrose.Memory.Manage.Readonly.Content;
using SSECSHCCM = Sucrose.Shared.Engine.CefSharp.Handler.CustomContextMenu;
using SSECSHCK = Sucrose.Shared.Engine.CefSharp.Handler.CustomKeyboard;
using SSECSHYT = Sucrose.Shared.Engine.CefSharp.Helper.YouTube;
using SSECSMI = Sucrose.Shared.Engine.CefSharp.Manage.Internal;
using SSEHP = Sucrose.Shared.Engine.Helper.Properties;
using SSEHS = Sucrose.Shared.Engine.Helper.Source;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SSTHP = Sucrose.Shared.Theme.Helper.Properties;
using SSTHV = Sucrose.Shared.Theme.Helper.Various;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Shared.Engine.CefSharp.Event
{
    internal static class YouTube
    {
        public static void CefEngineLoaded(object sender, RoutedEventArgs e)
        {
            string Video = SSTHV.GetYouTubeVideoId(SSEMI.Info.Source);
            string Playlist = SSTHV.GetYouTubePlaylistId(SSEMI.Info.Source);

            string Path = SSEHS.GetYouTubeContentPath();

            SSEHS.WriteYouTubeContent(Path, Video, Playlist);

            SSECSMI.CefEngine.Address = SSEHS.GetSource(SMMRC.YouTube, SSEMI.Host).ToString();
        }

        public static void CefEngineInitializedChanged(object sender, EventArgs e)
        {
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
            SSECSHYT.Load();

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

                SSEHP.ExecuteNormal(SSECSMI.CefEngine.ExecuteScriptAsync);
            }
        }

        private static async void PropertiesWatcher(object sender, FileSystemEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    SSEMI.Properties = SSTHP.ReadJson(e.FullPath);

                    if (!SSECSMI.CefEngine.IsDisposed && SSECSMI.CefEngine.IsInitialized && SSECSMI.CefEngine.CanExecuteJavascriptInMainFrame)
                    {
                        SSEHP.ExecuteNormal(SSECSMI.CefEngine.ExecuteScriptAsync);
                    }
                }
                catch (Exception Exception)
                {
                    await SSWEW.Watch_CatchException(Exception);
                }
            });
        }
    }
}