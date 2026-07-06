using System.Net;

namespace Sucrose.Shared.Space.Helper
{
    internal static class Security
    {
        public static void Apply()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13 | SecurityProtocolType.Ssl3;
            }
            catch
            {
                try
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                }
                catch
                {
                    //ServicePointManager.SecurityProtocol = (SecurityProtocolType)0 | (SecurityProtocolType)3072 | (SecurityProtocolType)12288;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
                }
            }
        }
    }
}