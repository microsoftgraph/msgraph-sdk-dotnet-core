// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Extensions
{
    using Microsoft.Graph.DotnetCore.Core.Test.Requests;
    using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Xunit;
    public class HttpRequestMessageExtensionsTests
    {
        [Fact]
        public async Task CloneAsync_WithEmptyHttpContent()
        {
            HttpRequestMessage originalRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com");

            HttpRequestMessage clonedRequest = await originalRequest.CloneAsync();

            Assert.NotNull(clonedRequest);
            Assert.Equal(originalRequest.Method, clonedRequest.Method);
            Assert.Equal(originalRequest.RequestUri, clonedRequest.RequestUri);
            Assert.Null(clonedRequest.Content);
        }

        [Fact]
        public async Task CloneAsync_WithHttpContent()
        {
            HttpRequestMessage originalRequest = new HttpRequestMessage(HttpMethod.Post, "http://example.com");
            originalRequest.Content = new StringContent("Sample Content", Encoding.UTF8, CoreConstants.MimeTypeNames.Application.Json);

            HttpRequestMessage clonedRequest = await originalRequest.CloneAsync();

            Assert.NotNull(clonedRequest);
            Assert.Equal(originalRequest.Method, clonedRequest.Method);
            Assert.Equal(originalRequest.RequestUri, clonedRequest.RequestUri);
            Assert.Equal(await originalRequest.Content.ReadAsStringAsync(), await clonedRequest.Content.ReadAsStringAsync());
            Assert.Equal(originalRequest.Content.Headers.ContentType, clonedRequest.Content.Headers.ContentType);
        }
    }
}
