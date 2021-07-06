// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using System;
    using System.Text.Json.Serialization;

    public class TestChatMessage
    {
        /// <summary>
        /// Gets or sets chat id.
        /// If the message was sent in a chat, represents the identity of the chat.
        /// </summary>
        [JsonPropertyName("chatId")]
        public string ChatId { get; set; }

        /// <summary>
        /// Gets or sets created date time.
        /// Timestamp of when the chat message was created.
        /// </summary>
        [JsonPropertyName("createdDateTime")]
        public DateTimeOffset? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets deleted date time.
        /// Read only. Timestamp at which the chat message was deleted, or null if not deleted.
        /// </summary>
        [JsonPropertyName("deletedDateTime")]
        public DateTimeOffset? DeletedDateTime { get; set; }

        /// <summary>
        /// Gets or sets etag.
        /// Read-only. Version number of the chat message.
        /// </summary>
        [JsonPropertyName("etag")]
        public string Etag { get; set; }

        /// <summary>
        /// Gets or sets body.
        /// Plaintext/HTML representation of the content of the chat message. Representation is specified by the contentType inside the body. The content is always in HTML if the chat message contains a chatMessageMention.
        /// </summary>
        [JsonPropertyName("body")]
        public TestItemBody Body { get; set; }
    }
}