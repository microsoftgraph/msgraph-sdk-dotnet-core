// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Kiota.Abstractions.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    /// <summary>
    /// The type UserEventsCollectionResponse.
    /// </summary>

    public class TestEventDeltaCollectionResponse: IParsable
    {
        /// <summary>
        /// Gets or sets the event collection value.
        /// </summary>
        public List<TestEvent> Value { get; set; }

        /// <summary>
        /// Gets or sets the nextLink string value.
        /// </summary>
        public string NextLink { get; set; }
        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Gets the field deserializers for the <see cref="TestEventDeltaCollectionResponse"/> instance
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <returns></returns>
        public IDictionary<string, Action<T, IParseNode>> GetFieldDeserializers<T>()
        {
            return new Dictionary<string, Action<T, IParseNode>>
            {
                {"@odata.nextLink", (o,n) => { (o as TestEventDeltaCollectionResponse).NextLink = n.GetStringValue(); } },
                {"value", (o,n) => { (o as TestEventDeltaCollectionResponse).Value = n.GetCollectionOfObjectValues<TestEvent>(TestEvent.CreateFromDiscriminatorValue).ToList(); } },
            };
        }

        /// <summary>
        /// Serialize the <see cref="TestEventDeltaCollectionResponse"/> instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to serialize the instance</param>
        /// <exception cref="ArgumentNullException">Thrown when the writer is null</exception>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("@odata.nextLink", NextLink);
            writer.WriteCollectionOfObjectValues("value", Value);
            writer.WriteAdditionalData(AdditionalData);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static TestEventDeltaCollectionResponse CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new TestEventDeltaCollectionResponse();
        }
    }
}