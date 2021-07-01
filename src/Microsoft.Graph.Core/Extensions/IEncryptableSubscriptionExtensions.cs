// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Contains extension methods for <see cref="IEncryptableSubscription"/>
    /// </summary>
    public static class IEncryptableSubscriptionExtensions
    {
        /// <summary>
        /// Adds the public encryption certificate information for change notifications with resource data to the subscription creation information.
        /// </summary>
        /// <param name="subscription">The subscription instance of type <see cref="IEncryptableSubscription"/></param>
        /// <param name="certificate">Certificate to use for encryption</param>
        public static void AddPublicEncryptionCertificate(this IEncryptableSubscription subscription, X509Certificate2 certificate)
        {
            subscription.EncryptionCertificate = Convert.ToBase64String(certificate.Export(X509ContentType.Cert));
        }
    }
}
