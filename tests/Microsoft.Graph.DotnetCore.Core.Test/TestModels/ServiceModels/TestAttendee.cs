// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;

    public class TestAttendee : TestRecipient,IParsable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAttendee"/> class.
        /// </summary>
        public TestAttendee()
        {
            this.ODataType = "microsoft.graph.attendee";
        }

        /// <summary>
        /// Gets the field deserializers for the <see cref="TestAttendee"/> instance
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <returns></returns>
        public new IDictionary<string, Action<T, IParseNode>> GetFieldDeserializers<T>()
        {
            return new Dictionary<string, Action<T, IParseNode>>(base.GetFieldDeserializers<T>())
            {
            };
        }

        /// <summary>
        /// Serialize the <see cref="TestChangeNotificationEncryptedContent"/> instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to serialize the instance</param>
        /// <exception cref="ArgumentNullException">Thrown when the writer is null</exception>
        public new void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            base.Serialize(writer);
            writer.WriteAdditionalData(AdditionalData);
        }
    }
}