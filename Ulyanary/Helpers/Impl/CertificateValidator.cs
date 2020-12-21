using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Ulyanary.Config;

namespace Ulyanary.Helpers
{
    public class CertificateValidator
    {
        private ConfigData _config;
        private readonly string _trustedThumbprint;

        public CertificateValidator(ConfigData config)
        {
            _config = config;
            _trustedThumbprint = _config.RestlessFalcon.sslThumbprint;
        }

        public bool ValidateCertificate(HttpRequestMessage request, X509Certificate2 certificate, X509Chain certificateChain, SslPolicyErrors policy)
        {
            var certificate2 = new X509Certificate2(certificate);
            if (certificate2.Thumbprint != null && certificate2.Thumbprint.Equals(_trustedThumbprint, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
