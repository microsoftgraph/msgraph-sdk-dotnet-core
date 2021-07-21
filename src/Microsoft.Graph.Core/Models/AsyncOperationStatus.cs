// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The type AsyncOperationStatus.
    /// </summary>
    [JsonConverter(typeof(DerivedTypeConverter<AsyncOperationStatus>))]
    public partial class AsyncOperationStatus
    {

        /// <summary>
        /// Gets or sets operation.
        /// </summary>
        [JsonPropertyName("operation")]
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets percentageComplete.
        /// </summary>
        [JsonPropertyName("percentageComplete")]
        public double? PercentageComplete { get; set; }

        /// <summary>
        /// Gets or sets status.
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }
    
        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; set; }
    
    }
}
