// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Serialization;

    /// <summary>
    /// Test class for testing serialization of Date.
    /// </summary>
    public class DateTestClass : IParsable
    {
        /// <summary>
        /// Gets or sets nullableDate.
        /// </summary>
        public Date? NullableDate
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets dateCollection.
        /// </summary>
        public IEnumerable<Date?> DateCollection
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets InvalidType.
        /// </summary>
        public int? InvalidType
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets IgnoredNumber
        /// </summary>
        public int IgnoredNumber
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets AdditionalData
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the field deserializers for the class
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>>
            {
                {"nullableDate", (n) => { NullableDate = n.GetDateValue(); } },
                {"dateCollection", (n) => { DateCollection = n.GetCollectionOfPrimitiveValues<Date?>(); } },
                {"invalidType", (n) => { InvalidType = n.GetIntValue(); } },
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
            writer.WriteDateValue("nullableDate", NullableDate);
            writer.WriteCollectionOfPrimitiveValues("dateCollection", DateCollection);
            writer.WriteIntValue("invalidType", InvalidType);
            writer.WriteAdditionalData(AdditionalData);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static DateTestClass CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new DateTestClass();
        }
    }
}
