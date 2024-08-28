// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// The IEncryptableSubscription interface
    /// </summary>
    public interface IEncryptableSubscription
    {
        /// <summary>
        /// The encryption certificate
        /// </summary>
        string EncryptionCertificate
        {
            get; set;
        }
    }
}
