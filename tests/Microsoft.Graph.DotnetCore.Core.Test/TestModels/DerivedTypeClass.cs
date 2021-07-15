// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// A property bag class for testing derived type deserialization.
    /// </summary>
    [JsonConverter(typeof(DerivedTypeConverter<DerivedTypeClass>))]
    public class DerivedTypeClass : AbstractEntityType
    {
        /// <summary>
        /// Gets or sets enumType.
        /// </summary>
        [JsonPropertyName("enumType")]
        public EnumType? EnumType { get; set; }

        /// <summary>
        /// Gets or sets id.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets id.
        /// </summary>
        [JsonPropertyName("memorableDates")]
        public IEnumerable<DateTestClass> MemorableDates { get; set; }

        /// <summary>
        /// Gets or sets link.
        /// </summary>
        [JsonPropertyName("link")]
        public string WebUrl { get; set; }
    }
}
