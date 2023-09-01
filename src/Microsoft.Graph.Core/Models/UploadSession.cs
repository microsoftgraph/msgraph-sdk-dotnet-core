// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Models
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Concrete implementation of the IUploadSession interface
    /// </summary>
    internal class UploadSession : IUploadSession
    {
        /// <summary>
        /// Expiration date of the upload session
        /// </summary>
        public DateTimeOffset? ExpirationDateTime { get; set; }

        /// <summary>
        /// The ranges yet to be uploaded to the server
        /// </summary>
        public List<string> NextExpectedRanges { get; set; }

        /// <summary>
        /// The URL for upload
        /// </summary>
        public string UploadUrl { get; set; }

        /// <summary>
        /// Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>> (StringComparer.OrdinalIgnoreCase)
            {
                {"expirationDateTime", (n) => { ExpirationDateTime = n.GetDateTimeOffsetValue(); } },
                {"nextExpectedRanges", (n) => { NextExpectedRanges = n.GetCollectionOfPrimitiveValues<string>().ToList(); } },
                {"uploadUrl", (n) => { UploadUrl = n.GetStringValue(); } },
            };
        }

        /// <summary>
        /// Serializes information the current object
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        /// </summary>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteDateTimeOffsetValue("expirationDateTime", ExpirationDateTime);
            writer.WriteCollectionOfPrimitiveValues<string>("nextExpectedRanges", NextExpectedRanges);
            writer.WriteStringValue("uploadUrl", UploadUrl);
            writer.WriteAdditionalData(AdditionalData);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static UploadSession CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new UploadSession();
        }
    }
}
