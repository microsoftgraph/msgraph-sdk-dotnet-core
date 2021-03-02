// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.TestModels.ServiceModels
{
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// The type UserEventsCollectionRequest.
    /// </summary>
    public partial class TestEventDeltaRequest : BaseRequest
    {
        /// <summary>
        /// Constructs a new UserEventsCollectionRequest.
        /// </summary>
        /// <param name="requestUrl">The URL for the built request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        /// <param name="options">Query and header option name value pairs for the request.</param>
        public TestEventDeltaRequest(
            string requestUrl,
            IBaseClient client,
            IEnumerable<Option> options)
            : base(requestUrl, client, options)
        {
        }

        /// <summary>
        /// Adds the specified Event to the collection via POST.
        /// </summary>
        /// <param name="eventsEvent">The Event to add.</param>
        /// <returns>The created Event.</returns>
        public System.Threading.Tasks.Task<TestEvent> AddAsync(TestEvent eventsEvent)
        {
            return this.AddAsync(eventsEvent, CancellationToken.None);
        }

        /// <summary>
        /// Adds the specified Event to the collection via POST.
        /// </summary>
        /// <param name="eventsEvent">The Event to add.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The created Event.</returns>
        public System.Threading.Tasks.Task<TestEvent> AddAsync(TestEvent eventsEvent, CancellationToken cancellationToken)
        {
            this.ContentType = CoreConstants.MimeTypeNames.Application.Json;
            this.Method = CoreConstants.HttpMethods.POST;
            return this.SendAsync<TestEvent>(eventsEvent, cancellationToken);
        }

        /// <summary>
        /// Gets the collection page.
        /// </summary>
        /// <returns>The collection page.</returns>
        public System.Threading.Tasks.Task<ITestEventDeltaCollectionPage> GetAsync()
        {
            return this.GetAsync(CancellationToken.None);
        }

        /// <summary>
        /// Gets the collection page.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The collection page.</returns>
        public async System.Threading.Tasks.Task<ITestEventDeltaCollectionPage> GetAsync(CancellationToken cancellationToken)
        {
            this.Method = CoreConstants.HttpMethods.GET;
            var response = await this.SendAsync<TestEventDeltaCollectionResponse>(null, cancellationToken).ConfigureAwait(false);
            if (response != null && response.Value != null && response.Value.CurrentPage != null)
            {
                if (response.AdditionalData != null)
                {
                    object nextPageLink;
                    response.AdditionalData.TryGetValue("@odata.nextLink", out nextPageLink);

                    var nextPageLinkString = nextPageLink as string;

                    if (!string.IsNullOrEmpty(nextPageLinkString))
                    {
                        response.Value.InitializeNextPageRequest(
                            this.Client,
                            nextPageLinkString);
                    }

                    // Copy the additional data collection to the page itself so that information is not lost
                    response.Value.AdditionalData = response.AdditionalData;
                }

                return response.Value;
            }

            return null;
        }

    }
}