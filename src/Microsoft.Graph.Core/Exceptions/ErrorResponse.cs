// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The error response object from the service on an unsuccessful call.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// The <see cref="Error"/> returned by the service.
        /// </summary>
        [JsonPropertyName("error")]
        public Error Error { get; set; }

        /// <summary>
        /// Additional data returned in the call.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; set; }
    }
}
