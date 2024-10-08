﻿// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Kiota.Abstractions.Serialization;

    public class TestChangeNotificationEncryptedContent : IDecryptableContent, IParsable, IAdditionalDataHolder
    {
        /// <summary>
        /// Gets or sets data.
        /// Base64-encoded encrypted data that produces a full resource respresented as JSON. The data has been encrypted with the provided dataKey using an AES/CBC/PKCS5PADDING cipher suite.
        /// </summary>
        public string Data
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets dataKey.
        /// Base64-encoded symmetric key generated by Microsoft Graph to encrypt the data value and to generate the data signature. This key is encrypted with the certificate public key that was provided during the subscription. It must be decrypted with the certificate private key before it can be used to decrypt the data or verify the signature. This key has been encrypted with the following cipher suite: RSA/ECB/OAEPWithSHA1AndMGF1Padding.
        /// </summary>
        public string DataKey
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets dataSignature.
        /// Base64-encoded HMAC-SHA256 hash of the data for validation purposes.
        /// </summary>
        public string DataSignature
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets encryptionCertificateId.
        /// ID of the certificate used to encrypt the dataKey.
        /// </summary>
        public string EncryptionCertificateId
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets encryptionCertificateThumbprint.
        /// Hexadecimal representation of the thumbprint of the certificate used to encrypt the dataKey.
        /// </summary>
        public string EncryptionCertificateThumbprint
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets @odata.type.
        /// </summary>
        public string ODataType
        {
            get; set;
        }

        /// <summary>
        /// Gets the field deserializers for the <see cref="TestChangeNotificationEncryptedContent"/> instance
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                {"data", (n) => { Data = n.GetStringValue(); } },
                {"dataKey", (n) => { DataKey = n.GetStringValue(); } },
                {"dataSignature", (n) => { DataSignature = n.GetStringValue(); } },
                {"encryptionCertificateId", (n) => { EncryptionCertificateId = n.GetStringValue(); } },
                {"encryptionCertificateThumbprint", (n) => { EncryptionCertificateThumbprint = n.GetStringValue(); } },
            };
        }

        /// <summary>
        /// Serialize the <see cref="TestChangeNotificationEncryptedContent"/> instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to serialize the instance</param>
        /// <exception cref="ArgumentNullException">Thrown when the writer is null</exception>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("data", Data);
            writer.WriteStringValue("dataKey", DataKey);
            writer.WriteStringValue("dataSignature", DataSignature);
            writer.WriteStringValue("encryptionCertificateId", EncryptionCertificateId);
            writer.WriteStringValue("encryptionCertificateThumbprint", EncryptionCertificateThumbprint);
            writer.WriteAdditionalData(AdditionalData);
        }
    }
}
