// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The type ItemBody.
    /// </summary>
    public partial class TestItemBody:  IParsable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestItemBody"/> class.
        /// </summary>
        public TestItemBody()
        {
            this.ODataType = "microsoft.graph.itemBody";
        }

        /// <summary>
        /// Gets or sets contentType.
        /// The type of the content. Possible values are text and html.
        /// </summary>
        public TestBodyType? ContentType { get; set; }

        /// <summary>
        /// Gets or sets content.
        /// The content of the item.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets @odata.type.
        /// </summary>
        public string ODataType { get; set; }

        /// <summary>
        /// Gets the field deserializers for the <see cref="TestItemBody"/> instance
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <returns></returns>
        public IDictionary<string, Action<T, IParseNode>> GetFieldDeserializers<T>()
        {
            return new Dictionary<string, Action<T, IParseNode>>
            {
                {"@odata.type", (o,n) => { (o as TestItemBody).ODataType = n.GetStringValue(); } },
                {"contentType", (o,n) => { (o as TestItemBody).ContentType = n.GetEnumValue<TestBodyType>(); } },
                {"content", (o,n) => { (o as TestItemBody).Content = n.GetStringValue(); } },
            };
        }

        /// <summary>
        /// Serialize the <see cref="TestItemBody"/> instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to serialize the instance</param>
        /// <exception cref="ArgumentNullException">Thrown when the writer is null</exception>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("@odata.type", ODataType);
            writer.WriteEnumValue("contentType", ContentType);
            writer.WriteStringValue("content", Content);
            writer.WriteAdditionalData(AdditionalData);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static TestItemBody CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new TestItemBody();
        }
    }
}