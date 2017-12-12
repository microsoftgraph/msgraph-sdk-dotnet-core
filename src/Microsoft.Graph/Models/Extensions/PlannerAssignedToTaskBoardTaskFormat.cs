// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// Represents ordering onformation for the containing <see cref="PlannerTask"/> on "assigned to" task board.
    /// </summary>
    public partial class PlannerAssignedToTaskBoardTaskFormat
    {
        /// <summary>
        /// Returns the order hint that applies to the task in the given assignee's column.
        /// </summary>
        /// <param name="userId">User id of the assignee.</param>
        /// <returns>The order hint.</returns>
        /// <remarks>This method first checks the <see cref="OrderHintsByAssignee"/> dictionary to find a user specific entry, if a suitable entry is not found 
        /// falls back to <see cref="UnassignedOrderHint"/> property.</remarks>
        public string GetOrderHintForAssignee(string userId)
        {
            return this.OrderHintsByAssignee?[userId] ?? this.UnassignedOrderHint;
        }
    }
}
