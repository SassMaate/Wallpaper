using Microsoft.Win32;
using SSDEHPT = Sucrose.Shared.Dependency.Enum.HighPerformanceType;
using SSDMME = Sucrose.Shared.Dependency.Manage.Manager.Engine;

namespace Sucrose.Shared.Space.Extension
{
    /// <summary>
    /// Manages Windows GPU preference settings for applications via DirectX UserGpuPreferences registry.
    /// Handles semicolon-delimited registry values and preserves existing settings while updating GPU preferences.
    /// </summary>
    internal static class GraphicPreference
    {
        #region Constants

        /// <summary>
        /// Delimiter used to separate multiple settings in registry values.
        /// </summary>
        private const char SettingDelimiter = ';';

        /// <summary>
        /// Key-value separator used in registry setting pairs.
        /// </summary>
        private const char KeyValueSeparator = '=';

        /// <summary>
        /// GPU preference setting key name used in registry values.
        /// </summary>
        private const string GpuPreferenceKey = "GpuPreference";

        /// <summary>
        /// Registry path for Windows DirectX GPU preferences.
        /// Location: HKEY_CURRENT_USER\Software\Microsoft\DirectX\UserGpuPreferences
        /// </summary>
        private const string RegistryPath = @"Software\Microsoft\DirectX\UserGpuPreferences";

        #endregion

        #region Public Methods

        /// <summary>
        /// Ensures the specified application has the GPU preference configured according to SSDMME.GraphicPreference.
        /// Preserves any existing registry settings while updating only the GpuPreference value.
        /// </summary>
        /// <param name="applicationPath">Full path to the application executable.</param>
        /// <returns>True if the preference was successfully set or already configured; otherwise, false.</returns>
        public static bool EnsureHighPerformance(string applicationPath)
        {
            if (string.IsNullOrWhiteSpace(applicationPath))
            {
                return false;
            }

            try
            {
                SSDEHPT preferenceType = SSDMME.GraphicPreference;

                if (IsPreferenceSet(applicationPath, preferenceType))
                {
                    return true;
                }

                return SetPreference(applicationPath, preferenceType);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves the current GPU preference setting for the specified application.
        /// </summary>
        /// <param name="applicationPath">Full path to the application executable.</param>
        /// <returns>The current GPU preference type, or Default if not set or on error.</returns>
        public static SSDEHPT GetPreference(string applicationPath)
        {
            if (string.IsNullOrWhiteSpace(applicationPath))
            {
                return SSDEHPT.Default;
            }

            try
            {
                using RegistryKey key = OpenRegistryKey(false);
                if (key == null)
                {
                    return SSDEHPT.Default;
                }

                string registryValue = GetRegistryValue(key, applicationPath);
                if (string.IsNullOrWhiteSpace(registryValue))
                {
                    return SSDEHPT.Default;
                }

                Dictionary<string, string> settings = ParseRegistryValue(registryValue);

                if (settings.TryGetValue(GpuPreferenceKey, out string preferenceValue) && int.TryParse(preferenceValue, out int preferenceInt))
                {
                    if (Enum.IsDefined(typeof(SSDEHPT), preferenceInt))
                    {
                        return (SSDEHPT)preferenceInt;
                    }
                }

                return SSDEHPT.Default;
            }
            catch
            {
                return SSDEHPT.Default;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Opens the DirectX GPU preferences registry key.
        /// </summary>
        /// <param name="writable">True to open the key with write access; false for read-only.</param>
        /// <returns>The opened registry key, or null if the operation fails.</returns>
        private static RegistryKey OpenRegistryKey(bool writable)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable);

                // If key doesn't exist and we need write access, create it
                if (key == null && writable)
                {
                    key = Registry.CurrentUser.CreateSubKey(RegistryPath, writable);
                }

                return key;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Builds a semicolon-delimited registry value from a dictionary of settings.
        /// Ensures proper formatting with trailing semicolon per Windows registry convention.
        /// </summary>
        private static string BuildRegistryValue(Dictionary<string, string> settings)
        {
            if (settings == null || settings.Count == 0)
            {
                return string.Empty;
            }

            IEnumerable<string> settingPairs = settings
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key))
                .Select(kvp => $"{kvp.Key}{KeyValueSeparator}{kvp.Value}");

            string result = string.Join(SettingDelimiter.ToString(), settingPairs);

            // Ensure trailing semicolon (Windows registry format convention)
            if (!string.IsNullOrWhiteSpace(result) && !result.EndsWith(SettingDelimiter.ToString()))
            {
                result += SettingDelimiter;
            }

            return result;
        }

        /// <summary>
        /// Retrieves a registry value for the specified application.
        /// </summary>
        private static string GetRegistryValue(RegistryKey key, string applicationPath)
        {
            try
            {
                return key?.GetValue(applicationPath) as string ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Sets the GPU preference for the specified application while preserving other registry settings.
        /// </summary>
        private static bool SetPreference(string applicationPath, SSDEHPT preferenceType)
        {
            try
            {
                using RegistryKey key = OpenRegistryKey(true);
                if (key == null)
                {
                    return false;
                }

                string existingValue = GetRegistryValue(key, applicationPath);
                Dictionary<string, string> settings = ParseRegistryValue(existingValue);

                // Update or add GPU preference
                settings[GpuPreferenceKey] = ((int)preferenceType).ToString();

                string newValue = BuildRegistryValue(settings);
                key.SetValue(applicationPath, newValue, RegistryValueKind.String);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Parses a semicolon-delimited registry value into a dictionary of key-value pairs.
        /// Format: "Key1=Value1;Key2=Value2;Key3=Value3;"
        /// </summary>
        private static Dictionary<string, string> ParseRegistryValue(string registryValue)
        {
            Dictionary<string, string> settings = new(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(registryValue))
            {
                return settings;
            }

            string[] parts = registryValue.Split(new[] { SettingDelimiter }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                string trimmedPart = part.Trim();
                if (string.IsNullOrWhiteSpace(trimmedPart))
                {
                    continue;
                }

                int separatorIndex = trimmedPart.IndexOf(KeyValueSeparator);
                if (separatorIndex > 0 && separatorIndex < trimmedPart.Length - 1)
                {
                    string key = trimmedPart[..separatorIndex].Trim();
                    string value = trimmedPart[(separatorIndex + 1)..].Trim();

                    if (!string.IsNullOrWhiteSpace(key))
                    {
                        settings[key] = value;
                    }
                }
            }

            return settings;
        }

        /// <summary>
        /// Checks if the specified GPU preference is already set for the application.
        /// </summary>
        private static bool IsPreferenceSet(string applicationPath, SSDEHPT preferenceType)
        {
            try
            {
                return GetPreference(applicationPath) == preferenceType;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}