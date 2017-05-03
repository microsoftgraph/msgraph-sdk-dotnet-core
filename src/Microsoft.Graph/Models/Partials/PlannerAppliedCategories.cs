// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Newtonsoft.Json;

    public partial class PlannerAppliedCategories
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "category1", Required = Newtonsoft.Json.Required.Default)]
        public bool? Category1{ get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "category2", Required = Newtonsoft.Json.Required.Default)]
        public bool? Category2 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "category3", Required = Newtonsoft.Json.Required.Default)]
        public bool? Category3 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "category4", Required = Newtonsoft.Json.Required.Default)]
        public bool? Category4 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "category5", Required = Newtonsoft.Json.Required.Default)]
        public bool? Category5 { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "category6", Required = Newtonsoft.Json.Required.Default)]
        public bool? Category6 { get; set; }
    }
}
