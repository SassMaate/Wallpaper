using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using SSDSH = Sucrose.Shared.Dependency.Struct.Host;

namespace Sucrose.Shared.Space.Helper
{
    internal static class Network
    {
        public static bool GetHostEntry()
        {
            foreach (SSDSH Host in GetHost())
            {
                try
                {
                    _ = Dns.GetHostEntry(Host.Address);

                    return true;
                }
                catch { }
            }

            return false;
        }

        public static async Task<bool> GetHostEntryAsync()
        {
            foreach (SSDSH Host in GetHost())
            {
                try
                {
                    _ = await Dns.GetHostEntryAsync(Host.Address);

                    return true;
                }
                catch { }
            }

            return false;
        }

        public static List<SSDSH> GetHost()
        {
            return
            [
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
                    Name = "Microsoft",
                    Address = "www.microsoft.com"
                },
                new()
                {
                    Name = "Cloudflare",
                    Address = "www.cloudflare.com"
                },
                new()
                {
                    Name = "DuckDuckGo",
                    Address = "www.duckduckgo.com"
                }
            ];
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