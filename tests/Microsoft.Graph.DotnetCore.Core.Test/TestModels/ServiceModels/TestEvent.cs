// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{

    /// <summary>
    /// The type TestEvent.
    /// </summary>

    public partial class TestEvent
    {

        ///<summary>
        /// The Event constructor
        ///</summary>
        public TestEvent()
        {
            this.ODataType = "microsoft.graph.event";
        }

        /// <summary>
        /// Gets or sets id.
        /// Read-only.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets @odata.type.
        /// </summary>
        [JsonPropertyName("@odata.type")]
        public string ODataType { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Gets or sets subject.
        /// The text of the event's subject line.
        /// </summary>
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets body.
        /// The body of the message associated with the event. It can be in HTML or text format.
        /// </summary>
        [JsonPropertyName("body")]
        public TestItemBody Body { get; set; }

        /// <summary>
        /// Gets or sets end.
        /// The date, time, and time zone that the event ends. By default, the end time is in UTC.
        /// </summary>
        [JsonPropertyName("end")]
        public TestDateTimeTimeZone End { get; set; }

        /// <summary>
        /// Gets or sets start.
        /// The date, time, and time zone that the event starts. By default, the start time is in UTC.
        /// </summary>
        [JsonPropertyName("start")]
        public TestDateTimeTimeZone Start { get; set; }

        /// <summary>
        /// Gets or sets attendees.
        /// The collection of attendees for the event.
        /// </summary>
        [JsonPropertyName("attendees")]
        public IEnumerable<TestAttendee> Attendees { get; set; }

    }
}