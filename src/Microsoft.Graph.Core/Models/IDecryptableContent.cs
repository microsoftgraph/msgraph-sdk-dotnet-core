// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// The IDecryptableContent interface
    /// </summary>
    public interface IDecryptableContent
    {
        /// <summary>
        /// The Data string
        /// </summary>
        string Data { get; set; }

        /// <summary>
        /// The DataKey string
        /// </summary>
        string DataKey { get; set; }

        /// <summary>
        /// The DataSignature string
        /// </summary>
        string DataSignature { get; set; }

        /// <summary>
        /// The EncryptionCertificateId string
        /// </summary>
        string EncryptionCertificateId { get; set; }

        /// <summary>
        /// The EncryptionCertificateThumbprint string
        /// </summary>
        string EncryptionCertificateThumbprint { get; set; }
    }
}
