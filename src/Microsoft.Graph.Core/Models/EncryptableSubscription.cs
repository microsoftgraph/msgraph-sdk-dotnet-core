using System;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Graph
{
    public abstract class EncryptableSubscription : IEncryptableSubscription
    {
        public abstract string EncryptionCertificate { get; set; }

        /// <summary>
        /// Adds the encryption certificate information for change notifications with resource data to the subscription creation information.
        /// </summary>
        /// <param name="certificate">Certificate to use for encryption</param>
        public void AddEncryptionCertificate(X509Certificate2 certificate)
        {
            EncryptionCertificate = Convert.ToBase64String(certificate.Export(X509ContentType.SerializedCert));
        }
    }
}
