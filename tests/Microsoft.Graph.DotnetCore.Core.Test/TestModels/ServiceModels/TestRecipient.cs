// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;

    public class TestRecipient: IParsable, IAdditionalDataHolder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestRecipient"/> class.
        /// </summary>
        public TestRecipient()
        {
            this.ODataType = "microsoft.graph.recipient";
        }

        /// <summary>
        /// Gets or sets emailAddress.
        /// The recipient's email address.
        /// </summary>
        public TestEmailAddress EmailAddress { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Gets or sets @odata.type.
        /// </summary>
        public string ODataType { get; set; }

        /// <summary>
        /// Gets the field deserializers for the <see cref="TestRecipient"/> instance
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <returns></returns>
        public IDictionary<string, Action<T, IParseNode>> GetFieldDeserializers<T>()
        {
            return new Dictionary<string, Action<T, IParseNode>>
            {
                {"@odata.type", (o,n) => { (o as TestRecipient).ODataType = n.GetStringValue(); } },
                {"emailAddress", (o,n) => { (o as TestRecipient).EmailAddress = n.GetObjectValue<TestEmailAddress>(TestEmailAddress.CreateFromDiscriminatorValue); } },
            };
        }

        /// <summary>
        /// Serialize the <see cref="TestRecipient"/> instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to serialize the instance</param>
        /// <exception cref="ArgumentNullException">Thrown when the writer is null</exception>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("@odata.type", ODataType);
            writer.WriteObjectValue("emailAddress", EmailAddress);
            writer.WriteAdditionalData(AdditionalData);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static TestRecipient CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            var mappingValueNode = parseNode.GetChildNode("@odata.type");
            var mappingValue = mappingValueNode?.GetStringValue();
            return mappingValue switch
            {
                "microsoft.graph.attendee" => new TestAttendee(),
                _ => new TestRecipient()
            };
        }
    }
}