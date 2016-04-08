// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Test.Mocks
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    /// <summary>
    /// Test class for testing serialization of an IEnumerable of Date.
    /// </summary>
    [DataContract]
    public class DateTestClass
    {
        /// <summary>
        /// Gets or sets StartDate.
        /// </summary>
        [DataMember(Name = "startDate", EmitDefaultValue = false, IsRequired = false)]
        public IEnumerable<Date> StartDate { get; set; }

        [JsonConverter(typeof(DateConverter))]
        [DataMember(Name = "invalidType", EmitDefaultValue = false, IsRequired = false)]
        public int? InvalidType { get; set; }
    }
}
