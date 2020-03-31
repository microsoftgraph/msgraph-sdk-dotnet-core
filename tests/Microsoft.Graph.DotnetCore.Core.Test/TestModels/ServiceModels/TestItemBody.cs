// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The type ItemBody.
    /// </summary>

    [JsonConverter(typeof(DerivedTypeConverter<TestItemBody>))]
    public partial class TestItemBody
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestItemBody"/> class.
        /// </summary>
        public TestItemBody()
        {
            this.ODataType = "microsoft.graph.itemBody";
        }

        /// <summary>
        /// Gets or sets contentType.
        /// The type of the content. Possible values are text and html.
        /// </summary>
        [JsonPropertyName("contentType")]
        public TestBodyType? ContentType { get; set; }

        /// <summary>
        /// Gets or sets content.
        /// The content of the item.
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Gets or sets @odata.type.
        /// </summary>
        [JsonPropertyName("@odata.type")]
        public string ODataType { get; set; }

    }
}