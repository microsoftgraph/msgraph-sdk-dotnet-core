// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    internal class UploadSliceRequestBuilder<T>
    {
        private readonly UploadResponseHandler ResponseHandler;
        private readonly IRequestAdapter RequestAdapter;
        private readonly string UrlTemplate;
        /// <summary>
        /// The beginning of the slice range to send.
        /// </summary>
        public long RangeBegin { get; private set; }

        /// <summary>
        /// The end of the slice range to send.
        /// </summary>
        public long RangeEnd { get; private set; }

        /// <summary>
        /// The length in bytes of the session.
        /// </summary>
        public long TotalSessionLength { get; private set; }

        /// <summary>
        /// The range length of the slice to send.
        /// </summary>
        public int RangeLength => (int)(this.RangeEnd - this.RangeBegin + 1);

        /// <summary>
        /// Request for uploading one slice of a session
        /// </summary>
        /// <param name="sessionUrl">URL to upload the slice.</param>
        /// <param name="requestAdapter">Client used for sending the slice.</param>
        /// <param name="rangeBegin">Beginning of range of this slice</param>
        /// <param name="rangeEnd">End of range of this slice</param>
        /// <param name="totalSessionLength">Total session length. This MUST be consistent
        /// across all slice.</param>
        public UploadSliceRequestBuilder(
            string sessionUrl,
            IRequestAdapter requestAdapter,
            long rangeBegin,
            long rangeEnd,
            long totalSessionLength)
        {
            this.UrlTemplate = sessionUrl ?? throw new ArgumentNullException(nameof(requestAdapter));
            this.RequestAdapter = requestAdapter ?? throw new ArgumentNullException(nameof(requestAdapter));
            this.RangeBegin = rangeBegin;
            this.RangeEnd = rangeEnd;
            this.TotalSessionLength = totalSessionLength;
            this.ResponseHandler = new UploadResponseHandler();
        }

        /// <summary>
        /// Uploads the slice using PUT.
        /// </summary>
        /// <param name="stream">Stream of data to be sent in the request. Length must be equal to the length
        /// of this slice (as defined by this.RangeLength)</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to use for cancelling requests</param>
        /// <returns>The status of the upload. If UploadSession.AdditionalData.ContainsKey("successResponse")
        /// is true, then the item has completed, and the value is the created item from the server.</returns>
        public async Task<UploadResult<T>> PutAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var requestInformation = this.CreatePutRequestInformationAsync(stream);
            var responseHandler = new NativeResponseHandler();
            await this.RequestAdapter.SendNoContentAsync(requestInformation, responseHandler, cancellationToken);
            return await this.ResponseHandler.HandleResponse<T>(responseHandler.Value as HttpResponseMessage);
        }

        /// <summary>
        /// Create <see cref="RequestInformation"/> instance to upload the file slice
        /// <param name="stream">The <see cref="Stream"/> to upload</param>
        /// </summary>
        public RequestInformation CreatePutRequestInformationAsync(Stream stream)
        {
            _ = stream ?? throw new ArgumentNullException(nameof(stream));
            var requestInfo = new RequestInformation
            {
                HttpMethod = Kiota.Abstractions.HttpMethod.PUT,
                UrlTemplate = UrlTemplate,
            };
            requestInfo.SetStreamContent(stream);
            requestInfo.Headers.Add("Content-Range", $"bytes {this.RangeBegin}-{this.RangeEnd}/{this.TotalSessionLength}");
            requestInfo.Headers.Add("Content-Length", $"{this.RangeLength}");
            return requestInfo;
        }
    }
}
