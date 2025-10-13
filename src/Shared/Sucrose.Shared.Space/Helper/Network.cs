using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using SMMRU = Sucrose.Memory.Manage.Readonly.Url;
using SSDSH = Sucrose.Shared.Dependency.Struct.Host;

namespace Sucrose.Shared.Space.Helper
{
    internal static class Network
    {
        public static bool GetHostEntry()
        {
            try
            {
                _ = Dns.GetHostEntry(SMMRU.Soferity.Replace("https://", ""));

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> GetHostEntryAsync()
        {
            try
            {
                _ = await Dns.GetHostEntryAsync(SMMRU.Soferity.Replace("https://", ""));

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static List<SSDSH> GetHost()
        {
            return new()
            {
                new()
                {
                    Name = "Bing",
                    Address = "www.bing.com"
                },
                new()
                {
                    Name = "Baidu",
                    Address = "www.baidu.com"
                },
                new()
                {
                    Name = "Yahoo",
                    Address = "www.yahoo.com"
                },
                new()
                {
                    Name = "Google",
                    Address = "www.google.com"
                },
                new()
                {
                    Name = "Yandex",
                    Address = "www.yandex.com"
                },
                new()
                {
                    Name = "DuckDuckGo",
                    Address = "www.duckduckgo.com"
                }
            };
        }

        public static bool IsInternetAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }

        public static string[] InstanceNetworkInterfaces()
        {
            PerformanceCounterCategory Category = new("Network Interface");

            return Category.GetInstanceNames();
        }

        public static NetworkInterface[] AllNetworkInterfaces()
        {
            return NetworkInterface.GetAllNetworkInterfaces();
        }

        public static IPAddress[] GetHostAddresses(string Host)
        {
            try
            {
                return Dns.GetHostAddresses(Host);
            }
            catch
            {
                return Array.Empty<IPAddress>();
            }
        }
    }
}