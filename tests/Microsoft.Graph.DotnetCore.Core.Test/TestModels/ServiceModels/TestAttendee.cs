// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    public class TestAttendee : TestRecipient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestAttendee"/> class.
        /// </summary>
        public TestAttendee()
        {
            this.ODataType = "microsoft.graph.attendee";
        }
        
    }
}