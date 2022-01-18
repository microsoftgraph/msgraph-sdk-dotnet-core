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
    public class DerivedTypeClass : AbstractEntityType, IParsable
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
        /// <typeparam name="T">The type to use</typeparam>
        /// <returns></returns>
        public new IDictionary<string, Action<T, IParseNode>> GetFieldDeserializers<T>()
        {
            return new Dictionary<string, Action<T, IParseNode>>(base.GetFieldDeserializers<T>())
            {
                {"enumType", (o,n) => { (o as DerivedTypeClass).EnumType = n.GetEnumValue<EnumType>(); } },
                {"name", (o,n) => { (o as DerivedTypeClass).Name = n.GetStringValue(); } },
                {"memorableDates", (o,n) => { (o as DerivedTypeClass).MemorableDates = n.GetCollectionOfObjectValues<DateTestClass>(); } },
                {"link", (o,n) => { (o as DerivedTypeClass).WebUrl = n.GetStringValue(); } },
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
    }
}
