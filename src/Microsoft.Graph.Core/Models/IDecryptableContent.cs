using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Graph
{
    public interface IDecryptableContent
    {
        string Data { get; set; }
        string DataKey { get; set; }
        string DataSignature { get; set; }
        string EncryptionCertificateId { get; set; }
        string EncryptionCertificateThumbprint { get; set; }

        /// <summary>
        /// Validates the signature and decrypted content attached with the notification.
        /// </summary>
        /// <param name="certificateProvider">Certificate provider to decrypt the content. The first parameter is the certificate ID provided when creating the subscription. The second is the certificate thumbprint. The certificate WILL be disposed at the end of decryption.</param>
        /// <returns>Decrypted content as string.</returns>
        Task<string> Decrypt(Func<string, string, Task<X509Certificate2>> certificateProvider);
        /// <summary>
        /// Validates the signature and decrypted content attached with the notification.
        /// </summary>
        /// <typeparam name="T">Type to deserialize the data to.</typeparam>
        /// <param name="certificateProvider">Certificate provider to decrypt the content. The first parameter is the certificate ID provided when creating the subscription. The second is the certificate thumbprint. The certificate WILL be disposed at the end of decryption.</param>
        /// <returns>Decrypted content as the provided type.</returns>
        Task<T> Decrypt<T>(Func<string, string, Task<X509Certificate2>> certificateProvider);
    }
}
