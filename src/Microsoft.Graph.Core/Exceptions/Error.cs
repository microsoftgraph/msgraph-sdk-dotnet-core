// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// The error object that handles unsuccessful responses returned from the service.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Error
    {
        /// <summary>
        /// The HTTP status code.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "code", Required = Required.Default)]
        public string Code { get; set; }

        /// <summary>
        /// The inner error of the response. These are additional error objects that may be more specific than the top level error.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "innererror", Required = Required.Default)]
        public Error InnerError { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "message", Required = Required.Default)]
        public string Message { get; set; }

        /// <summary>
        /// The Throw site of the error.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "throwSite", Required = Required.Default)]
        public string ThrowSite { get; set; }

        /// <summary>
        /// The AdditionalData property bag.
        /// </summary>
        [JsonExtensionData(ReadData = true)]
        public IDictionary<string, object> AdditionalData { get; set; }

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

            if (!string.IsNullOrEmpty(this.ThrowSite))
            {
                errorStringBuilder.AppendFormat("Throw site: {0}", this.ThrowSite);
                errorStringBuilder.Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(this.Message))
            {
                errorStringBuilder.AppendFormat("Message: {0}", this.Message);
                errorStringBuilder.Append(Environment.NewLine);
            }

            if (this.InnerError != null)
            {
                errorStringBuilder.Append(Environment.NewLine);
                errorStringBuilder.Append("Inner error");
                errorStringBuilder.Append(Environment.NewLine);
                errorStringBuilder.Append(this.InnerError.ToString());
            }

            return errorStringBuilder.ToString();
        }
    }
}
