// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The error response object from the service on an unsuccessful call.
    /// </summary>
    public class ErrorResponse :IParsable
    {
        /// <summary>
        /// The <see cref="Error"/> returned by the service.
        /// </summary>
        public Error Error { get; set; }

        /// <summary>
        /// Additional data returned in the call.
        /// </summary>
        public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the field deserializers for the class
        /// </summary>
        /// <typeparam name="T">The type of the class</typeparam>
        /// <returns></returns>
        public IDictionary<string, Action<T, IParseNode>> GetFieldDeserializers<T>()
        {
            return new Dictionary<string, Action<T, IParseNode>>
            {
                {"error", (o,n) => { (o as ErrorResponse).Error = n.GetObjectValue<Error>(Error.CreateFromDiscriminatorValue); } }
            };
        }

        /// <summary>
        /// Serializes the class using the the given writer
        /// </summary>
        /// <param name="writer">The <see cref="ISerializationWriter"/> to use</param>
        /// <exception cref="ArgumentNullException">Thrown when the provided writer is null</exception>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteObjectValue("error", Error);
            writer.WriteAdditionalData(AdditionalData);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static ErrorResponse CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new ErrorResponse();
        }
    }
}
