// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CSharp.RuntimeBinder;
    using Microsoft.Graph.DotnetCore.Core.Test.Requests;
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels;
    using Microsoft.Graph.DotnetCore.Test.Mocks;
    using Microsoft.Graph.DotnetCore.Test.Tasks;
    using Xunit;

    /**
     Spec https://github.com/microsoftgraph/msgraph-sdk-design/blob/master/tasks/PageIteratorTask.md
    **/
    public class PageIteratorTests : RequestTestBase
    {
        private PageIterator<TestEvent> eventPageIterator;

        [Fact]
        public async Task Given_Concrete_CollectionPage_It_Throws_RuntimeBinderException()
        {
            var page = new CollectionPage<TestEvent>()
            {
                AdditionalData = new Dictionary<string, object>()
            };

            eventPageIterator = PageIterator<TestEvent>.CreatePageIterator(baseClient, page, (e) => { return true; });
            await Assert.ThrowsAsync<RuntimeBinderException>(() => eventPageIterator.IterateAsync());
        }

        [Fact]
        public void Given_Null_CollectionPage_It_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => PageIterator<TestEvent>.CreatePageIterator(baseClient, null, (e) => { return true; }));
        }

        [Fact]
        public void Given_Null_Delegate_It_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => PageIterator<TestEvent>.CreatePageIterator(baseClient, new CollectionPage<TestEvent>(), null));
        }

        [Fact]
        public async Task Given_Concrete_Generated_CollectionPage_It_Iterates_Page_Items()
        {
            int inputEventCount = 17;
            var page = new TestEventDeltaCollectionPage {AdditionalData = new Dictionary<string, object>()};
            for (int i = 0; i < inputEventCount; i++)
            {
                page.Add(new TestEvent() { Subject = $"Subject{i.ToString()}" });
            }

            List<TestEvent> events = new List<TestEvent>();

            eventPageIterator = PageIterator<TestEvent>.CreatePageIterator(baseClient, page, (e) =>
            {
                events.Add(e);
                return true;
            });

            await eventPageIterator.IterateAsync();

            Assert.Equal(inputEventCount, events.Count);
        }

        [Fact]
        public async Task Given_Concrete_Generated_CollectionPage_It_Stops_Iterating_Page_Items()
        {
            int inputEventCount = 17;
            var page = new TestEventDeltaCollectionPage();
            for (int i = 0; i < inputEventCount; i++)
            {
                page.Add(new TestEvent() { Subject = $"Subject{i.ToString()}" });
            }

            List<TestEvent> events = new List<TestEvent>();

            eventPageIterator = PageIterator<TestEvent>.CreatePageIterator(baseClient, page, (e) =>
            {
                if (e.Subject == "Subject7")
                    return false;

                events.Add(e);
                return true;
            });

            await eventPageIterator.IterateAsync();

            Assert.Equal(7, events.Count);
            Assert.Equal(PagingState.Paused, eventPageIterator.State);
        }

        [Fact]
        public async Task Given_CollectionPage_It_Stops_Iterating_Across_Pages()
        {
            // Create the 17 events to initialize the original collection page.
            List<TestEvent> testEvents = new List<TestEvent>();
            int inputEventCount = 17;
            for (int i = 0; i < inputEventCount; i++)
            {
                testEvents.Add(new TestEvent() { Subject = $"Subject{i.ToString()}" });
            }

            // Create the 5 events to initialize the next collection page.
            TestEventDeltaCollectionPage nextPage = new TestEventDeltaCollectionPage();
            int nextPageEventCount = 5;
            for (int i = 0; i < nextPageEventCount; i++)
            {
                nextPage.Add(new TestEvent() { Subject = $"Subject for next page events: {i.ToString()}" });
            }

            // Create the CancellationTokenSource to test the cancellation of paging in the delegate.
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var pagingToken = cancellationTokenSource.Token;

            // Create the delegate to process each entity returned in the pages. The delegate will cancel 
            // paging when the target subject is encountered.
            Func<TestEvent, bool> processEachEvent = (e) =>
            {
                bool shouldContinue = true;

                if (e.Subject.Contains("Subject3"))
                {
                    cancellationTokenSource.Cancel();
                }

                if (e.Subject.Contains("Subject for next page events"))
                {
                    Assert.True(false, "Unexpectedly paged the next page of results.");
                }

                return shouldContinue;
            };

            MockUserEventsCollectionRequest mockUserEventsCollectionRequest = new MockUserEventsCollectionRequest(baseClient, nextPage);
            var mockUserEventsCollectionPage = new MockUserEventsCollectionPage(testEvents, mockUserEventsCollectionRequest) as ITestEventDeltaCollectionPage;

            eventPageIterator = PageIterator<TestEvent>.CreatePageIterator(baseClient, mockUserEventsCollectionPage, processEachEvent);
            await eventPageIterator.IterateAsync(pagingToken);

            Assert.True(cancellationTokenSource.IsCancellationRequested, "The delegate page iterator did not cancel requests to fetch more pages.");
        }

        [Fact]
        public async Task Given_CollectionPage_It_Iterates_Across_Pages()
        {
            // Create the 17 events to initialize the original collection page.
            List<TestEvent> originalCollectionPageEvents = new List<TestEvent>();
            int inputEventCount = 17;
            for (int i = 0; i < inputEventCount; i++)
            {
                originalCollectionPageEvents.Add(new TestEvent() { Subject = $"Subject{i.ToString()}" });
            }

            // Create the 5 events to initialize the next collection page.
            TestEventDeltaCollectionPage nextPage = new TestEventDeltaCollectionPage();
            int nextPageEventCount = 5;
            for (int i = 0; i < nextPageEventCount; i++)
            {
                nextPage.Add(new TestEvent() { Subject = $"Subject for next page events: {i.ToString()}" });
            }
            nextPage.AdditionalData = new Dictionary<string, object>();

            bool reachedNextPage = false;

            // Create the delegate to process each entity returned in the pages. The delegate will 
            // signal that we reached an event in the next page.
            Func<TestEvent, bool> processEachEvent = (e) =>
            {
                if (e.Subject.Contains("Subject for next page events"))
                {
                    reachedNextPage = true;
                    return false;
                }

                return true;
            };

            MockUserEventsCollectionRequest mockUserEventsCollectionRequest = new MockUserEventsCollectionRequest(baseClient, nextPage);
            var mockUserEventsCollectionPage = new MockUserEventsCollectionPage(originalCollectionPageEvents, mockUserEventsCollectionRequest, "nextLink") as ITestEventDeltaCollectionPage;

            eventPageIterator = PageIterator<TestEvent>.CreatePageIterator(baseClient, mockUserEventsCollectionPage, processEachEvent);
            await eventPageIterator.IterateAsync();

            Assert.True(reachedNextPage, "The delegate page iterator did not reach the next page.");
            Assert.Equal(PagingState.Paused, eventPageIterator.State);
        }

        [Fact]
        public async Task Given_CollectionPage_It_Detects_Next_Link_Loop()
        {
            // Create the 17 events to initialize the original collection page.
            List<TestEvent> originalCollectionPageEvents = new List<TestEvent>();
            int inputEventCount = 5;
            for (int i = 0; i < inputEventCount; i++)
            {
                originalCollectionPageEvents.Add(new TestEvent() { Subject = $"Subject{i.ToString()}" });
            }

            // Create the 5 events to initialize the next collection page.
            TestEventDeltaCollectionPage nextPage = new TestEventDeltaCollectionPage();
            int nextPageEventCount = 5;
            for (int i = 0; i < nextPageEventCount; i++)
            {
                nextPage.Add(new TestEvent() { Subject = $"Subject for next page events: {i.ToString()}" });
            }

            // This will be the same nextLink value as the one set in MockUserEventsCollectionPage constructor.
            nextPage.InitializeNextPageRequest(baseClient, "https://graph.microsoft.com/v1.0/me/events?$skip=10");

            // Create the delegate to process each entity returned in the pages. The delegate will 
            // signal that we reached an event in the next page.
            Func<TestEvent, bool> processEachEvent = (e) =>
            {
                return true;
            };

            MockUserEventsCollectionRequest mockUserEventsCollectionRequest = new MockUserEventsCollectionRequest(baseClient, nextPage);
            var mockUserEventsCollectionPage = new MockUserEventsCollectionPage(originalCollectionPageEvents, mockUserEventsCollectionRequest) as ITestEventDeltaCollectionPage;

            eventPageIterator = PageIterator<TestEvent>.CreatePageIterator(baseClient, mockUserEventsCollectionPage, processEachEvent);

            ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await eventPageIterator.IterateAsync());
            Assert.Contains("Detected nextLink loop", exception.Message);
        }

        [Fact]
        public async Task Given_CollectionPage_It_Handles_Empty_NextPage()
        {
            try
            {
                // Create the 17 events to initialize the original collection page.
                List<TestEvent> originalCollectionPageEvents = new List<TestEvent>();
                int inputEventCount = 17;
                for (int i = 0; i < inputEventCount; i++)
                {
                    originalCollectionPageEvents.Add(new TestEvent() { Subject = $"Subject{i.ToString()}" });
                }

                // Create empty next collection page.
                TestEventDeltaCollectionPage nextPage = new TestEventDeltaCollectionPage();

                // Create the delegate to process each entity returned in the pages. 
                Func<TestEvent, bool> processEachEvent = (e) =>
                {
                    return true;
                };

                MockUserEventsCollectionRequest mockUserEventsCollectionRequest = new MockUserEventsCollectionRequest(baseClient, nextPage);
                var mockUserEventsCollectionPage = new MockUserEventsCollectionPage(originalCollectionPageEvents, mockUserEventsCollectionRequest) as ITestEventDeltaCollectionPage;

                eventPageIterator = PageIterator<TestEvent>.CreatePageIterator(baseClient, mockUserEventsCollectionPage, processEachEvent);
                await eventPageIterator.IterateAsync();
            }
            catch (Exception)
            {
                Assert.True(false, "Unexpected exception occurred when next page contains no elements.");
            }
        }

        [Fact]
        public void Given_PageIterator_It_Has_PagingState_NotStarted()
        {
            // Arrange
            List<TestEvent> originalCollectionPageEvents = new List<TestEvent>();
            TestEventDeltaCollectionPage nextPage = new TestEventDeltaCollectionPage();

            MockUserEventsCollectionRequest mockUserEventsCollectionRequest = new MockUserEventsCollectionRequest(baseClient, nextPage);
            var mockUserEventsCollectionPage = new MockUserEventsCollectionPage(originalCollectionPageEvents, mockUserEventsCollectionRequest) as ITestEventDeltaCollectionPage;

            // Act
            eventPageIterator = PageIterator<TestEvent>.CreatePageIterator(baseClient, mockUserEventsCollectionPage, (e) => { return true; });

            // Assert
            Assert.Equal(PagingState.NotStarted, eventPageIterator.State);
        }

        [Fact]
        public async Task Given_RequestConfigurator_It_Is_Invoked()
        {
            // Create the 17 events to initialize the original collection page.
            List<TestEvent> originalCollectionPageEvents = new List<TestEvent>();
            int inputEventCount = 17;
            for (int i = 0; i < inputEventCount; i++)
            {
                originalCollectionPageEvents.Add(new TestEvent() { Subject = $"Subject{i.ToString()}" });
            }

            // Create the 5 events to initialize the next collection page.
            TestEventDeltaCollectionPage nextPage = new TestEventDeltaCollectionPage();
            int nextPageEventCount = 5;
            for (int i = 0; i < nextPageEventCount; i++)
            {
                nextPage.Add(new TestEvent() { Subject = $"Subject for next page events: {i.ToString()}" });
            }
            nextPage.AdditionalData = new Dictionary<string, object>();

            // Create the delegate to process each entity returned in the pages. 
            Func<TestEvent, bool> processEachEvent = (e) => { return true; };

            // Create the delegate to configure the next page request. The delegate will signal that it was invoked.
            bool requestConfiguratorInvoked = false;

            Func<IBaseRequest, IBaseRequest> requestConfigurator = (request) =>
            {
                requestConfiguratorInvoked = true;
                return request;
            };

            MockUserEventsCollectionRequest mockUserEventsCollectionRequest = new MockUserEventsCollectionRequest(baseClient, nextPage);
            var mockUserEventsCollectionPage = new MockUserEventsCollectionPage(originalCollectionPageEvents, mockUserEventsCollectionRequest) as ITestEventDeltaCollectionPage;

            eventPageIterator = PageIterator<TestEvent>.CreatePageIterator(baseClient, mockUserEventsCollectionPage, processEachEvent, requestConfigurator);
            await eventPageIterator.IterateAsync();

            Assert.True(requestConfiguratorInvoked, "The delegate request configurator not invoked.");
        }
    }
}
