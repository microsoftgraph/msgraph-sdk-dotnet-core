// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The type AsyncOperationStatus.
    /// </summary>
    public partial class AsyncOperationStatus: IParsable, IAdditionalDataHolder
    {
        /// <summary>
        /// Gets or sets operation.
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets percentageComplete.
        /// </summary>
        public double? PercentageComplete { get; set; }

        /// <summary>
        /// Gets or sets status.
        /// </summary>
        public string Status { get; set; }
    
        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the field deserializers for the <see cref="AsyncOperationStatus"/> instance
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                {"operation", (n) => { Operation = n.GetStringValue(); } },
                {"percentageComplete", (n) => { PercentageComplete = n.GetDoubleValue(); } },
                {"status", (n) => { Status = n.GetStringValue(); } },
            };
        }

        /// <summary>
        /// Serialize the <see cref="AsyncOperationStatus"/> instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to serialize the instance</param>
        /// <exception cref="ArgumentNullException">Thrown when the writer is null</exception>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("operation", Operation);
            writer.WriteDoubleValue("percentageComplete", PercentageComplete);
            writer.WriteStringValue("status", Status);
            writer.WriteAdditionalData(AdditionalData);
        }
    }
}
