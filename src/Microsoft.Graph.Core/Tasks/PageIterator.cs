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

    /*
     Spec https://github.com/microsoftgraph/msgraph-sdk-design/blob/main/tasks/PageIteratorTask.md
    */

    /// <summary>
    /// Use PageIterator&lt;TEntity&gt; to automatically page through result sets across multiple calls 
    /// and process each item in the result set.
    /// </summary>
    /// <typeparam name="TEntity">The Microsoft Graph entity type returned in the result set.</typeparam>
    /// <typeparam name="TCollectionPage">The Microsoft Graph collection response type returned in the collection response.</typeparam>
    public class PageIterator<TEntity, TCollectionPage> where TCollectionPage : IParsable,IAdditionalDataHolder,new()
    {
        private IRequestAdapter _requestAdapter;
        private TCollectionPage _currentPage;
        private Queue<TEntity> _pageItemQueue;
        private Func<TEntity, bool> _processPageItemCallback;
        private Func<TEntity, Task<bool>> _asyncProcessPageItemCallback;
        private Func<RequestInformation, RequestInformation> _requestConfigurator;
        private Dictionary<string, ParsableFactory<IParsable>> _errorMapping;

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
        /// Boolean value representing if the callback is Async
        /// </summary>
        internal bool IsProcessPageItemCallbackAsync => _processPageItemCallback == default;

        /// <summary>
        /// Creates the PageIterator with the results of an initial paged request. 
        /// </summary>
        /// <param name="client">The GraphServiceClient object used to execute the next request on paging </param>
        /// <param name="page">A generated implementation of ICollectionPage.</param>
        /// <param name="callback">A Func delegate that processes type TEntity in the result set and should return false if the iterator should cancel processing.</param>
        /// <param name="requestConfigurator">A Func delegate that configures the NextPageRequest</param>
        /// <param name="errorMapping">The error mappings to use in case of failed request during page iteration</param>
        /// <returns>A PageIterator&lt;TEntity&gt; that will process additional result pages based on the rules specified in Func&lt;TEntity,bool&gt; processPageItems</returns>
        public static PageIterator<TEntity, TCollectionPage> CreatePageIterator(IBaseClient client, TCollectionPage page, Func<TEntity, bool> callback, Func<RequestInformation, RequestInformation> requestConfigurator = null, Dictionary<string, ParsableFactory<IParsable>> errorMapping = null )
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            return CreatePageIterator(client.RequestAdapter, page, callback, requestConfigurator,errorMapping);
        }

        /// <summary>
        /// Creates the PageIterator with the results of an initial paged request. 
        /// </summary>
        /// <param name="requestAdapter">The <see cref="IRequestAdapter"/> object used to create the NextPageRequest for a delta query.</param>
        /// <param name="page">A generated implementation of ICollectionPage.</param>
        /// <param name="callback">A Func delegate that processes type TEntity in the result set and should return false if the iterator should cancel processing.</param>
        /// <param name="requestConfigurator">A Func delegate that configures the NextPageRequest</param>
        /// <param name="errorMapping">The error mappings to use in case of failed request during page iteration</param>
        /// <returns>A PageIterator&lt;TEntity&gt; that will process additional result pages based on the rules specified in Func&lt;TEntity,bool&gt; processPageItems</returns>
        public static PageIterator<TEntity, TCollectionPage> CreatePageIterator(IRequestAdapter requestAdapter, TCollectionPage page, Func<TEntity, bool> callback, Func<RequestInformation, RequestInformation> requestConfigurator = null,Dictionary<string, ParsableFactory<IParsable>> errorMapping = null)
        {
            if (requestAdapter == null)
                throw new ArgumentNullException(nameof(requestAdapter));

            if (page == null)
                throw new ArgumentNullException(nameof(page));

            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            if (!page.GetFieldDeserializers().ContainsKey("value"))
                throw new ArgumentException("The Parsable does not contain a collection property");

            var pageItems = ExtractEntityListFromParsable(page);

            return new PageIterator<TEntity, TCollectionPage>()
            {
                _requestAdapter = requestAdapter,
                _currentPage = page,
                _pageItemQueue = new Queue<TEntity>(pageItems),
                _processPageItemCallback = callback,
                _requestConfigurator = requestConfigurator,
                _errorMapping = errorMapping ?? new Dictionary<string, ParsableFactory<IParsable>>(StringComparer.OrdinalIgnoreCase) {
                    {"4XX", (parsable) => new ServiceException(ErrorConstants.Messages.PageIteratorRequestError,new Exception(GetErrorMessageFromParsable(parsable))) },
                    {"5XX", (parsable) => new ServiceException(ErrorConstants.Messages.PageIteratorRequestError,new Exception(GetErrorMessageFromParsable(parsable))) }
                },
                State = PagingState.NotStarted
            };
        }

        /// <summary>
        /// Creates the PageIterator with the results of an initial paged request. 
        /// </summary>
        /// <param name="client">The GraphServiceClient object used to create the NextPageRequest for a delta query.</param>
        /// <param name="page">A generated implementation of ICollectionPage.</param>
        /// <param name="asyncCallback">A Func delegate that processes type TEntity in the result set aynchrnously and should return false if the iterator should cancel processing.</param>
        /// <param name="requestConfigurator">A Func delegate that configures the NextPageRequest</param>
        /// <param name="errorMapping">The error mappings to use in case of failed request during page iteration</param>
        /// <returns>A PageIterator&lt;TEntity&gt; that will process additional result pages based on the rules specified in Func&lt;TEntity,bool&gt; processPageItems</returns>
        public static PageIterator<TEntity, TCollectionPage> CreatePageIterator(IBaseClient client, TCollectionPage page, Func<TEntity, Task<bool>> asyncCallback, Func<RequestInformation, RequestInformation> requestConfigurator = null,Dictionary<string, ParsableFactory<IParsable>> errorMapping = null)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            return CreatePageIterator(client.RequestAdapter, page, asyncCallback, requestConfigurator);
        }

        /// <summary>
        /// Creates the PageIterator with the results of an initial paged request. 
        /// </summary>
        /// <param name="requestAdapter">The <see cref="IRequestAdapter"/> object used to execute the next request on paging .</param>
        /// <param name="page">A generated implementation of ICollectionPage.</param>
        /// <param name="asyncCallback">A Func delegate that processes type TEntity in the result set aynchrnously and should return false if the iterator should cancel processing.</param>
        /// <param name="requestConfigurator">A Func delegate that configures the NextPageRequest</param>
        /// <param name="errorMapping">The error mappings to use in case of failed request during page iteration</param>
        /// <returns>A PageIterator&lt;TEntity&gt; that will process additional result pages based on the rules specified in Func&lt;TEntity,bool&gt; processPageItems</returns>
        public static PageIterator<TEntity, TCollectionPage> CreatePageIterator(IRequestAdapter requestAdapter, TCollectionPage page, Func<TEntity, Task<bool>> asyncCallback, Func<RequestInformation, RequestInformation> requestConfigurator = null,Dictionary<string, ParsableFactory<IParsable>> errorMapping = null)
        {
            if (requestAdapter == null)
                throw new ArgumentNullException(nameof(requestAdapter));

            if (page == null)
                throw new ArgumentNullException(nameof(page));

            if (asyncCallback == null)
                throw new ArgumentNullException(nameof(asyncCallback));

            if (!page.GetFieldDeserializers().ContainsKey("value"))
                throw new ArgumentException("The Parsable does not contain a collection property");

            var pageItems = ExtractEntityListFromParsable(page);

            return new PageIterator<TEntity, TCollectionPage>()
            {
                _requestAdapter = requestAdapter,
                _currentPage = page,
                _pageItemQueue = new Queue<TEntity>(pageItems),
                _asyncProcessPageItemCallback = asyncCallback,
                _requestConfigurator = requestConfigurator,
                _errorMapping = errorMapping ?? new Dictionary<string, ParsableFactory<IParsable>>(StringComparer.OrdinalIgnoreCase) {
                    {"4XX", (parsable) => new ServiceException(ErrorConstants.Messages.PageIteratorRequestError,new Exception(GetErrorMessageFromParsable(parsable))) },
                    {"5XX", (parsable) =>new ServiceException(ErrorConstants.Messages.PageIteratorRequestError,new Exception(GetErrorMessageFromParsable(parsable))) },
                },
                State = PagingState.NotStarted
            };
        }

        /// <summary>
        /// Iterate across the content of a a single results page with the callback.
        /// </summary>
        /// <returns>A boolean value that indicates whether the callback cancelled 
        /// iterating across the page results or whether there are more pages to page. 
        /// A return value of false indicates that the iterator should stop iterating.</returns>
        private async Task<bool> IntrapageIterateAsync()
        {
            State = PagingState.IntrapageIteration;

            while (_pageItemQueue.Count != 0) // && shouldContinue)
            {
                bool shouldContinue = IsProcessPageItemCallbackAsync ? await _asyncProcessPageItemCallback(_pageItemQueue.Dequeue()) : _processPageItemCallback(_pageItemQueue.Dequeue());

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
            if (_currentPage.AdditionalData != null && _currentPage.AdditionalData.TryGetValue(CoreConstants.OdataInstanceAnnotations.DeltaLink, out object deltalink))
            {
                Deltalink = deltalink.ToString();
                State = PagingState.Delta;
                Nextlink = string.Empty;

                return false;
            }
            var deltaLink = ExtractNextLinkFromParsable(_currentPage, "OdataDeltaLink");
            // There are no pages CURRENTLY ready to be paged. Attempt to call delta query later.
            if (!string.IsNullOrEmpty(deltaLink))
            {
                Deltalink = deltaLink;
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
                    HttpMethod = Method.GET,
                    UrlTemplate = string.IsNullOrEmpty(Nextlink) ? Deltalink : Nextlink,
                };
                // if we have a request configurator, modify the request as desired then execute it to get the next page
                nextPageRequestInformation = _requestConfigurator == null ? nextPageRequestInformation : _requestConfigurator(nextPageRequestInformation);
                _currentPage = await _requestAdapter.SendAsync<TCollectionPage>(nextPageRequestInformation, (parseNode) => new TCollectionPage(), _errorMapping, token);

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
            if (!string.IsNullOrEmpty(Nextlink) && Nextlink.Equals(ExtractNextLinkFromParsable(_currentPage)))
            {
                throw new ServiceException($"Detected nextLink loop. Nextlink value: {Nextlink}");
            }
        }
#pragma warning disable CS1574
#pragma warning disable CS1587
        /// <summary>
        /// Fetches page collections and iterates through each page of items and processes it according to the Func&lt;TEntity, bool&gt; set in <see cref="CreatePageIterator"/>. 
        /// </summary>
#pragma warning restore CS1587
#pragma warning restore CS1574
        /// <returns>The task object that represents the results of this asynchronous operation.</returns>
        /// <exception cref="Microsoft.Graph.ServiceException">Thrown when the service encounters an error with
        /// a request.</exception>
        public async Task IterateAsync()
        {
            await IterateAsync(new CancellationToken()).ConfigureAwait(false);
        }

#pragma warning disable CS1574
#pragma warning disable CS1587
        /// <summary>
        /// Fetches page collections and iterates through each page of items and processes it according to the Func&lt;TEntity, bool&gt; set in <see cref="CreatePageIterator"/>. 
        /// </summary>
#pragma warning restore CS1587
#pragma warning restore CS1574
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
                await InterpageIterateAsync(token).ConfigureAwait(false);
            }

            // Iterate over the contents of queue. The queue could be from the initial page
            // results passed to the iterator, the results of a delta query, or from a 
            // previously cancelled iteration that gets resumed.
            bool shouldContinueInterpageIteration = await IntrapageIterateAsync();

            // Request more pages if they are available.
            while (shouldContinueInterpageIteration && !token.IsCancellationRequested)
            {
                // Make a call to get the next page of results and add items to queue. 
                await InterpageIterateAsync(token).ConfigureAwait(false);

                // Iterate over items added to the queue by InterpageIterateAsync and
                // determine whether there are more pages to request.
                shouldContinueInterpageIteration = await IntrapageIterateAsync();
            }
        }

#pragma warning disable CS1574
#pragma warning disable CS1587
        /// <summary>
        /// Resumes iterating through each page of items and processes it according to the Func&lt;TEntity, bool&gt; set in <see cref="CreatePageIterator"/>. 
        /// </summary>
#pragma warning restore CS1587
#pragma warning restore CS1574
        /// <returns>The task object that represents the results of this asynchronous operation.</returns>
        public async Task ResumeAsync()
        {
            await ResumeAsync(new CancellationToken()).ConfigureAwait(false);
        }

#pragma warning disable CS1574
#pragma warning disable CS1587
        /// <summary>
        /// Resumes iterating through each page of items and processes it according to the Func&lt;TEntity, bool&gt; set in <see cref="CreatePageIterator"/>. 
        /// </summary>
#pragma warning restore CS1574
#pragma warning restore CS1587
        /// <param name="token">The CancellationToken used to stop iterating calls for more pages.</param>
        /// <returns>The task object that represents the results of this asynchronous operation.</returns>
        /// <exception cref="Microsoft.Graph.ServiceException">Thrown when the service encounters an error with
        /// a request.</exception>
        public async Task ResumeAsync(CancellationToken token)
        {
            await IterateAsync(token).ConfigureAwait(false);
        }

        /// <summary>
        /// Helper method to extract the collection rom an <see cref="IParsable"/> instance.
        /// </summary>
        /// <param name="parsableCollection">The <see cref="IParsable"/> to extract the collection from</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Thrown when the object doesn't contain a collection inside it</exception>
        private static List<TEntity> ExtractEntityListFromParsable(TCollectionPage parsableCollection)
        {
            return parsableCollection.GetType().GetProperty("Value")?.GetValue(parsableCollection, null) as List<TEntity> ?? throw new ArgumentException("The Parsable does not contain a collection property");
        }

        /// <summary>
        /// Helper method to extract the nextLink property from an <see cref="IParsable"/> instance.
        /// </summary>
        /// <param name="parsableCollection">The <see cref="IParsable"/> to extract the nextLink from</param>
        /// <param name="nextLinkPropertyName">The property name of the nextLink string</param>
        /// <returns></returns>
        private static string ExtractNextLinkFromParsable(TCollectionPage parsableCollection, string nextLinkPropertyName = "OdataNextLink")
        {
            var nextLinkProperty = parsableCollection.GetType().GetProperty(nextLinkPropertyName);
            if (nextLinkProperty != null && 
                nextLinkProperty.GetValue(parsableCollection, null) is string nextLinkString  
                && !string.IsNullOrEmpty(nextLinkString))
            {
                return nextLinkString;
            }
            
            // the next link property may not be defined in the response schema so we also check its presence in the additional data bag
            return parsableCollection.AdditionalData.TryGetValue(CoreConstants.OdataInstanceAnnotations.NextLink,out var nextLink) ? nextLink.ToString() : string.Empty;
        }
        
        private static string GetErrorMessageFromParsable(IParseNode responseParseNode)
        {
            var errorParseNode = responseParseNode.GetChildNode("error");
            // concatenate the error code and message
            return $"{errorParseNode?.GetChildNode("code")?.GetStringValue()} : {errorParseNode?.GetChildNode("message")?.GetStringValue()}";
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
