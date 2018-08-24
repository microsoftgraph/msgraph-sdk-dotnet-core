// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
using Microsoft.Graph;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    public class RedirectHandlerTests
    {
        private TestHttpMessageHandler testHttpMessageHandler;
        private RedirectHandler redirectHandler;
        private HttpMessageInvoker invoker;
        

        public RedirectHandlerTests()
        {
            this.testHttpMessageHandler = new TestHttpMessageHandler();
            this.redirectHandler = new RedirectHandler(this.testHttpMessageHandler);
            this.invoker = new HttpMessageInvoker(this.redirectHandler);
        }

        [Fact]
        public void RedirectHandler_HttpMessageHandlerConstructor()
        {
            Assert.NotNull(this.redirectHandler.InnerHandler);
            Assert.Equal(this.redirectHandler.InnerHandler, this.testHttpMessageHandler);
            Assert.IsType(typeof(RedirectHandler), this.redirectHandler);
        }

        [Fact]
        public async Task OkStatusShouldPassThrough()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");

            var redirectResponse = new HttpResponseMessage(HttpStatusCode.OK);
            this.testHttpMessageHandler.AddResponseMapping("http://example.org/foo", redirectResponse);

            var response =await this.invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.Equal(response.StatusCode, HttpStatusCode.OK);
            Assert.Same(response.RequestMessage, httpRequestMessage);
        }

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        public async Task ShouldRedirectSameMethodAndContent(HttpStatusCode statusCode)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo");
            httpRequestMessage.Content = new StringContent("Hello World");


            var redirectResponse = new HttpResponseMessage(statusCode);
            redirectResponse.Headers.Location = new Uri("http://example.net/bar");

            this.testHttpMessageHandler.AddResponseMapping("http://example.org/foo", redirectResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.NotSame(response.RequestMessage.RequestUri.Host, httpRequestMessage.RequestUri.Host);
            Assert.Null(response.RequestMessage.Headers.Authorization);

        }

        [Theory]
        [InlineData(HttpStatusCode.MovedPermanently)]  // 301
        [InlineData(HttpStatusCode.Found)]  // 302
        [InlineData(HttpStatusCode.TemporaryRedirect)]  // 307
        public async Task RedirectWithDifferentHostShouldRemoveAuthHeader(HttpStatusCode statusCode)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("fooAuth", "aparam");

            var redirectResponse = new HttpResponseMessage(statusCode);
            redirectResponse.Headers.Location = new Uri("http://example.net/bar");

            this.testHttpMessageHandler.AddResponseMapping("http://example.org/foo", redirectResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.NotSame(response.RequestMessage.RequestUri.Host, httpRequestMessage.RequestUri.Host);
            Assert.Null(response.RequestMessage.Headers.Authorization);

        }

       
    }
}
