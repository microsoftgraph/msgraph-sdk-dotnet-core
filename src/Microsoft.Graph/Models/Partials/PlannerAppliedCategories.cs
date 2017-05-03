// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the categories applied for a <see cref="PlannerTask"/>. The category descriptions are defined for each Plan, in <see cref="PlannerPlanDetails" /> resource.
    /// </summary>
    public partial class PlannerAppliedCategories
    {
        /// <summary>
        /// Specifies if category1 is applied to a the task.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "category1", Required = Required.Default)]
        public bool? Category1 { get; set; }

        /// <summary>
        /// Specifies if category2 is applied to a the task.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "category2", Required = Required.Default)]
        public bool? Category2 { get; set; }

        /// <summary>
        /// Specifies if category3 is applied to a the task.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "category3", Required = Required.Default)]
        public bool? Category3 { get; set; }

        /// <summary>
        /// Specifies if category4 is applied to a the task.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "category4", Required = Required.Default)]
        public bool? Category4 { get; set; }

        /// <summary>
        /// Specifies if category5 is applied to a the task.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "category5", Required = Required.Default)]
        public bool? Category5 { get; set; }

        /// <summary>
        /// Specifies if category6 is applied to a the task.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "category6", Required = Required.Default)]
        public bool? Category6 { get; set; }
    }
}
