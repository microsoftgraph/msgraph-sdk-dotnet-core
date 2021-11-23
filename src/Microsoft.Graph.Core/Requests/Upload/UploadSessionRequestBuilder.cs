// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Graph.Core.Models;
    using Microsoft.Kiota.Abstractions;
    using System.Net.Http;
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
        /// <returns>The task to await.</returns>
        public async Task DeleteAsync()
        {
            var requestInformation = this.CreateDeleteRequestInformationAsync();
            await this.requestAdapter.SendNoContentAsync(requestInformation);
        }

        /// <summary>
        /// Creates <see cref="RequestInformation"/> instance for a DELETE request
        /// </summary>
        /// <returns>The <see cref="RequestInformation"/> instance.</returns>
        public RequestInformation CreateDeleteRequestInformationAsync()
        {
            var requestInfo = new RequestInformation
            {
                HttpMethod = Kiota.Abstractions.HttpMethod.DELETE,
                UrlTemplate = urlTemplate,
            };
            return requestInfo;
        }

        /// <summary>
        /// Gets the specified UploadSession.
        /// </summary>
        /// <returns>The Item.</returns>
        public async Task<IUploadSession> GetAsync()
        {
            var requestInformation = this.CreateGetRequestInformationAsync();
            var responseHandler = new NativeResponseHandler();
            await this.requestAdapter.SendNoContentAsync(requestInformation, responseHandler);
            var uploadResult = await this.responseHandler.HandleResponse<UploadSession>(responseHandler.Value as HttpResponseMessage);
            return uploadResult.UploadSession;
        }

        /// <summary>
        /// Creates <see cref="RequestInformation"/> instance for a GET request
        /// </summary>
        /// <returns>The <see cref="RequestInformation"/> instance.</returns>
        public RequestInformation CreateGetRequestInformationAsync()
        {
            var requestInfo = new RequestInformation
            {
                HttpMethod = Kiota.Abstractions.HttpMethod.GET,
                UrlTemplate = urlTemplate,
            };
            return requestInfo;
        }
    }
}
