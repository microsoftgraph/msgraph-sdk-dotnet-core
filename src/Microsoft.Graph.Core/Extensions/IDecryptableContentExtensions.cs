// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Text.Json;
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Kiota.Abstractions.Serialization;
    using Microsoft.Kiota.Serialization.Json;

    /// <summary>
    /// Contains extension methods for <see cref="IDecryptableContentExtensions"/>
    /// </summary>
    public static class IDecryptableContentExtensions
    {
        /// <summary>
        /// Validates the signature and decrypted content attached with the notification.
        /// </summary>
        /// <typeparam name="T">Type to deserialize the data to.</typeparam>
        /// <param name="encryptedContent">The encrypted content of type <see cref="IDecryptableContent"/></param>
        /// <param name="certificateProvider">Certificate provider to decrypt the content.
        /// The first parameter is the certificate ID provided when creating the subscription.
        /// The second is the certificate thumbprint. The certificate WILL be disposed at the end of decryption.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="certificateProvider"/> is null</exception>
        /// <returns>Decrypted content as the provided type.</returns>
        public static async Task<T> DecryptAsync<T>(this IDecryptableContent encryptedContent, Func<string, string, Task<X509Certificate2>> certificateProvider) where T : IParsable, new()
        {
            if (certificateProvider == null)
                throw new ArgumentNullException(nameof(certificateProvider));

            var stringContent = await encryptedContent.DecryptAsync(certificateProvider).ConfigureAwait(false);
            using var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(stringContent));
            var parseNodeFactory = ParseNodeFactoryRegistry.DefaultInstance;
            var rootNode = parseNodeFactory.GetRootParseNode(CoreConstants.MimeTypeNames.Application.Json, contentStream);
            return rootNode.GetObjectValue<T>((parsable) => new T()); ;
        }

        /// <summary>
        /// Validates the signature and decrypted content attached with the notification.
        /// https://docs.microsoft.com/en-us/graph/webhooks-with-resource-data#decrypting-resource-data-from-change-notifications 
        /// </summary>
        /// <param name="encryptedContent">The encrypted content of type <see cref="IDecryptableContent"/></param>
        /// <param name="certificateProvider">Certificate provider to decrypt the content.
        /// The first parameter is the certificate ID provided when creating the subscription.
        /// The second is the certificate thumbprint. The certificate WILL be disposed at the end of decryption.</param>
        /// <exception cref="InvalidDataException">Thrown when the <see cref="IDecryptableContent.DataSignature"/> value does not match the signature in the payload</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="certificateProvider"/> is null</exception>
        /// <exception cref="ApplicationException">Thrown when there is a failure in attempting to decrypt the information</exception>
        /// <returns>Decrypted content as string.</returns>
        public static async Task<string> DecryptAsync(this IDecryptableContent encryptedContent, Func<string, string, Task<X509Certificate2>> certificateProvider)
        {
            if (certificateProvider == null)
                throw new ArgumentNullException(nameof(certificateProvider));

            using var certificate = await certificateProvider(encryptedContent.EncryptionCertificateId, encryptedContent.EncryptionCertificateThumbprint).ConfigureAwait(false);
            using var rsaPrivateKey = certificate.GetRSAPrivateKey();
            var decryptedSymmetricKey = rsaPrivateKey.Decrypt(Convert.FromBase64String(encryptedContent.DataKey), RSAEncryptionPadding.OaepSHA1);
            using var hashAlg = new HMACSHA256(decryptedSymmetricKey);
            var expectedSignatureValue = Convert.ToBase64String(hashAlg.ComputeHash(Convert.FromBase64String(encryptedContent.Data)));
            if (!string.Equals(encryptedContent.DataSignature, expectedSignatureValue))
            {
                throw new InvalidDataException("Signature does not match");
            }
            else
            {
                return Encoding.UTF8.GetString(AesDecrypt(Convert.FromBase64String(encryptedContent.Data), decryptedSymmetricKey));
            }
        }

        private static byte[] AesDecrypt(byte[] dataToDecrypt, byte[] key)
        {
            try
            {
#pragma warning disable SYSLIB0021
                using var cryptoServiceProvider = new AesCryptoServiceProvider
#pragma warning restore SYSLIB0021
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                    Key = key
                };
                var numArray = new byte[16]; //16 is the IV size for the decryption provider required by specification
                Array.Copy(key, numArray, numArray.Length);
                cryptoServiceProvider.IV = numArray;
                using var memoryStream = new MemoryStream();
                using var cryptoStream = new CryptoStream(memoryStream, cryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Write);
                cryptoStream.Write(dataToDecrypt, 0, dataToDecrypt.Length);
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unexpected error occurred while trying to decrypt the input", ex);
            }
        }
    }
}
