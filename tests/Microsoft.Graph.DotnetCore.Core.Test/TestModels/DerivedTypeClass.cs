// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A property bag class for testing derived type deserialization.
    /// </summary>
    public class DerivedTypeClass : AbstractEntityType, IParsable, IAdditionalDataHolder
    {
        /// <summary>
        /// Gets or sets enumType.
        /// </summary>
        public EnumType? EnumType { get; set; }

        /// <summary>
        /// Gets or sets id.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets id.
        /// </summary>
        public IEnumerable<DateTestClass> MemorableDates { get; set; }

        /// <summary>
        /// Gets or sets link.
        /// </summary>
        public string WebUrl { get; set; }

        /// <summary>
        /// Gets the field deserializers for the class
        /// </summary>
        /// <returns></returns>
        public new IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>(base.GetFieldDeserializers())
            {
                {"enumType", (n) => { EnumType = n.GetEnumValue<EnumType>(); } },
                {"name", (n) => { Name = n.GetStringValue(); } },
                {"memorableDates", (n) => { MemorableDates = n.GetCollectionOfObjectValues<DateTestClass>(DateTestClass.CreateFromDiscriminatorValue); } },
                {"link", (n) => { WebUrl = n.GetStringValue(); } },
            };
        }

        /// <summary>
        /// Serializes this instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to use.</param>
        /// <exception cref="NotImplementedException"></exception>
        public new void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            base.Serialize(writer);
            writer.WriteEnumValue("enumType", EnumType);
            writer.WriteStringValue("name", Name);
            writer.WriteCollectionOfObjectValues("memorableDates", MemorableDates);
            writer.WriteStringValue("link", WebUrl);
            writer.WriteAdditionalData(AdditionalData);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static new DerivedTypeClass CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new DerivedTypeClass();
        }
    }
}
