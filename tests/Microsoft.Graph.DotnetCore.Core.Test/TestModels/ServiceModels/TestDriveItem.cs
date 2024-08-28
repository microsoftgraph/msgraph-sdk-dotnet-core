// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using System;
    using System.Collections.Generic;
    using System.Text.Json.Serialization;
    using Microsoft.Kiota.Abstractions.Serialization;

    public partial class TestDriveItem : IParsable, IAdditionalDataHolder
    {
        ///<summary>
        /// The Drive constructor
        ///</summary>
        public TestDriveItem()
        {
            this.ODataType = "microsoft.graph.drive";
        }

        /// <summary>
        /// Gets or sets id.
        /// Read-only.
        /// </summary>
        public string Id
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets @odata.type.
        /// </summary>
        [JsonPropertyName("@odata.type")]
        public string ODataType
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets name.
        /// The name of the item. Read-write.
        /// </summary>
        [JsonPropertyName("name")]
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets size.
        /// Size of the item in bytes. Read-only.
        /// </summary>
        [JsonPropertyName("size")]
        public Int64? Size
        {
            get; set;
        }

        /// <summary>
        /// Gets the field deserializers for the <see cref="TestDriveItem"/> instance
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                {"id", (n) => { Id = n.GetStringValue(); } },
                {"@odata.type", (n) => { ODataType = n.GetStringValue(); } },
                {"name", (n) => { Name = n.GetStringValue(); } },
                {"size", (n) => { Size = n.GetLongValue(); } }
            };
        }

        /// <summary>
        /// Serialize the <see cref="TestDriveItem"/> instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to serialize the instance</param>
        /// <exception cref="ArgumentNullException">Thrown when the writer is null</exception>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("id", Id);
            writer.WriteStringValue("@odata.type", ODataType);
            writer.WriteStringValue("name", Name);
            writer.WriteLongValue("size", Size);
        }
    }
}
