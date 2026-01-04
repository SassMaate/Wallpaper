#if LAUNCHER

using Newtonsoft.Json;
using STEMREA = Sucrose.Transmission.Event.MessageReceivedEventArgs;
using STIL = Sucrose.Transmission.Interface.Launcher;
using SSLMI = Sucrose.Shared.Launcher.Manage.Internal;

namespace Sucrose.Shared.Transmission.Services
{
    public static class LauncherTransmissionService
    {
        public static void Handler(STEMREA e)
        {
            try
            {
                if (e != null && !string.IsNullOrEmpty(e.Message))
                {
                    STIL Data = JsonConvert.DeserializeObject<STIL>(e.Message);

                    if (Data.Hide)
                    {
                        SSLMI.TrayIconManager.Hide();
                    }

                    if (Data.Show)
                    {
                        SSLMI.TrayIconManager.Show();
                    }

                    if (Data.Release)
                    {
                        SSLMI.TrayIconManager.Release();
                    }

                    SSLMI.TrayIconManager.Icon(Data.Icon);
                }
            }
            catch { }
        }
    }
}

#endif