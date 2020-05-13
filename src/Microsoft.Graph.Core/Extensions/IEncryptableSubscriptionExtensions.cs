﻿using System;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Graph
{
    public static class IEncryptableSubscriptionExtensions
    {
        /// <summary>
        /// Adds the public encryption certificate information for change notifications with resource data to the subscription creation information.
        /// </summary>
        /// <param name="certificate">Certificate to use for encryption</param>
        public static void AddPublicEncryptionCertificate(this IEncryptableSubscription subscription, X509Certificate2 certificate)
        {
            subscription.EncryptionCertificate = Convert.ToBase64String(certificate.Export(X509ContentType.SerializedCert));
        }
    }
}