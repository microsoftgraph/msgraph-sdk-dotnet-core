// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net.Http.Headers;
    using System.IO.Compression;

    public class CompressionHandler: DelegatingHandler
    {
        internal CompressionHandlerOption CompressionOptions { get; set; }
        public CompressionHandler(CompressionHandlerOption compressionHandlerOption = null)
        {
            CompressionOptions = compressionHandlerOption ?? new CompressionHandlerOption();
        }

        public CompressionHandler(HttpMessageHandler innerHandler, CompressionHandlerOption compressionHandlerOption = null)
            :this(compressionHandlerOption)
        {
            InnerHandler = innerHandler;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            StringWithQualityHeaderValue gzipQHeaderValue = new StringWithQualityHeaderValue("gzip");

            // Add Accept-encoding: gzip header to incoming request if it doesn't have one.
            if (!request.Headers.AcceptEncoding.Contains(gzipQHeaderValue))
            {
                request.Headers.AcceptEncoding.Add(gzipQHeaderValue);
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            // Decompress response content when Content-Encoding: gzip header is present.
            if (IsDecompressContent(response) && CompressionOptions.ShouldDecompressResponseContent(response))
            {
                response.Content = new StreamContent(new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress));
            }

            return response;
        }

        private bool IsDecompressContent(HttpResponseMessage response)
        {
            return response.Content != null && response.Content.Headers.ContentEncoding.Contains("gzip");
        }
    }
}
