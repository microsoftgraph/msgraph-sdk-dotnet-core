// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    /// <summary>
    /// Test class for testing serialization of Date.
    /// </summary>
    [JsonConverter(typeof(DerivedTypeConverter<DateTestClass>))]
    public class DateTestClass
    {
        /// <summary>
        /// Gets or sets nullableDate.
        /// </summary>
        [JsonPropertyName("nullableDate")]
        [JsonConverter(typeof(DateConverter))]
        public Date NullableDate { get; set; }

        /// <summary>
        /// Gets or sets dateCollection.
        /// </summary>
        [JsonPropertyName("dateCollection")]
        public IEnumerable<Date> DateCollection { get; set; }

        [JsonPropertyName("invalidType")]
        public int? InvalidType { get; set; }

        [JsonIgnore]
        public int IgnoredNumber { get; set; }
    }
}
