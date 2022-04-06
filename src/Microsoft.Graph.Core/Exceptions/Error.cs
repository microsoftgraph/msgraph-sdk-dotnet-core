// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The error object contained in 400 and 500 responses returned from the service.
    /// Models OData protocol, 9.4 Error Response Body
    /// http://docs.oasis-open.org/odata/odata/v4.01/csprd05/part1-protocol/odata-v4.01-csprd05-part1-protocol.html#_Toc14172757
    /// </summary>
    public class Error: IParsable, IAdditionalDataHolder
    {
        /// <summary>
        /// This code represents the HTTP status code when this Error object accessed from the ServiceException.Error object.
        /// This code represent a sub-code when the Error object is in the InnerError or ErrorDetails object.
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
        /// An array of details that describe the error[s] encountered with the request.
        /// </summary>
        public List<ErrorDetail> Details { get; set; }

        /// <summary>
        /// The inner error of the response. These are additional error objects that may be more specific than the top level error.
        /// </summary>
        public Error InnerError { get; set; }

        /// <summary>
        /// The Throw site of the error.
        /// </summary>
        public string ThrowSite { get; internal set; }

        /// <summary>
        /// Gets or set the client-request-id header returned in the response headers collection. 
        /// </summary>
        public string ClientRequestId { get; internal set; }

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
                {"target", (n) => {Target = n.GetStringValue(); }},
                {"details", (n) => {Details = n.GetCollectionOfObjectValues<ErrorDetail>(ErrorDetail.CreateFromDiscriminatorValue).ToList(); }},
                {"innerError", (n) => {InnerError = n.GetObjectValue<Error>(Error.CreateFromDiscriminatorValue); }}
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
            writer.WriteCollectionOfObjectValues<ErrorDetail>("details", Details);
            writer.WriteObjectValue<Error>("innerError", InnerError);
            writer.WriteAdditionalData(AdditionalData);
        }

        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        /// </summary>
        public static Error CreateFromDiscriminatorValue(IParseNode parseNode)
        {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new Error();
        }

        /// <summary>
        /// Concatenates the error into a string.
        /// </summary>
        /// <returns>A human-readable string error response.</returns>
        public override string ToString()
        {
            var errorStringBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(this.Code))
            {
                errorStringBuilder.AppendFormat("Code: {0}", this.Code);
                errorStringBuilder.Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(this.Message))
            {
                errorStringBuilder.AppendFormat("Message: {0}", this.Message);
                errorStringBuilder.Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(this.Target))
            {
                errorStringBuilder.AppendFormat("Target: {0}", this.Target);
                errorStringBuilder.Append(Environment.NewLine);
            }

            if (this.Details != null && this.Details.GetEnumerator().MoveNext())
            {
                errorStringBuilder.Append("Details:");
                errorStringBuilder.Append(Environment.NewLine);

                int i = 0;
                foreach (var detail in this.Details)
                {
                    errorStringBuilder.AppendFormat("\tDetail{0}:{1}", i, detail.ToString());
                    errorStringBuilder.Append(Environment.NewLine);
                    i++;
                }
            }

            if (this.InnerError != null)
            {
                errorStringBuilder.Append("Inner error:");
                errorStringBuilder.Append(Environment.NewLine);
                errorStringBuilder.Append("\t" + this.InnerError.ToString());
            }

            if (!string.IsNullOrEmpty(this.ThrowSite))
            {
                errorStringBuilder.AppendFormat("Throw site: {0}", this.ThrowSite);
                errorStringBuilder.Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(this.ClientRequestId))
            {
                errorStringBuilder.AppendFormat("ClientRequestId: {0}", this.ClientRequestId);
                errorStringBuilder.Append(Environment.NewLine);
            }

            if (this.AdditionalData != null && this.AdditionalData.GetEnumerator().MoveNext())
            {
                errorStringBuilder.Append("AdditionalData:");
                errorStringBuilder.Append(Environment.NewLine);
                foreach (var prop in this.AdditionalData)
                {
                    errorStringBuilder.AppendFormat("\t{0}: {1}", prop.Key, prop.Value?.ToString() ?? "null");
                    errorStringBuilder.Append(Environment.NewLine);
                }
            }

            return errorStringBuilder.ToString();
        }
    }
}
