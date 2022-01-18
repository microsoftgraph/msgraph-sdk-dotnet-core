// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Requests
{
    using Microsoft.Kiota.Abstractions;
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The type BatchRequestBuilder
    /// </summary>
    public class BatchRequestBuilder
    {
        /// <summary>
        /// Constructs a new BatchRequestBuilder.
        /// </summary>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public BatchRequestBuilder(IRequestAdapter requestAdapter)
        {
            _ = requestAdapter ?? throw new ArgumentNullException(nameof(requestAdapter));
            UrlTemplate = "{+baseurl}/$batch";
            RequestAdapter = requestAdapter;
        }

        /// <summary>
        /// Url template to use to build the URL for the current request builder
        /// </summary>
        internal string UrlTemplate { get; set; }

        /// <summary>
        /// The request adapter to use to execute the requests.
        /// </summary>
        internal IRequestAdapter RequestAdapter { get; set; }

        /// <summary>
        /// Sends out the <see cref="BatchRequestContent"/> using the POST method
        /// </summary>
        /// <param name="batchRequestContent">The <see cref="BatchRequestContent"/> for the request</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to use for cancelling requests</param>
        /// <returns></returns>
        public async Task<BatchResponseContent> PostAsync(BatchRequestContent batchRequestContent, CancellationToken cancellationToken = default)
        {
            _ = batchRequestContent ?? throw new ArgumentNullException(nameof(batchRequestContent));
            var requestInfo = await CreatePostRequestInformationAsync(batchRequestContent);
            var responseHandler = new NativeResponseHandler();
            await this.RequestAdapter.SendNoContentAsync(requestInfo, responseHandler, cancellationToken);
            return new BatchResponseContent(responseHandler.Value as HttpResponseMessage);
        }

        /// <summary>
        /// Create <see cref="RequestInformation"/> instance to post to batch endpoint
        /// <param name="batchRequestContent">The <see cref="BatchRequestContent"/> for the request</param>
        /// </summary>
        public async Task<RequestInformation> CreatePostRequestInformationAsync(BatchRequestContent batchRequestContent)
        {
            _ = batchRequestContent ?? throw new ArgumentNullException(nameof(batchRequestContent));
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.POST,
                UrlTemplate = UrlTemplate,
            };
            requestInfo.Content = await batchRequestContent.GetBatchRequestContentAsync();
            requestInfo.Headers.Add("Content-Type", CoreConstants.MimeTypeNames.Application.Json);
            return requestInfo;
        }
    }
}