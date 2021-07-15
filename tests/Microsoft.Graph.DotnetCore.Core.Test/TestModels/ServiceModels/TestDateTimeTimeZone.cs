// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The type DateTimeTimeZone.
    /// </summary>
    [JsonConverter(typeof(DerivedTypeConverter<TestDateTimeTimeZone>))]
    public partial class TestDateTimeTimeZone
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestDateTimeTimeZone"/> class.
        /// </summary>
        public TestDateTimeTimeZone()
        {
            this.ODataType = "microsoft.graph.dateTimeTimeZone";
        }

        /// <summary>
        /// Gets or sets dateTime.
        /// A single point of time in a combined date and time representation ({date}T{time}; for example, 2017-08-29T04:00:00.0000000).
        /// </summary>
        [JsonPropertyName("dateTime")]
        public string DateTime { get; set; }

        /// <summary>
        /// Gets or sets timeZone.
        /// Represents a time zone, for example, 'Pacific Standard Time'. See below for more possible values.
        /// </summary>
        [JsonPropertyName("timeZone")]
        public string TimeZone { get; set; }

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