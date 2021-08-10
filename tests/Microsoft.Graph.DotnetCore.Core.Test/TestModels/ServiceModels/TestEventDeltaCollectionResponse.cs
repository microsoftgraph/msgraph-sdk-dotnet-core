// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    /// <summary>
    /// The type UserEventsCollectionResponse.
    /// </summary>

    public class TestEventDeltaCollectionResponse
    {
        /// <summary>
        /// Gets or sets the <see cref="ITestEventDeltaCollectionPage"/> value.
        /// </summary>
        [JsonPropertyName("value")]
        public ITestEventDeltaCollectionPage Value { get; set; }

        /// <summary>
        /// Gets or sets the nextLink string value.
        /// </summary>
        [JsonPropertyName("@odata.nextLink")]
        [JsonConverter(typeof(NextLinkConverter))]
        public string NextLink { get; set; }
        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; set; }
    }
}