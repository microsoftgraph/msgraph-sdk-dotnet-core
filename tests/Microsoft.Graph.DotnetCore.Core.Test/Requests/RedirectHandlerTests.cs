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
    public class RedirectHandlerTests : IDisposable
    {
        private MockRedirectHander testHttpMessageHandler;
        private RedirectHandler redirectHandler;
        private HttpMessageInvoker invoker;
        

        public RedirectHandlerTests()
        {
            this.testHttpMessageHandler = new MockRedirectHander();
            this.redirectHandler = new RedirectHandler(this.testHttpMessageHandler);
            this.invoker = new HttpMessageInvoker(this.redirectHandler);
            
        }

        public void Dispose()
        {
            this.invoker.Dispose();
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
            this.testHttpMessageHandler.SetHttpResponse(redirectResponse);

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
            redirectResponse.Headers.Location = new Uri("http://example.org/bar");

            this.testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.Equal(response.RequestMessage.Method, httpRequestMessage.Method);
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.Equal(response.RequestMessage.Content.ReadAsStringAsync().Result, "Hello World");

        }

        [Fact]
        public async Task ShouldRedirectChangeMethodAndContent()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo");
            httpRequestMessage.Content = new StringContent("Hello World");

            var redirectResponse = new HttpResponseMessage(HttpStatusCode.SeeOther);
            redirectResponse.Headers.Location = new Uri("http://example.org/bar");

            this.testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.NotEqual(response.RequestMessage.Method, httpRequestMessage.Method);
            Assert.Equal(response.RequestMessage.Method, HttpMethod.Get);
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.Null(response.RequestMessage.Content);
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

            this.testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.NotSame(response.RequestMessage.RequestUri.Host, httpRequestMessage.RequestUri.Host);
            Assert.Null(response.RequestMessage.Headers.Authorization);

        }

        [Fact]
        public async Task RedirectWithSameHostShouldKeepAuthHeader()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("fooAuth", "aparam");

            var redirectResponse = new HttpResponseMessage(HttpStatusCode.Redirect);
            redirectResponse.Headers.Location = new Uri("http://example.org/bar");

            this.testHttpMessageHandler.SetHttpResponse(redirectResponse, new HttpResponseMessage(HttpStatusCode.OK));

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());
            Console.WriteLine(response.RequestMessage.RequestUri.Host);
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.Equal(response.RequestMessage.RequestUri.Host, httpRequestMessage.RequestUri.Host);
            Assert.NotNull(response.RequestMessage.Headers.Authorization);
        }

        [Fact]
        public async Task ExceedMaxRedirectsShouldThrowsException()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo");

            var _response1 = new HttpResponseMessage(HttpStatusCode.Redirect);
            _response1.Headers.Location = new Uri("http://example.org/bar");

            var _response2 = new HttpResponseMessage(HttpStatusCode.Redirect);
            _response2.Headers.Location = new Uri("http://example.org/foo");

            this.testHttpMessageHandler.SetHttpResponse(_response1, _response2);

            try
            {
                await Assert.ThrowsAsync<ServiceException>(async () => await this.invoker.SendAsync(
                    httpRequestMessage, CancellationToken.None));
            }
            catch (ServiceException exception)
            {
                Assert.True(exception.IsMatch(ErrorConstants.Codes.TooManyRedirects));
                Assert.Equal(ErrorConstants.Messages.TooManyRedirectsFormatString, exception.Error.Message);
                Assert.IsType(typeof(ServiceException), exception);
            }

        }


    }
}
