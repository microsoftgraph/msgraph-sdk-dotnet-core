// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The error details object.
    /// Models OData protocol, 9.4 Error Response Body details object.
    /// http://docs.oasis-open.org/odata/odata/v4.01/csprd05/part1-protocol/odata-v4.01-csprd05-part1-protocol.html#_Toc14172757
    /// </summary>
    public class ErrorDetail
    {
        /// <summary>
        /// This code serves as a sub-status for the error code specified in the Error object.
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }

        /// <summary>
        /// Indicates the target of the error, for example, the name of the property in error.
        /// </summary>
        [JsonPropertyName("target")]
        public string Target { get; set; }

        /// <summary>
        /// The AdditionalData property bag.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; set; }

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