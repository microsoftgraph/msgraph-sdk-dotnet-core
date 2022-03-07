// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;

    public partial class TestDrive : IParsable, IAdditionalDataHolder
    {
        ///<summary>
        /// The Drive constructor
        ///</summary>
        public TestDrive()
        {
            this.ODataType = "microsoft.graph.drive";
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
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Gets the field deserializers for the <see cref="TestDrive"/> instance
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <returns></returns>
        public IDictionary<string, Action<T, IParseNode>> GetFieldDeserializers<T>()
        {
            return new Dictionary<string, Action<T, IParseNode>>
            {
                {"@odata.type", (o,n) => { (o as TestDrive).ODataType = n.GetStringValue(); } },
                {"id", (o,n) => { (o as TestDrive).Id = n.GetStringValue(); } },
                {"name", (o,n) => { (o as TestDrive).Name = n.GetStringValue(); } },
            };
        }

        /// <summary>
        /// Serialize the <see cref="TestDrive"/> instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to serialize the instance</param>
        /// <exception cref="ArgumentNullException">Thrown when the writer is null</exception>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("@odata.type", ODataType);
            writer.WriteStringValue("id", Id);
            writer.WriteStringValue("name", Name);
            writer.WriteAdditionalData(AdditionalData);
        }
    }
}