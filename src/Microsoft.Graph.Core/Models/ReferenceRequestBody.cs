// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------


namespace Microsoft.Graph
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// The reference request body.
    /// </summary>
    public class ReferenceRequestBody
    {
        /// <summary>
        /// The OData.id value.
        /// </summary>
        [JsonPropertyName("@odata.id")]
        public string ODataId
        {
            get; set;
        }
    }
}
