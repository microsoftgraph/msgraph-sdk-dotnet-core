// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Kiota.Abstractions.Serialization;
using System;
using System.Collections.Generic;

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{

    /// <summary>
    /// The type TestEvent.
    /// </summary>
    public partial class TestEvent : IParsable
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
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets @odata.type.
        /// </summary>
        public string ODataType { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Gets or sets subject.
        /// The text of the event's subject line.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets body.
        /// The body of the message associated with the event. It can be in HTML or text format.
        /// </summary>
        public TestItemBody Body { get; set; }

        /// <summary>
        /// Gets or sets end.
        /// The date, time, and time zone that the event ends. By default, the end time is in UTC.
        /// </summary>
        public TestDateTimeTimeZone End { get; set; }

        /// <summary>
        /// Gets or sets start.
        /// The date, time, and time zone that the event starts. By default, the start time is in UTC.
        /// </summary>
        public TestDateTimeTimeZone Start { get; set; }

        /// <summary>
        /// Gets or sets attendees.
        /// The collection of attendees for the event.
        /// </summary>
        public IEnumerable<TestAttendee> Attendees { get; set; }

        /// <summary>
        /// Gets the field deserializers for the <see cref="TestEvent"/> instance
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <returns></returns>
        public IDictionary<string, Action<T, IParseNode>> GetFieldDeserializers<T>()
        {
            return new Dictionary<string, Action<T, IParseNode>>
            {
                {"@odata.type", (o,n) => { (o as TestEvent).ODataType = n.GetStringValue(); } },
                {"id", (o,n) => { (o as TestEvent).Id = n.GetStringValue(); } },
                {"subject", (o,n) => { (o as TestEvent).Subject = n.GetStringValue(); } },
                {"body", (o,n) => { (o as TestEvent).Body = n.GetObjectValue<TestItemBody>(TestItemBody.CreateFromDiscriminatorValue); } },
                {"end", (o,n) => { (o as TestEvent).End = n.GetObjectValue<TestDateTimeTimeZone>(TestDateTimeTimeZone.CreateFromDiscriminatorValue); } },
                {"start", (o,n) => { (o as TestEvent).Start = n.GetObjectValue<TestDateTimeTimeZone>(TestDateTimeTimeZone.CreateFromDiscriminatorValue); } },
                {"attendees", (o,n) => { (o as TestEvent).Attendees = n.GetCollectionOfObjectValues<TestAttendee>(TestAttendee.CreateFromDiscriminatorValue); } },
            };
        }

        /// <summary>
        /// Serialize the <see cref="TestEvent"/> instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to serialize the instance</param>
        /// <exception cref="ArgumentNullException">Thrown when the writer is null</exception>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("@odata.type", ODataType);
            writer.WriteStringValue("id", Id);
            writer.WriteStringValue("subject", Subject);
            writer.WriteObjectValue("body", Body);
            writer.WriteObjectValue("end", End);
            writer.WriteObjectValue("start", Start);
            writer.WriteCollectionOfObjectValues("attendees", Attendees);
            writer.WriteAdditionalData(AdditionalData);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static TestEvent CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new TestEvent();
        }
    }
}