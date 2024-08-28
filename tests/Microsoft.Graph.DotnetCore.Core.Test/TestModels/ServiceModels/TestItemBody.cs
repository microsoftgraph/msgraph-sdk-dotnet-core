// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Kiota.Abstractions.Serialization;

    /// <summary>
    /// The type ItemBody.
    /// </summary>
    public partial class TestItemBody : IParsable, IAdditionalDataHolder
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
        public TestBodyType? ContentType
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets content.
        /// The content of the item.
        /// </summary>
        public string Content
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets @odata.type.
        /// </summary>
        public string ODataType
        {
            get; set;
        }

        /// <summary>
        /// Gets the field deserializers for the <see cref="TestItemBody"/> instance
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                {"@odata.type", (n) => { ODataType = n.GetStringValue(); } },
                {"contentType", (n) => { ContentType = n.GetEnumValue<TestBodyType>(); } },
                {"content", (n) => { Content = n.GetStringValue(); } },
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
