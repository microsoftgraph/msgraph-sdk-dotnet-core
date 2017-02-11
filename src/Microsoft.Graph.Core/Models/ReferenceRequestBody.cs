// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Newtonsoft.Json;
    using System.Runtime.Serialization;

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ReferenceRequestBody
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "@odata.id", Required = Required.Default)]
        public string ODataId { get; set; }
    }
}
