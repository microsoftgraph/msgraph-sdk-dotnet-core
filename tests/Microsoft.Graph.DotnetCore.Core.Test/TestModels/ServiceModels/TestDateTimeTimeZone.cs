// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The type DateTimeTimeZone.
    /// </summary>
    public partial class TestDateTimeTimeZone : IParsable, IAdditionalDataHolder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestDateTimeTimeZone"/> class.
        /// </summary>
        public TestDateTimeTimeZone()
        {
            this.ODataType = "microsoft.graph.dateTimeTimeZone";
        }

        /// <summary>
        /// Gets or sets dateTime.
        /// A single point of time in a combined date and time representation ({date}T{time}; for example, 2017-08-29T04:00:00.0000000).
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// Gets or sets timeZone.
        /// Represents a time zone, for example, 'Pacific Standard Time'. See below for more possible values.
        /// </summary>
        public string TimeZone { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets @odata.type.
        /// </summary>
        public string ODataType { get; set; }

        /// <summary>
        /// Gets the field deserializers for the <see cref="TestDateTimeTimeZone"/> instance
        /// </summary>
        /// <typeparam name="T">The type to deserialize</typeparam>
        /// <returns></returns>
        public IDictionary<string, Action<T, IParseNode>> GetFieldDeserializers<T>()
        {
            return new Dictionary<string, Action<T, IParseNode>>
            {
                {"dateTime", (o,n) => { (o as TestDateTimeTimeZone).DateTime = n.GetStringValue(); } },
                {"timeZone", (o,n) => { (o as TestDateTimeTimeZone).TimeZone = n.GetStringValue(); } },
                {"@odata.type", (o,n) => { (o as TestDateTimeTimeZone).ODataType = n.GetStringValue(); } },
            };
        }

        /// <summary>
        /// Serialize the <see cref="TestDateTimeTimeZone"/> instance
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to serialize the instance</param>
        /// <exception cref="ArgumentNullException">Thrown when the writer is null</exception>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteStringValue("dateTime", DateTime);
            writer.WriteStringValue("timeZone", TimeZone);
            writer.WriteStringValue("@odata.type", ODataType);
            writer.WriteAdditionalData(AdditionalData);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static TestDateTimeTimeZone CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new TestDateTimeTimeZone();
        }
    }
}