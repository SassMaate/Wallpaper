using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Windows.Media;
using SMMG = Sucrose.Manager.Manage.General;
using SMMO = Sucrose.Manager.Manage.Objectionable;
using SSDESHT = Sucrose.Shared.Dependency.Enum.StretchType;

namespace Sucrose.Shared.Dependency.Manage
{
    internal static class Internal
    {
        public static SSDESHT DefaultStretchType = SSDESHT.None;

        public static Stretch DefaultBackgroundStretch = Stretch.UniformToFill;

        public static readonly SocketsHttpHandler HttpHandler = new()
        {
            MaxConnectionsPerServer = 100,
            AutomaticDecompression = DecompressionMethods.All,
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),

            SslOptions =
            {
                EnabledSslProtocols = SslProtocols.None | SslProtocols.Tls12 | SslProtocols.Tls13,
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true,
                LocalCertificateSelectionCallback = (sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers) => null
            }
        };

        public static readonly HttpClient Client = new(HttpHandler, false)
        {
            Timeout = Timeout.InfiniteTimeSpan, //TimeSpan.FromSeconds(30)

            DefaultRequestHeaders =
            {
                { "User-Agent", SMMG.UserAgent }
            }
        };

        public static readonly HttpClient ClientGitHub = new(HttpHandler, false)
        {
            Timeout = Timeout.InfiniteTimeSpan, //TimeSpan.FromSeconds(30)

            DefaultRequestHeaders =
            {
                { "User-Agent", SMMG.UserAgent },
                { "Authorization", $"Bearer {SMMO.PersonalAccessToken}" }
            }
        };
    }
}