// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels
{
    using System.Text.Json.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// A property bag class with no default constructor for unit testing purposes.
    /// </summary>
    [JsonConverter(typeof(DerivedTypeConverter<AbstractClass>))]
    public abstract class AbstractClass
    {
        protected AbstractClass()
        {

        }

        /// <summary>
        /// Gets or sets id.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; set; }
    }
}
