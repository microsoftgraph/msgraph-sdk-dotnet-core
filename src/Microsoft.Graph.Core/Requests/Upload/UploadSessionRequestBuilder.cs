// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Graph.Core.Models;
    using Microsoft.Kiota.Abstractions;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    internal class UploadSessionRequestBuilder
    {
        private readonly UploadResponseHandler responseHandler;
        private readonly IRequestAdapter requestAdapter;
        private readonly string urlTemplate;

        /// <summary>
        /// Create a new UploadSessionRequest
        /// </summary>
        /// <param name="uploadSession">The IUploadSession to use in the request.</param>
        /// <param name="requestAdapter">The <see cref="IRequestAdapter"/> for handling requests.</param>
        public UploadSessionRequestBuilder(IUploadSession uploadSession, IRequestAdapter requestAdapter)
        {
            this.responseHandler = new UploadResponseHandler();
            this.requestAdapter = requestAdapter;
            this.urlTemplate = uploadSession.UploadUrl;
        }

        /// <summary>
        /// Deletes the specified Session
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to use for cancelling requests</param>
        /// <returns>The task to await.</returns>
        public async Task DeleteAsync(CancellationToken cancellationToken = default)
        {
            var requestInformation = this.ToDeleteRequestInformation();
            await this.requestAdapter.SendNoContentAsync(requestInformation, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Creates <see cref="RequestInformation"/> instance for a DELETE request
        /// </summary>
        /// <returns>The <see cref="RequestInformation"/> instance.</returns>
        public RequestInformation ToDeleteRequestInformation()
        {
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.DELETE,
                UrlTemplate = urlTemplate,
            };
            return requestInfo;
        }

        /// <summary>
        /// Gets the specified UploadSession.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to use for cancelling requests</param>
        /// <returns>The Item.</returns>
        public async Task<IUploadSession> GetAsync(CancellationToken cancellationToken = default)
        {
            var requestInformation = this.ToGetRequestInformation();
            var nativeResponseHandler = new NativeResponseHandler();
            requestInformation.SetResponseHandler(nativeResponseHandler);
            await this.requestAdapter.SendNoContentAsync(requestInformation, cancellationToken: cancellationToken).ConfigureAwait(false);
            var uploadResult = await this.responseHandler.HandleResponse<UploadSession>(nativeResponseHandler.Value as HttpResponseMessage).ConfigureAwait(false);
            return uploadResult.UploadSession;
        }

        /// <summary>
        /// Creates <see cref="RequestInformation"/> instance for a GET request
        /// </summary>
        /// <returns>The <see cref="RequestInformation"/> instance.</returns>
        public RequestInformation ToGetRequestInformation()
        {
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                UrlTemplate = urlTemplate,
            };
            return requestInfo;
        }
    }
}
