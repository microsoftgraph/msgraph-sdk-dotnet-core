using System.Text.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Graph
{
    public static class IDecryptableContentExtensions
    {
        private static readonly Lazy<int> AESInitializationVectorSize = new Lazy<int>(() =>
        {
            using (AesCryptoServiceProvider provider = new AesCryptoServiceProvider())
            {
                return provider.LegalBlockSizes[0].MinSize;
            }
        });

        /// <summary>
        /// Validates the signature and decrypted content attached with the notification.
        /// </summary>
        /// <typeparam name="T">Type to deserialize the data to.</typeparam>
        /// <param name="certificateProvider">Certificate provider to decrypt the content. The first parameter is the certificate ID provided when creating the subscription. The second is the certificate thumbprint. The certificate WILL be disposed at the end of decryption.</param>
        /// <returns>Decrypted content as the provided type.</returns>
        public static async Task<T> Decrypt<T>(this IDecryptableContent encryptedContent, Func<string, string, Task<X509Certificate2>> certificateProvider) where T : class
        {
            return JsonSerializer.Deserialize<T>(await encryptedContent.Decrypt(certificateProvider));
        }
        /// <summary>
        /// Validates the signature and decrypted content attached with the notification.
        /// https://docs.microsoft.com/en-us/graph/webhooks-with-resource-data#decrypting-resource-data-from-change-notifications 
        /// </summary>
        /// <param name="certificateProvider">Certificate provider to decrypt the content. The first parameter is the certificate ID provided when creating the subscription. The second is the certificate thumbprint. The certificate WILL be disposed at the end of decryption.</param>
        /// <returns>Decrypted content as string.</returns>
        public static async Task<string> Decrypt(this IDecryptableContent encryptedContent, Func<string, string, Task<X509Certificate2>> certificateProvider)
        {
            using (var certificate = await certificateProvider(encryptedContent.EncryptionCertificateId, encryptedContent.EncryptionCertificateThumbprint))
            using (var rsaPrivateKey = RSACertificateExtensions.GetRSAPrivateKey(certificate))
            {
                var decryptedSymetrickey = rsaPrivateKey.Decrypt(Convert.FromBase64String(encryptedContent.DataKey), RSAEncryptionPadding.OaepSHA1);
                using (var hashAlg = new HMACSHA256(decryptedSymetrickey))
                {
                    var expectedSignatureValue = Convert.ToBase64String(hashAlg.ComputeHash(Convert.FromBase64String(encryptedContent.Data)));
                    if (!string.Equals(encryptedContent.DataSignature, expectedSignatureValue))
                    {
                        throw new InvalidDataException("Signature does not match");
                    }
                    else
                    {
                        return Encoding.UTF8.GetString(AESDecrypt(Convert.FromBase64String(encryptedContent.Data), decryptedSymetrickey));
                    }
                }
            }
        }
        private static byte[] AESDecrypt(byte[] dataToDecrypt, byte[] key)
        {
            try
            {
                using (var cryptoServiceProvider = new AesCryptoServiceProvider
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                    Key = key
                })
                {
                    var numArray = new byte[AESInitializationVectorSize.Value / 8];
                    Array.Copy(key, numArray, numArray.Length);
                    cryptoServiceProvider.IV = numArray;
                    using (var memoryStream = new MemoryStream())
                    using (var cryptoStream = new CryptoStream(memoryStream, cryptoServiceProvider.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(dataToDecrypt, 0, dataToDecrypt.Length);
                        cryptoStream.FlushFinalBlock();
                        return memoryStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unexpected error occured while trying to decrypt the input", ex);
            }
        }
    }
}
