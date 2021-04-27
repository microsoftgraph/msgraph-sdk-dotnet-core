// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels;

namespace Microsoft.Graph.DotnetCore.Test.Mocks
{
    using Microsoft.Graph.DotnetCore.Test.Tasks;
    using System;
    using System.Collections.Generic;
    public class MockUserEventsCollectionPage : CollectionPage<TestEvent>, ITestEventDeltaCollectionPage
    {
        public MockUserEventsCollectionPage(IList<TestEvent> currentPage, TestEventDeltaRequest nextPageRequest, string linkType = "") : base(currentPage)
        {
            this.AdditionalData = new Dictionary<string, object>();

            if (linkType == "deltalink")
                AdditionalData.Add(CoreConstants.OdataInstanceAnnotations.DeltaLink, "testDeltalink");

            NextPageRequest = nextPageRequest;
        }

        public TestEventDeltaRequest NextPageRequest { get; private set; }

        public void InitializeNextPageRequest(IBaseClient client, string nextPageLinkString)
        {
            throw new NotImplementedException();
        }
    }
}