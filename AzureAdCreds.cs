using System;
using System.Security.Cryptography.X509Certificates;

namespace KeyVault.PnP.Job
{
    public class AzureAdCreds
    {
        public const string SectionName = "AzureAdApp";

        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string CertificateKey { get; set; }

        public X509Certificate2 Certificate => new X509Certificate2(Convert.FromBase64String(CertificateKey));
    }
}
