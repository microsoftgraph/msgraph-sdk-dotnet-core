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
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>() {
                {"allowNewTimeProposals", (n) => { AllowNewTimeProposals = n.GetBoolValue(); } },
                {"bodyPreview", (n) => { BodyPreview = n.GetStringValue(); } },
                {"hasAttachments", (n) => { HasAttachments = n.GetBoolValue(); } },
                {"hideAttendees", (n) => { HideAttendees = n.GetBoolValue(); } },
                {"iCalUId", (n) => { ICalUId = n.GetStringValue(); } },
                {"instances", (n) => { Instances = n.GetCollectionOfObjectValues<TestEventItem>(TestEventItem.CreateFromDiscriminatorValue).ToList(); } },
                {"isAllDay", (n) => { IsAllDay = n.GetBoolValue(); } },
                {"isCancelled", (n) => { IsCancelled = n.GetBoolValue(); } },
                {"isDraft", (n) => { IsDraft = n.GetBoolValue(); } },
                {"isOnlineMeeting", (n) => { IsOnlineMeeting = n.GetBoolValue(); } },
                {"isOrganizer", (n) => { IsOrganizer = n.GetBoolValue(); } },
                {"isReminderOn", (n) => { IsReminderOn = n.GetBoolValue(); } },
                {"onlineMeetingUrl", (n) => { OnlineMeetingUrl = n.GetStringValue(); } },
                {"originalEndTimeZone", (n) => { OriginalEndTimeZone = n.GetStringValue(); } },
                {"originalStart", (n) => { OriginalStart = n.GetDateTimeOffsetValue(); } },
                {"originalStartTimeZone", (n) => { OriginalStartTimeZone = n.GetStringValue(); } },
                {"reminderMinutesBeforeStart", (n) => { ReminderMinutesBeforeStart = n.GetIntValue(); } },
                {"responseRequested", (n) => { ResponseRequested = n.GetBoolValue(); } },
                {"seriesMasterId", (n) => { SeriesMasterId = n.GetStringValue(); } },
                {"subject", (n) => { Subject = n.GetStringValue(); } },
                {"transactionId", (n) => { TransactionId = n.GetStringValue(); } },
                {"webLink", (n) => { WebLink = n.GetStringValue(); } },
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
