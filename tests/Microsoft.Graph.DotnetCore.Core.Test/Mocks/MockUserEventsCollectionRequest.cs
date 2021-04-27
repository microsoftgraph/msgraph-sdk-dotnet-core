// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph.DotnetCore.Core.Test.Requests;

namespace Microsoft.Graph.DotnetCore.Test.Tasks
{
    public class MockUserEventsCollectionRequest : TestEventDeltaRequest
    {
        private TestEventDeltaCollectionPage nextPage;

        public MockUserEventsCollectionRequest(IBaseClient client,TestEventDeltaCollectionPage nextPage) : base("https://graph.microsoft.com/v1.0/me/events?$skip=10", client, null)
        {
            this.nextPage = nextPage;
        }

        public new Task<ITestEventDeltaCollectionPage> GetAsync()
        {
            return Task.FromResult<ITestEventDeltaCollectionPage>(nextPage);
        }

        public new Task<ITestEventDeltaCollectionPage> GetAsync(CancellationToken cancellationToken)
        {
            return this.GetAsync();
        }
        #region Not implemented

        public TestEvent this[int index] { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public TestEventDeltaRequest NextPageRequest => throw new System.NotImplementedException();

        public IList<TestEvent> CurrentPage => throw new System.NotImplementedException();

        public IDictionary<string, object> AdditionalData { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public int Count => throw new System.NotImplementedException();

        public bool IsReadOnly => throw new System.NotImplementedException();

        public void Add(TestEvent item)
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(TestEvent item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(TestEvent[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<TestEvent> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        public int IndexOf(TestEvent item)
        {
            throw new System.NotImplementedException();
        }

        public void InitializeNextPageRequest(IBaseClient client, string nextPageLinkString)
        {
            throw new System.NotImplementedException();
        }

        public void Insert(int index, TestEvent item)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(TestEvent item)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }
}