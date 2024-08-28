// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Serialization;

    /// <summary>
    /// Graph service exception.
    /// </summary>
    public class ServiceException : ApiException, IParsable, IAdditionalDataHolder
    {
        /// <summary>
        /// Creates a new service exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The possible innerException.</param>
        public ServiceException(string message, Exception innerException = null)
            : this(message, responseHeaders: null, statusCode: 0, innerException: innerException)
        {
        }

        /// <summary>
        /// Creates a new service exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The possible innerException.</param>
        /// <param name="responseHeaders">The HTTP response headers from the response.</param>
        /// <param name="statusCode">The HTTP status code from the response.</param>
        public ServiceException(string message, System.Net.Http.Headers.HttpResponseHeaders responseHeaders, int statusCode, Exception innerException = null)
            : base(message, innerException)
        {
            this.ResponseHeaders = responseHeaders;
            this.ResponseStatusCode = statusCode;
        }

        /// <summary>
        /// Creates a new service exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The possible innerException.</param>
        /// <param name="responseHeaders">The HTTP response headers from the response.</param>
        /// <param name="statusCode">The HTTP status code from the response.</param>
        /// <param name="rawResponseBody">The raw JSON response body.</param>
        public ServiceException(string message,
                                System.Net.Http.Headers.HttpResponseHeaders responseHeaders,
                                int statusCode,
                                string rawResponseBody,
                                Exception innerException = null)
            : this(message, responseHeaders, statusCode, innerException)
        {
            this.RawResponseBody = rawResponseBody;
        }

        // ResponseHeaders and StatusCode exposed as pass-through.

        /// <summary>
        /// The HTTP response headers from the response.
        /// </summary>
        public new System.Net.Http.Headers.HttpResponseHeaders ResponseHeaders
        {
            get; private set;
        }

        /// <summary>
        /// Provide the raw JSON response body.
        /// </summary>
        public string RawResponseBody
        {
            get; private set;
        }

        /// <summary>Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.</summary>
        public IDictionary<string, object> AdditionalData
        {
            get; set;
        }

        /// <summary>
        /// Checks if a given error code has been returned in the response at any level in the error stack.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <returns>True if the error code is in the stack.</returns>
        public bool IsMatch(string errorCode)
        {
            if (string.IsNullOrEmpty(errorCode))
            {
                throw new ArgumentException("errorCode cannot be null or empty", nameof(errorCode));
            }

            if (RawResponseBody is not null && RawResponseBody.IndexOf(errorCode, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(Message) && Message.IndexOf(errorCode, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $@"Status Code: {this.ResponseStatusCode}{Environment.NewLine}{base.ToString()}";
        }

        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        public IDictionary<string, Action<IParseNode>> GetFieldDeserializers()
        {
            return new Dictionary<string, Action<IParseNode>> {
                {"statusCode", n => { ResponseStatusCode = n.GetIntValue() ?? 0; } },
                {"rawResponseBody", n => { RawResponseBody = n.GetStringValue(); } }
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        /// </summary>
        public void Serialize(ISerializationWriter writer)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteIntValue("statusCode", ResponseStatusCode);
            writer.WriteStringValue("rawResponseBody", RawResponseBody);
            writer.WriteStringValue("message", Message);
            writer.WriteAdditionalData(AdditionalData);
        }
    }
}
