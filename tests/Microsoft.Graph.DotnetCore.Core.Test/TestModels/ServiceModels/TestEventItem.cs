// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Kiota.Abstractions.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    public class TestEventItem : IParsable, IAdditionalDataHolder
    {
        /// <summary>true if the meeting organizer allows invitees to propose a new time when responding; otherwise, false. Optional. Default is true.</summary>
        public bool? AllowNewTimeProposals { get; set; }
        /// <summary>The preview of the message associated with the event. It is in text format.</summary>
        public string BodyPreview { get; set; }
        /// <summary>Set to true if the event has attachments.</summary>
        public bool? HasAttachments { get; set; }
        /// <summary>When set to true, each attendee only sees themselves in the meeting request and meeting Tracking list. Default is false.</summary>
        public bool? HideAttendees { get; set; }
        /// <summary>A unique identifier for an event across calendars. This ID is different for each occurrence in a recurring series. Read-only.</summary>
        public string ICalUId { get; set; }
        /// <summary>The occurrences of a recurring series, if the event is a series master. This property includes occurrences that are part of the recurrence pattern, and exceptions that have been modified, but does not include occurrences that have been cancelled from the series. Navigation property. Read-only. Nullable.</summary>
        public List<TestEventItem> Instances { get; set; }
        public bool? IsAllDay { get; set; }
        public bool? IsCancelled { get; set; }
        public bool? IsDraft { get; set; }
        public bool? IsOnlineMeeting { get; set; }
        public bool? IsOrganizer { get; set; }
        public bool? IsReminderOn { get; set; }
        public string OnlineMeetingUrl { get; set; }
        public string OriginalEndTimeZone { get; set; }
        public DateTimeOffset? OriginalStart { get; set; }
        public string OriginalStartTimeZone { get; set; }
        public int? ReminderMinutesBeforeStart { get; set; }
        public bool? ResponseRequested { get; set; }
        public string SeriesMasterId { get; set; }
        public string Subject { get; set; }
        public string TransactionId { get; set; }
        public string WebLink { get; set; }
        /// <summary>Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.</summary>
        public IDictionary<string, object> AdditionalData { get; set; }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        public IDictionary<string, Action<T, IParseNode>> GetFieldDeserializers<T>()
        {
            return new Dictionary<string, Action<T, IParseNode>>() {
                {"allowNewTimeProposals", (o,n) => { (o as TestEventItem).AllowNewTimeProposals = n.GetBoolValue(); } },
                {"bodyPreview", (o,n) => { (o as TestEventItem).BodyPreview = n.GetStringValue(); } },
                {"hasAttachments", (o,n) => { (o as TestEventItem).HasAttachments = n.GetBoolValue(); } },
                {"hideAttendees", (o,n) => { (o as TestEventItem).HideAttendees = n.GetBoolValue(); } },
                {"iCalUId", (o,n) => { (o as TestEventItem).ICalUId = n.GetStringValue(); } },
                {"instances", (o,n) => { (o as TestEventItem).Instances = n.GetCollectionOfObjectValues<TestEventItem>(TestEventItem.CreateFromDiscriminatorValue).ToList(); } },
                {"isAllDay", (o,n) => { (o as TestEventItem).IsAllDay = n.GetBoolValue(); } },
                {"isCancelled", (o,n) => { (o as TestEventItem).IsCancelled = n.GetBoolValue(); } },
                {"isDraft", (o,n) => { (o as TestEventItem).IsDraft = n.GetBoolValue(); } },
                {"isOnlineMeeting", (o,n) => { (o as TestEventItem).IsOnlineMeeting = n.GetBoolValue(); } },
                {"isOrganizer", (o,n) => { (o as TestEventItem).IsOrganizer = n.GetBoolValue(); } },
                {"isReminderOn", (o,n) => { (o as TestEventItem).IsReminderOn = n.GetBoolValue(); } },
                {"onlineMeetingUrl", (o,n) => { (o as TestEventItem).OnlineMeetingUrl = n.GetStringValue(); } },
                {"originalEndTimeZone", (o,n) => { (o as TestEventItem).OriginalEndTimeZone = n.GetStringValue(); } },
                {"originalStart", (o,n) => { (o as TestEventItem).OriginalStart = n.GetDateTimeOffsetValue(); } },
                {"originalStartTimeZone", (o,n) => { (o as TestEventItem).OriginalStartTimeZone = n.GetStringValue(); } },
                {"reminderMinutesBeforeStart", (o,n) => { (o as TestEventItem).ReminderMinutesBeforeStart = n.GetIntValue(); } },
                {"responseRequested", (o,n) => { (o as TestEventItem).ResponseRequested = n.GetBoolValue(); } },
                {"seriesMasterId", (o,n) => { (o as TestEventItem).SeriesMasterId = n.GetStringValue(); } },
                {"subject", (o,n) => { (o as TestEventItem).Subject = n.GetStringValue(); } },
                {"transactionId", (o,n) => { (o as TestEventItem).TransactionId = n.GetStringValue(); } },
                {"webLink", (o,n) => { (o as TestEventItem).WebLink = n.GetStringValue(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        /// </summary>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteBoolValue("allowNewTimeProposals", AllowNewTimeProposals);
            writer.WriteStringValue("bodyPreview", BodyPreview);
            writer.WriteBoolValue("hasAttachments", HasAttachments);
            writer.WriteBoolValue("hideAttendees", HideAttendees);
            writer.WriteStringValue("iCalUId", ICalUId);
            writer.WriteCollectionOfObjectValues<TestEventItem>("instances", Instances);
            writer.WriteBoolValue("isAllDay", IsAllDay);
            writer.WriteBoolValue("isCancelled", IsCancelled);
            writer.WriteBoolValue("isDraft", IsDraft);
            writer.WriteBoolValue("isOnlineMeeting", IsOnlineMeeting);
            writer.WriteBoolValue("isOrganizer", IsOrganizer);
            writer.WriteBoolValue("isReminderOn", IsReminderOn);
            writer.WriteStringValue("onlineMeetingUrl", OnlineMeetingUrl);
            writer.WriteStringValue("originalEndTimeZone", OriginalEndTimeZone);
            writer.WriteDateTimeOffsetValue("originalStart", OriginalStart);
            writer.WriteStringValue("originalStartTimeZone", OriginalStartTimeZone);
            writer.WriteIntValue("reminderMinutesBeforeStart", ReminderMinutesBeforeStart);
            writer.WriteBoolValue("responseRequested", ResponseRequested);
            writer.WriteStringValue("seriesMasterId", SeriesMasterId);
            writer.WriteStringValue("subject", Subject);
            writer.WriteStringValue("transactionId", TransactionId);
            writer.WriteStringValue("webLink", WebLink);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static TestEventItem CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new TestEventItem();
        }
    }
}
