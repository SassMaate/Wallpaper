using System.Diagnostics;
using SMMRP = Sucrose.Memory.Manage.Readonly.Process;

namespace Sucrose.Shared.Space.Helper
{
    internal static class Power
    {
        // Chromium-based engines (CefSharp, WebView2) acquire a process-wide display+system
        // PowerRequest while a <video> element plays (blink::VideoWakeLock -> PowerSetRequest with
        // PowerRequestDisplayRequired). That is a different OS mechanism than the per-thread
        // SetThreadExecutionState used by the Awakening helper, so the wallpaper's StayAwake intent
        // cannot revoke it and the screen stays on even when StayAwake is off. There is no Chromium
        // switch that disables it; "powercfg /requestsoverride" telling Windows to ignore those power
        // requests for the named processes is the only way to make StayAwake authoritative again.
        //
        //   Ignore == true  -> install the override  (StayAwake OFF: allow the display/system to sleep)
        //   Ignore == false -> remove the override   (StayAwake ON: let the browser wake locks stand)
        //
        // The override is written to HKLM, so it requires elevation (one UAC prompt) and is persistent
        // across reboots; it is keyed by process name only, so the WebView2 entry affects every
        // WebView2 host on the machine. It should be cleared on uninstall.
        public static void OverrideBrowserWakeLock(bool Ignore)
        {
            string Requests = Ignore ? "DISPLAY SYSTEM" : string.Empty;

            // Chain both subprocess overrides into a single elevated call so only one UAC prompt appears.
            string Command = string.Format
            (
                "/c powercfg /requestsoverride PROCESS \"{0}\" {2} & powercfg /requestsoverride PROCESS \"{1}\" {2}",
                SMMRP.WebViewFullName,
                SMMRP.CefSharpFullName,
                Requests
            );

            try
            {
                ProcessStartInfo ProcessInfo = new("cmd.exe", Command)
                {
                    Verb = "runas",
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                Process.Start(ProcessInfo);
            }
            catch
            {
                // Elevation was declined or is unavailable; leave the current override state unchanged.
            }
        }
    }
}