// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Test.TestModels
{
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    /// <summary>
    /// A property bag class for testing derived type deserialization.
    /// </summary>
    [JsonConverter(typeof(DerivedTypeConverter))]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class DerivedTypeClass : AbstractEntityType
    {
        /// <summary>
        /// Gets or sets enumType.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "enumType", Required = Required.Default)]
        public EnumType? EnumType { get; set; }

        /// <summary>
        /// Gets or sets id.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "name", Required = Required.Default)]
        public string Name { get; set; }
    }
}
