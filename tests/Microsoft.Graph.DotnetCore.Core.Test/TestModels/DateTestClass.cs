// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Test class for testing serialization of Date.
    /// </summary>
    public class DateTestClass: IParsable
    {
        /// <summary>
        /// Gets or sets nullableDate.
        /// </summary>
        public Date NullableDate { get; set; }

        /// <summary>
        /// Gets or sets dateCollection.
        /// </summary>
        public IEnumerable<Date> DateCollection { get; set; }

        /// <summary>
        /// Gets or sets InvalidType.
        /// </summary>
        public int? InvalidType { get; set; }

        /// <summary>
        /// Gets or sets IgnoredNumber
        /// </summary>
        public int IgnoredNumber { get; set; }

        /// <summary>
        /// Gets or sets AdditionalData
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the field deserializers for the class
        /// </summary>
        /// <typeparam name="T">The type to use</typeparam>
        /// <returns></returns>
        public IDictionary<string, Action<T, IParseNode>> GetFieldDeserializers<T>()
        {
            return new Dictionary<string, Action<T, IParseNode>>
            {
                {"nullableDate", (o,n) => { (o as DateTestClass).NullableDate = new Date(n.GetDateTimeOffsetValue().Value.DateTime); } },
                {"dateCollection", (o,n) => { (o as DateTestClass).DateCollection = n.GetCollectionOfPrimitiveValues<DateTimeOffset?>().Select(dateTimeOffset => new Date(dateTimeOffset.Value.DateTime) ); } },
                {"invalidType", (o,n) => { (o as DateTestClass).InvalidType = n.GetIntValue(); } },
            };
        }

        /// <summary>
        /// Serializes this instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to use.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("nullableDate", NullableDate?.ToString());
            writer.WriteCollectionOfPrimitiveValues("dateCollection", DateCollection?.Select( date => date.ToString()));
            writer.WriteIntValue("invalidType", InvalidType);
            writer.WriteAdditionalData(AdditionalData);
        }
    }
}
