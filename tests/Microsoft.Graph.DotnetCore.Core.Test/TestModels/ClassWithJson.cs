// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------



namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels
{
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A property bag class with no default constructor for unit testing purposes.
    /// </summary>
    /// 
    public class ClassWithJson
    {
        public ClassWithJson()
        {
        }

        /// <summary>
        /// Gets or sets id.
        /// </summary>
        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; set; }
    }
}
