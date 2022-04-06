// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// The error details object.
    /// Models OData protocol, 9.4 Error Response Body details object.
    /// http://docs.oasis-open.org/odata/odata/v4.01/csprd05/part1-protocol/odata-v4.01-csprd05-part1-protocol.html#_Toc14172757
    /// </summary>
    public class ErrorDetail : IParsable, IAdditionalDataHolder
    {
        /// <summary>
        /// This code serves as a sub-status for the error code specified in the Error object.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Indicates the target of the error, for example, the name of the property in error.
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// The AdditionalData property bag.
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
                {"code", (n) => { Code = n.GetStringValue(); } },
                {"message", (n) => {Message = n.GetStringValue(); }},
                {"target", (n) => {Target = n.GetStringValue(); }}
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
            writer.WriteStringValue("code", Code);
            writer.WriteStringValue("message", Message);
            writer.WriteStringValue("target", Target);
            writer.WriteAdditionalData(AdditionalData);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static ErrorDetail CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new ErrorDetail();
        }

        /// <summary>
        /// Concatenates the error detail into a string.
        /// </summary>
        /// <returns>A string representation of an ErrorDetail object.</returns>
        public override string ToString()
        {
            var errorDetailsStringBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(this.Code))
            {
                errorDetailsStringBuilder.Append(Environment.NewLine);
                errorDetailsStringBuilder.AppendFormat("\t\tCode: {0}", this.Code);
                errorDetailsStringBuilder.Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(this.Message))
            {
                errorDetailsStringBuilder.AppendFormat("\t\tMessage: {0}", this.Message);
                errorDetailsStringBuilder.Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(this.Target))
            {
                errorDetailsStringBuilder.AppendFormat("\t\tTarget: {0}", this.Target);
                errorDetailsStringBuilder.Append(Environment.NewLine);
            }

            if (this.AdditionalData != null && this.AdditionalData.GetEnumerator().MoveNext())
            {
                errorDetailsStringBuilder.Append("\t\tAdditionalData:");
                errorDetailsStringBuilder.Append(Environment.NewLine);
                foreach (var prop in this.AdditionalData)
                {
                    errorDetailsStringBuilder.AppendFormat("\t{0} : {1}", prop.Key, prop.Value.ToString());
                    errorDetailsStringBuilder.Append(Environment.NewLine);
                }
            }

            return errorDetailsStringBuilder.ToString();
        }
    }
}