// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;

    public class TestChatMessage: IParsable
    {
        /// <summary>
        /// Gets or sets chat id.
        /// If the message was sent in a chat, represents the identity of the chat.
        /// </summary>
        public string ChatId { get; set; }

        /// <summary>
        /// Gets or sets created date time.
        /// Timestamp of when the chat message was created.
        /// </summary>
        public DateTimeOffset? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets deleted date time.
        /// Read only. Timestamp at which the chat message was deleted, or null if not deleted.
        /// </summary>
        public DateTimeOffset? DeletedDateTime { get; set; }

        /// <summary>
        /// Gets or sets etag.
        /// Read-only. Version number of the chat message.
        /// </summary>
        public string Etag { get; set; }

        /// <summary>
        /// Gets or sets body.
        /// Plaintext/HTML representation of the content of the chat message. Representation is specified by the contentType inside the body. The content is always in HTML if the chat message contains a chatMessageMention.
        /// </summary>
        public TestItemBody Body { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the field deserializers for the <see cref="TestChatMessage"/> instance
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <returns></returns>
        public IDictionary<string, Action<T, IParseNode>> GetFieldDeserializers<T>()
        {
            return new Dictionary<string, Action<T, IParseNode>>
            {
                {"chatId", (o,n) => { (o as TestChatMessage).ChatId = n.GetStringValue(); } },
                {"createdDateTime", (o,n) => { (o as TestChatMessage).CreatedDateTime = n.GetDateTimeOffsetValue(); } },
                {"deletedDateTime", (o,n) => { (o as TestChatMessage).DeletedDateTime = n.GetDateTimeOffsetValue(); } },
                {"etag", (o,n) => { (o as TestChatMessage).Etag = n.GetStringValue(); } },
                {"body", (o,n) => { (o as TestChatMessage).Body = n.GetObjectValue<TestItemBody>(TestItemBody.CreateFromDiscriminatorValue); } },
            };
        }

        /// <summary>
        /// Serialize the <see cref="TestChatMessage"/> instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to serialize the instance</param>
        /// <exception cref="ArgumentNullException">Thrown when the writer is null</exception>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("chatId", ChatId);
            writer.WriteDateTimeOffsetValue("createdDateTime", CreatedDateTime);
            writer.WriteDateTimeOffsetValue("deletedDateTime", DeletedDateTime);
            writer.WriteStringValue("etag", Etag);
            writer.WriteObjectValue("body", Body);
            writer.WriteAdditionalData(AdditionalData);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static TestChatMessage CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new TestChatMessage();
        }
    }
}