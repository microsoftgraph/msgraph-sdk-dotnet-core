// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents an intermediate object used for deserializing OData method responses
    /// that return a single boolean OData primitive. This type is consumed by code files
    /// generated with:
    /// https://github.com/microsoftgraph/MSGraph-SDK-Code-Generator/blob/dev/Templates/CSharp/Requests/MethodRequest.cs.tt
    /// The value of a return type is an object:
    /// http://docs.oasis-open.org/odata/odata-csdl-json/v4.01/odata-csdl-json-v4.01.html#sec_ReturnType
    /// </summary>
    public class ODataMethodBooleanResponse
    {
        /// <summary>
        /// Nullable in case the value property is not present.
        /// /// </summary>
        [JsonPropertyName("value")]
        public bool? Value { get; set; }

        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData { get; set; }
    }
}