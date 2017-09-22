// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an external reference item on a <see cref="PlannerTaskDetails"/>. 
    /// </summary>
    public partial class PlannerExternalReference
    {
        /// <summary>
        /// OData type name for PlannerExternalReference resource.
        /// </summary>
        internal const string ODataTypeName = "#microsoft.graph.plannerExternalReference";

        /// <summary>
        /// Ensures that @odata.type property is included when this object is serialized. 
        /// This is required since this object is used as a value in dynamic properties of open types. 
        /// </summary>
        /// <param name="context">Serialization context. This parameter is ignored.</param>
        [OnSerializing]
        internal void AddODataType(StreamingContext context)
        {
            if (this.AdditionalData == null)
            {
                this.AdditionalData = new Dictionary<string, object>();
            }

            // We don't want to reapply ODataType. This happens when you try to re-serialize
            // an object you already downloaded. Addresses this issue:
            // https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/182
            if (!this.AdditionalData.ContainsKey(CoreConstants.Serialization.ODataType))
            {
                this.AdditionalData.Add(CoreConstants.Serialization.ODataType, ODataTypeName);
            }
        }
    }
}
