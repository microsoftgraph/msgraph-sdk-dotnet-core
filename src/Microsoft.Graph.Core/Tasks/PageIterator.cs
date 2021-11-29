// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HttpMethod = Kiota.Abstractions.HttpMethod;

    /*
     Spec https://github.com/microsoftgraph/msgraph-sdk-design/blob/master/tasks/PageIteratorTask.md
    */

    /// <summary>
    /// Use PageIterator&lt;TEntity&gt; to automatically page through result sets across multiple calls 
    /// and process each item in the result set.
    /// </summary>
    /// <typeparam name="TEntity">The Microsoft Graph entity type returned in the result set.</typeparam>
    public class PageIterator<TEntity>
    {
        private BaseClient _client;
        private IParsable _currentPage;
        private Queue<TEntity> _pageItemQueue;
        private Func<TEntity, bool> _processPageItemCallback;
        private Func<RequestInformation, RequestInformation> _requestConfigurator;

        /// <summary>
        /// The @odata.deltaLink returned from a delta query.
        /// </summary>
        public string Deltalink { get; private set; }
        /// <summary>
        /// The @odata.nextLink returned in a paged result.
        /// </summary>
        public string Nextlink { get; private set; }
        /// <summary>
        /// The PageIterator state.
        /// </summary>
        public PagingState State { get; set; }

        /// <summary>
        /// Creates the PageIterator with the results of an initial paged request. 
        /// </summary>
        /// <param name="client">The GraphServiceClient object used to create the NextPageRequest for a delta query.</param>
        /// <param name="page">A generated implementation of ICollectionPage.</param>
        /// <param name="callback">A Func delegate that processes type TEntity in the result set and should return false if the iterator should cancel processing.</param>
        /// <param name="requestConfigurator">A Func delegate that configures the NextPageRequest</param>
        /// <returns>A PageIterator&lt;TEntity&gt; that will process additional result pages based on the rules specified in Func&lt;TEntity,bool&gt; processPageItems</returns>
        public static PageIterator<TEntity> CreatePageIterator(BaseClient client, IParsable page, Func<TEntity, bool> callback, Func<RequestInformation, RequestInformation> requestConfigurator = null)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (page == null)
                throw new ArgumentNullException(nameof(page));

            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (!page.GetFieldDeserializers<IParsable>().ContainsKey("value"))
                throw new ArgumentException("The Parsable does not contain a collection property");

            var pageItems = ExtractEntityListFromParsable(page);

            return new PageIterator<TEntity>()
            {
                _client = client,
                _currentPage = page,
                _pageItemQueue = new Queue<TEntity>(pageItems),
                _processPageItemCallback = callback,
                _requestConfigurator = requestConfigurator,
                State = PagingState.NotStarted
            };
        }

        /// <summary>
        /// Iterate across the content of a a single results page with the callback.
        /// </summary>
        /// <returns>A boolean value that indicates whether the callback cancelled 
        /// iterating across the page results or whether there are more pages to page. 
        /// A return value of false indicates that the iterator should stop iterating.</returns>
        private bool IntrapageIterate()
        {
            State = PagingState.IntrapageIteration;

            while (_pageItemQueue.Count != 0) // && shouldContinue)
            {
                bool shouldContinue = _processPageItemCallback(_pageItemQueue.Dequeue());

                // Cancel processing of items in the page and stop requesting more pages.
                if (!shouldContinue)
                {
                    State = PagingState.Paused;
                    return shouldContinue;
                }
            }

            // Setup deltalink request. Using dynamic to access the NextPageRequest.
            var nextLink = ExtractNextLinkFromParsable(_currentPage);
            // There are more pages ready to be paged.
            if (!string.IsNullOrEmpty(nextLink))
            {
                Nextlink = nextLink;
                Deltalink = string.Empty;
                return true;
            }

            // There are no pages CURRENTLY ready to be paged. Attempt to call delta query later.
            else if (_currentPage.AdditionalData != null && _currentPage.AdditionalData.TryGetValue(CoreConstants.OdataInstanceAnnotations.DeltaLink, out object deltalink))
            {
                Deltalink = deltalink.ToString();
                State = PagingState.Delta;
                Nextlink = string.Empty;

                return false;
            }

            // Paging has completed - no more nextlinks.
            else
            {
                State = PagingState.Complete;
                Nextlink = string.Empty;

                return false;
            }
        }

        /// <summary>
        /// Call the next page request when there is another page of data.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>The task object that represents the results of this asynchronous operation.</returns>
        /// <exception cref="Microsoft.Graph.ServiceException">Thrown when the service encounters an error with
        /// a request.</exception>
        private async Task InterpageIterateAsync(CancellationToken token)
        {
            State = PagingState.InterpageIteration;

            // Get the next page if it is available and queue the items for processing.
            if (!string.IsNullOrEmpty(Nextlink) || !string.IsNullOrEmpty(Deltalink))
            {
                // Call the MSGraph API to get the next page of results and set that page as the currentPage.
                var nextPageRequestInformation = new RequestInformation
                {
                    HttpMethod = HttpMethod.GET,
                    UrlTemplate = string.IsNullOrEmpty(Nextlink) ? Deltalink : Nextlink,
                };
                // if we have a request configurator, modify the request as desired then execute it to get the next page
                nextPageRequestInformation = _requestConfigurator == null ? nextPageRequestInformation : _requestConfigurator(nextPageRequestInformation);
                _currentPage = await GetNextPageAsync(nextPageRequestInformation, token);

                var pageItems = ExtractEntityListFromParsable(_currentPage);
                // Add all of the items returned in the response to the queue.
                if (pageItems != null && pageItems.Count > 0)
                {
                    foreach (TEntity entity in pageItems)
                    {
                        _pageItemQueue.Enqueue(entity);
                    }
                }
            }

            // Detect nextLink loop
            if (Nextlink.Equals(ExtractNextLinkFromParsable(_currentPage)))
            {
                throw new ServiceException(new Error()
                {
                    Message = $"Detected nextLink loop. Nextlink value: {Nextlink}"
                });
            }
        }

        /// <summary>
        /// Fetches page collections and iterates through each page of items and processes it according to the Func&lt;TEntity, bool&gt; set in <see cref="CreatePageIterator"/>. 
        /// </summary>
        /// <returns>The task object that represents the results of this asynchronous operation.</returns>
        /// <exception cref="Microsoft.Graph.ServiceException">Thrown when the service encounters an error with
        /// a request.</exception>
        public async Task IterateAsync()
        {
            await IterateAsync(new CancellationToken());
        }

        /// <summary>
        /// Fetches page collections and iterates through each page of items and processes it according to the Func&lt;TEntity, bool&gt; set in <see cref="CreatePageIterator"/>. 
        /// </summary>
        /// <param name="token">The CancellationToken used to stop iterating calls for more pages.</param>
        /// <returns>The task object that represents the results of this asynchronous operation.</returns>
        /// <exception cref="Microsoft.Graph.ServiceException">Thrown when the service encounters an error with
        /// a request or there is an internal error with the service.</exception>
        public async Task IterateAsync(CancellationToken token)
        {
            // Occurs when we try to request new changes from MSGraph with a deltalink.
            if (State == PagingState.Delta)
            {
                // Make a call to get the next page of results and add items to queue. 
                await InterpageIterateAsync(token);
            }

            // Iterate over the contents of queue. The queue could be from the initial page
            // results passed to the iterator, the results of a delta query, or from a 
            // previously cancelled iteration that gets resumed.
            bool shouldContinueInterpageIteration = IntrapageIterate();

            // Request more pages if they are available.
            while (shouldContinueInterpageIteration && !token.IsCancellationRequested)
            {
                // Make a call to get the next page of results and add items to queue. 
                await InterpageIterateAsync(token);

                // Iterate over items added to the queue by InterpageIterateAsync and
                // determine whether there are more pages to request.
                shouldContinueInterpageIteration = IntrapageIterate();
            }
        }

        /// <summary>
        /// Resumes iterating through each page of items and processes it according to the Func&lt;TEntity, bool&gt; set in <see cref="CreatePageIterator"/>. 
        /// </summary>
        /// <returns>The task object that represents the results of this asynchronous operation.</returns>
        /// is provided to the PageIterator</exception>
        public async Task ResumeAsync()
        {
            await ResumeAsync(new CancellationToken());
        }

        /// <summary>
        /// Resumes iterating through each page of items and processes it according to the Func&lt;TEntity, bool&gt; set in <see cref="CreatePageIterator"/>. 
        /// </summary>
        /// <param name="token">The CancellationToken used to stop iterating calls for more pages.</param>
        /// <returns>The task object that represents the results of this asynchronous operation.</returns>
        /// <exception cref="Microsoft.Graph.ServiceException">Thrown when the service encounters an error with
        /// a request.</exception>
        public async Task ResumeAsync(CancellationToken token)
        {
            await IterateAsync(token);
        }

        /// <summary>
        /// Helper method to extract the collection rom an <see cref="IParsable"/> instance.
        /// </summary>
        /// <param name="parsableCollection">The <see cref="IParsable"/> to extract the collection from</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when the object doesn't contain a collection inside it</exception>
        private static List<TEntity> ExtractEntityListFromParsable(IParsable parsableCollection)
        {
            return parsableCollection.GetType().GetProperty("Value").GetValue(parsableCollection, null) as List<TEntity> ?? throw new ArgumentException("The Parsable does not contain a collection property");
        }

        /// <summary>
        /// Helper method to execute the the request to get back the next page
        /// </summary>
        /// <param name="nextPageRequestInformation">The <see cref="RequestInformation"/> of the next page request</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use to cancel requests</param>
        /// <returns></returns>
        private async Task<IParsable> GetNextPageAsync(RequestInformation nextPageRequestInformation, CancellationToken cancellationToken)
        {
            // Use reflection to invoke the "requestAdapter.SendAsync" method so as to be able to be able to pass the generic type arguments
            // neededed for handling the response.
            var method = _client.RequestAdapter.GetType().GetMethod("SendAsync").MakeGenericMethod(_currentPage.GetType());
            var sendTask = (Task)method.Invoke(_client.RequestAdapter, new object[] { nextPageRequestInformation, null, cancellationToken });
            await sendTask.ConfigureAwait(false);
            // return the result from the task.
            return (IParsable)sendTask.GetType().GetProperty("Result").GetValue(sendTask);
        }

        /// <summary>
        /// Helper method to extract the nextLink property from an <see cref="IParsable"/> instance.
        /// </summary>
        /// <param name="parsableCollection">The <see cref="IParsable"/> to extract the nextLink from</param>
        /// <returns></returns>
        private static string ExtractNextLinkFromParsable(IParsable parsableCollection)
        {
            return parsableCollection.GetType().GetProperty("NextLink").GetValue(parsableCollection, null) as string ?? string.Empty;
        }
    }

    /// <summary>
    /// Specifies the state of the PageIterator.
    /// </summary>
    public enum PagingState
    {
        /// <summary>
        /// The iterator has neither started iterating thorugh the initial page nor request more pages.
        /// </summary>
        NotStarted,
        /// <summary>
        /// The callback returned false or a cancellation token was set. The iterator is resumeable.
        /// </summary>
        Paused,
        /// <summary>
        /// Iterating across the contents of page.
        /// </summary>
        IntrapageIteration,
        /// <summary>
        /// Iterating across paged requests.
        /// </summary>
        InterpageIteration,
        /// <summary>
        /// A deltaToken was returned. The iterator is resumeable.
        /// </summary>
        Delta,
        /// <summary>
        /// Reached the end of a non-deltaLink paged result set.
        /// </summary>
        Complete
    }
}
