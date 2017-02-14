// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Test.TestModels
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    /// <summary>
    /// Test class for testing serialization of Date.
    /// </summary>
    [JsonConverter(typeof(DerivedTypeConverter))]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class DateTestClass
    {
        /// <summary>
        /// Gets or sets nullableDate.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "nullableDate", Required = Required.Default)]
        public Date NullableDate { get; set; }

        /// <summary>
        /// Gets or sets dateCollection.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "dateCollection", Required = Required.Default)]
        public IEnumerable<Date> DateCollection { get; set; }

        [JsonConverter(typeof(DateConverter))]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "invalidType", Required = Required.Default)]
        public int? InvalidType { get; set; }
    }
}
