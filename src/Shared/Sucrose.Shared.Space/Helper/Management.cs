using System.Diagnostics;
using System.Management;
using System.Text;

namespace Sucrose.Shared.Space.Helper
{
    internal static class Management
    {
        public static string GetCommandLine(int PID)
        {
            try
            {
                using ManagementClass Class = new("Win32_Process");
                using ManagementObjectCollection Collection = Class.GetInstances();

                return Collection.Cast<ManagementObject>().SingleOrDefault(Object => Check(Object, "ProcessId", 0) == PID)?["CommandLine"]?.ToString()?.Trim() ?? string.Empty;
            }
            catch
            {
                return null;
            }
        }

        public static string GetCommandLine2(int PID)
        {
            try
            {
                string Command = $@"Get-CimInstance Win32_Process -Filter ""ProcessId={PID}"" | Select-Object -ExpandProperty CommandLine";

                ProcessStartInfo ProcessInfo = new()
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = "powershell.exe",
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = $"-NoProfile -Command \"{Command}\"",
                };

                using Process Process = Process.Start(ProcessInfo);

                string Result = Process.StandardOutput.ReadToEnd();

                Process.WaitForExit();

                return string.IsNullOrWhiteSpace(Result) ? string.Empty : Result?.Trim();
            }
            catch
            {
                return null;
            }
        }

        public static int Check(ManagementObject Object, string Title, int Back)
        {
            try
            {
                string Value = Object[Title]?.ToString()?.Trim();

                return int.TryParse(Value, out int Result) ? Result : Back;
            }
            catch
            {
                return Back;
            }
        }

        public static bool Check(ManagementObject Object, string Title, bool Back)
        {
            try
            {
                string Value = Object[Title]?.ToString()?.Trim()?.ToLowerInvariant();

                if (string.IsNullOrEmpty(Value))
                {
                    return Back;
                }

                if (Value is "true" or "1" or "yes" or "on")
                {
                    return true;
                }

                if (Value is "false" or "0" or "no" or "off")
                {
                    return false;
                }

                return Back;
            }
            catch
            {
                return Back;
            }
        }

        public static long Check(ManagementObject Object, string Title, long Back)
        {
            try
            {
                string Value = Object[Title]?.ToString()?.Trim();

                return long.TryParse(Value, out long Result) ? Result : Back;
            }
            catch
            {
                return Back;
            }
        }

        public static string Check(ManagementObject Object, string Title, string Back)
        {
            try
            {
                string Value = Object[Title]?.ToString()?.Trim();

                return string.IsNullOrWhiteSpace(Value) ? Back : Value;
            }
            catch
            {
                return Back;
            }
        }

        public static Version Check(ManagementObject Object, string Title, Version Back)
        {
            try
            {
                string Value = Object[Title]?.ToString()?.Trim();

                return System.Version.TryParse(Value, out Version Version) ? Version : Back;
            }
            catch
            {
                return Back;
            }
        }
    }
}