// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

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
        private IBaseClient _client;
        private ICollectionPage<TEntity> _currentPage;
        private Queue<TEntity> _pageItemQueue;
        private Func<TEntity, bool> _processPageItemCallback;
        private Func<IBaseRequest, IBaseRequest> _requestConfigurator;

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
        public static PageIterator<TEntity> CreatePageIterator(IBaseClient client, ICollectionPage<TEntity> page, Func<TEntity, bool> callback, Func<IBaseRequest, IBaseRequest> requestConfigurator = null)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            if (page == null)
                throw new ArgumentNullException("page");

            if (callback == null)
                throw new ArgumentNullException("processPageItems");

            return new PageIterator<TEntity>()
            {
                _client = client,
                _currentPage = page,
                _pageItemQueue = new Queue<TEntity>(page),
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
            dynamic page = _currentPage;
            // There are more pages ready to be paged.
            if (page.NextPageRequest != null)
            {
                Nextlink = page.NextPageRequest.GetHttpRequestMessage().RequestUri.AbsoluteUri;
                return true;
            }

            // There are no pages CURRENTLY ready to be paged. Attempt to call delta query later.
            else if (_currentPage.AdditionalData != null && _currentPage.AdditionalData.TryGetValue(CoreConstants.OdataInstanceAnnotations.DeltaLink, out object deltalink))
            {
                Deltalink = deltalink.ToString();
                State = PagingState.Delta;
                Nextlink = string.Empty;

                page.InitializeNextPageRequest(this._client, Deltalink);
                _currentPage = page;

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

            // We need access to the NextPageRequest to call and get the next page. ICollectionPage<TEntity> doesn't define NextPageRequest.
            // We are making this dynamic so we can access NextPageRequest.
            dynamic page = _currentPage;

            // Get the next page if it is available and queue the items for processing.
            if (page.NextPageRequest != null)
            {
                // Call the MSGraph API to get the next page of results and set that page as the currentPage.
                _currentPage = await (_requestConfigurator == null ? page.NextPageRequest : _requestConfigurator(page.NextPageRequest)).GetAsync(token).ConfigureAwait(false);
                page = _currentPage;

                // Add all of the items returned in the response to the queue.
                if (_currentPage != null && _currentPage.Count > 0)
                {
                    foreach (TEntity entity in _currentPage)
                    {
                        _pageItemQueue.Enqueue(entity);
                    }
                }
            }

            // Detect nextLink loop
            if (page.NextPageRequest != null && Nextlink.Equals(page.NextPageRequest.GetHttpRequestMessage().RequestUri.AbsoluteUri))
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
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">Thrown when a base CollectionPage that does not implement NextPageRequest
        /// is provided to the PageIterator</exception>
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
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">Thrown when a base CollectionPage that does not implement NextPageRequest
        /// is provided to the PageIterator</exception>
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
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">Thrown when a base CollectionPage that does not implement NextPageRequest
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
        /// <exception cref="Microsoft.CSharp.RuntimeBinder.RuntimeBinderException">Thrown when a base CollectionPage that does not implement NextPageRequest
        /// is provided to the PageIterator</exception>
        /// <exception cref="Microsoft.Graph.ServiceException">Thrown when the service encounters an error with
        /// a request.</exception>
        public async Task ResumeAsync(CancellationToken token)
        {
            await IterateAsync(token);
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