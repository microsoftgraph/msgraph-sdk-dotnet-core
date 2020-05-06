using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Graph
{
    public interface IEncryptableSubscription
    {
        string EncryptionCertificate { get; set; }

        void AddEncryptionCertificate(X509Certificate2 certificate);
    }
}