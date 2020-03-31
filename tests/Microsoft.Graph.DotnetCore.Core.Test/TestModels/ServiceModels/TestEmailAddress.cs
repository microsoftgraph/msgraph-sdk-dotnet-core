// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The type TestEmailAddress.
    /// </summary>
    [JsonConverter(typeof(DerivedTypeConverter<TestEmailAddress>))]
    public partial class TestEmailAddress
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestEmailAddress"/> class.
        /// </summary>
        public TestEmailAddress()
        {
            this.ODataType = "microsoft.graph.emailAddress";
        }

        /// <summary>
        /// Gets or sets name.
        /// The display name of the person or entity.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets address.
        /// The email address of the person or entity.
        /// </summary>
        [JsonPropertyName("address")]
        public string Address { get; set; }

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