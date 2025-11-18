using Microsoft.Win32;
using System.Diagnostics;
using SBMI = Sucrose.Backgroundog.Manage.Internal;
using SSWEW = Sucrose.Shared.Watchdog.Extension.Watch;
using SWNM = Skylark.Wing.Native.Methods;

namespace Sucrose.Backgroundog.Extension
{
    internal static class Windows
    {
        public static async void Stop()
        {
            try
            {
                SBMI.SubscriptionManager?.Uninitialize();

                SystemEvents.SessionSwitch -= SessionSwitch;

                SystemEvents.PowerModeChanged -= PowerModeChanged;
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        public static async void Start()
        {
            try
            {
                SBMI.WindowsLock = IsLocked();

                SBMI.WindowsListener = new();

                SBMI.SubscriptionManager = new(SBMI.WindowsListener);

                SBMI.SubscriptionManager.Initialize();

                SystemEvents.SessionSwitch += SessionSwitch;

                SystemEvents.PowerModeChanged += PowerModeChanged;
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        private static bool IsLocked()
        {
            try
            {
                IntPtr foregroundWindow = SWNM.GetForegroundWindow();
                
                // Check if the window handle is valid
                if (foregroundWindow == IntPtr.Zero)
                {
                    return false;
                }

                uint threadId = SWNM.GetWindowThreadProcessId(foregroundWindow, out int PID);
                
                // Check if we got a valid PID
                if (PID <= 0)
                {
                    return false;
                }

                Process Process = Process.GetProcessById(PID);

                return Process.ProcessName.Equals("LockApp", StringComparison.OrdinalIgnoreCase);
            }
            catch { }

            return false;
        }

        private static async void SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            try
            {
                switch (e.Reason)
                {
                    case SessionSwitchReason.SessionLock:
                        SBMI.WindowsLock = true;
                        break;
                    case SessionSwitchReason.SessionLogon:
                        SBMI.WindowsSession = true;
                        break;
                    case SessionSwitchReason.RemoteConnect:
                        SBMI.WindowsRemote = true;
                        break;
                    case SessionSwitchReason.SessionLogoff:
                        SBMI.WindowsSession = false;
                        break;
                    case SessionSwitchReason.SessionUnlock:
                        SBMI.WindowsLock = false;
                        break;
                    case SessionSwitchReason.ConsoleConnect:
                        SBMI.WindowsConsole = true;
                        break;
                    case SessionSwitchReason.RemoteDisconnect:
                        SBMI.WindowsRemote = false;
                        break;
                    case SessionSwitchReason.ConsoleDisconnect:
                        SBMI.WindowsConsole = false;
                        break;
                    case SessionSwitchReason.SessionRemoteControl:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }

        private static async void PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            try
            {
                switch (e.Mode)
                {
                    case PowerModes.Resume:
                        SBMI.WindowsSleep = false;
                        break;
                    case PowerModes.Suspend:
                        SBMI.WindowsSleep = true;
                        break;
                    case PowerModes.StatusChange:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception Exception)
            {
                await SSWEW.Watch_CatchException(Exception);
            }
        }
    }
}