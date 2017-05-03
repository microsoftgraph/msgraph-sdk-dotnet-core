// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    public partial class PlannerAssignments : IEnumerable<KeyValuePair<string, PlannerAssignment>>
    {
        public PlannerAssignments()
        {
            this.AdditionalData = new Dictionary<string, object>();
        }

        public IEnumerable<string> Assignees => this.AdditionalData.Where(kvp => kvp.Value is PlannerAssignment).Select(kvp => kvp.Key);

        public int Count => this.Assignees.Count();

        public PlannerAssignment this[string assignee]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(assignee))
                {
                    throw new ArgumentNullException(nameof(assignee));
                }

                if (!this.AdditionalData.TryGetValue(assignee, out object assignmentObject))
                {
                    return null;
                }

                return assignmentObject as PlannerAssignment;
            }

            set
            {
                if (string.IsNullOrWhiteSpace(assignee))
                {
                    throw new ArgumentNullException(nameof(assignee));
                }

                this.AdditionalData[assignee] = value;
            }
        }

        public void AddAssignee(string assignee)
        {
            if (string.IsNullOrEmpty(assignee))
            {
                throw new ArgumentNullException(nameof(assignee));
            }

            var plannerAssignment = new PlannerAssignment();
            plannerAssignment.OrderHint = " !";

            this.AdditionalData.Add(assignee, plannerAssignment);
        }

        public IEnumerator<KeyValuePair<string, PlannerAssignment>> GetEnumerator()
        {
            return this.AdditionalData
                .Where(kvp => kvp.Value is PlannerAssignment)
                .Select(kvp => new KeyValuePair<string, PlannerAssignment>(kvp.Key, (PlannerAssignment)kvp.Value))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        [OnDeserialized]
        internal void DeserializeAssignments(StreamingContext context)
        {
            this.AdditionalData.ConvertComplexTypeProperties<PlannerAssignment>(PlannerAssignment.ODataTypeName);
        }
    }
}
