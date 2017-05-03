// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    public partial class PlannerAssignment
    {
        internal const string ODataTypeName = "#microsoft.graph.plannerAssignment";

        [OnSerializing]
        public void AddODataType(StreamingContext context)
        {
            if (this.AdditionalData == null)
            {
                this.AdditionalData = new Dictionary<string, object>();
            }

            this.AdditionalData.Add("@odata.type", ODataTypeName);
        }
    }
}
