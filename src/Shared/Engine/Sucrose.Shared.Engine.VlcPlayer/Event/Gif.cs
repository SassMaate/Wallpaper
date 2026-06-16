using System.IO;
using System.Windows;
using Application = System.Windows.Application;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using SSEHD = Sucrose.Shared.Engine.Helper.Data;
using SSEHP = Sucrose.Shared.Engine.Helper.Properties;
using SSEMI = Sucrose.Shared.Engine.Manage.Internal;
using SSEVPHP = Sucrose.Shared.Engine.VlcPlayer.Helper.Properties;
using SSEVPMI = Sucrose.Shared.Engine.VlcPlayer.Manage.Internal;
using SSTHP = Sucrose.Shared.Theme.Helper.Properties;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;

namespace Sucrose.Shared.Engine.VlcPlayer.Event
{
    internal static class Gif
    {
        public static void MediaEngineEndReached(object sender, EventArgs e)
        {
            if (SSEHD.GetLoop())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ThreadPool.QueueUserWorkItem(_ => SSEVPMI.MediaEngine.Play(SSEVPMI.MediaBase));
                });
            }
        }

        public static void MediaViewLoaded(object sender, RoutedEventArgs e)
        {
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

                SSEHP.ExecuteNormal(SSEVPHP.ExecuteScript);
            }

            SSEVPMI.MediaView.MediaPlayer = SSEVPMI.MediaEngine;

            SSEVPMI.MediaView.VerticalAlignment = VerticalAlignment.Stretch;
            SSEVPMI.MediaView.HorizontalAlignment = HorizontalAlignment.Stretch;

            SSEVPMI.MediaEngine.Play(SSEVPMI.MediaBase);
        }

        private static async void PropertiesWatcher(object sender, FileSystemEventArgs e)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    SSEMI.Properties = SSTHP.ReadJson(e.FullPath);

                    if (SSEVPMI.MediaEngine != null)
                    {
                        SSEHP.ExecuteNormal(SSEVPHP.ExecuteScript);
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