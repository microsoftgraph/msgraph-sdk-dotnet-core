// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;

    public partial class TestNoteBook: IParsable, IAdditionalDataHolder
    {
        ///<summary>
        /// The Drive constructor
        ///</summary>
        public TestNoteBook()
        {
            this.ODataType = "microsoft.graph.notebook";
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
        /// Gets or sets name.
        /// The name of the item. Read-write.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Gets the field deserializers for the <see cref="TestNoteBook"/> instance
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                {"@odata.type", (n) => { ODataType = n.GetStringValue(); } },
                {"id", (n) => { Id = n.GetStringValue(); } },
                {"displayName", (n) => { DisplayName = n.GetStringValue(); } },
            };
        }

        /// <summary>
        /// Serialize the <see cref="TestNoteBook"/> instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to serialize the instance</param>
        /// <exception cref="ArgumentNullException">Thrown when the writer is null</exception>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("@odata.type", ODataType);
            writer.WriteStringValue("id", Id);
            writer.WriteStringValue("displayName", DisplayName);
            writer.WriteAdditionalData(AdditionalData);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static TestNoteBook CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new TestNoteBook();
        }
    }
}