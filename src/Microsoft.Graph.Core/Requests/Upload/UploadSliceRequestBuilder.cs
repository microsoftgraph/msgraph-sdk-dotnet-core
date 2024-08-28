// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Serialization;

    internal class UploadSliceRequestBuilder<T> where T : IParsable, new()
    {
        private readonly UploadResponseHandler ResponseHandler;
        private readonly IRequestAdapter RequestAdapter;
        private readonly string UrlTemplate;
        /// <summary>
        /// The beginning of the slice range to send.
        /// </summary>
        public long RangeBegin
        {
            get; private set;
        }

        /// <summary>
        /// The end of the slice range to send.
        /// </summary>
        public long RangeEnd
        {
            get; private set;
        }

        /// <summary>
        /// The length in bytes of the session.
        /// </summary>
        public long TotalSessionLength
        {
            get; private set;
        }

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
            var requestInformation = this.CreatePutRequestInformation(stream);
            var nativeResponseHandler = new NativeResponseHandler();
            requestInformation.SetResponseHandler(nativeResponseHandler);
            await this.RequestAdapter.SendNoContentAsync(requestInformation, cancellationToken: cancellationToken).ConfigureAwait(false);
            return await this.ResponseHandler.HandleResponseAsync<T>(nativeResponseHandler.Value as HttpResponseMessage).ConfigureAwait(false);
        }

        /// <summary>
        /// Create <see cref="RequestInformation"/> instance to upload the file slice
        /// <param name="stream">The <see cref="Stream"/> to upload</param>
        /// </summary>
        [Obsolete("Use CreatePutRequestInformation instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        public RequestInformation CreatePutRequestInformationAsync(Stream stream) => CreatePutRequestInformation(stream);
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods

        /// <summary>
        /// Create <see cref="RequestInformation"/> instance to upload the file slice
        /// <param name="stream">The <see cref="Stream"/> to upload</param>
        /// </summary>
        public RequestInformation CreatePutRequestInformation(Stream stream)
        {
            _ = stream ?? throw new ArgumentNullException(nameof(stream));
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.PUT,
                UrlTemplate = UrlTemplate,
            };
            requestInfo.SetStreamContent(stream, binaryContentType);
            requestInfo.Headers.Add("Content-Range", $"bytes {this.RangeBegin}-{this.RangeEnd}/{this.TotalSessionLength}");
            requestInfo.Headers.Add("Content-Length", $"{this.RangeLength}");
            return requestInfo;
        }
        private const string binaryContentType = "application/octet-stream";
    }
}
