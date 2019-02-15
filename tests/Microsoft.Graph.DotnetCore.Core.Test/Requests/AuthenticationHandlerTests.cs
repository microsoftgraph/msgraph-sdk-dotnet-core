// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using Xunit;
    using System.Threading.Tasks;

    public class AuthenticationHandlerTests : IDisposable
    {
        private MockRedirectHandler testHttpMessageHandler;
        private AuthenticationHandler authenticationHandler;
        private MockAuthenticationProvider mockAuthenticationProvider;
        private HttpMessageInvoker invoker;

        public AuthenticationHandlerTests()
        {
            testHttpMessageHandler = new MockRedirectHandler();
            mockAuthenticationProvider = new MockAuthenticationProvider();
            authenticationHandler = new AuthenticationHandler(mockAuthenticationProvider.Object, testHttpMessageHandler);
            invoker = new HttpMessageInvoker(authenticationHandler);
        }

        public void Dispose()
        {
            invoker.Dispose();
        }

        [Fact]
        public void AuthHandler_AuthProviderConstructor()
        {
            using (AuthenticationHandler auth = new AuthenticationHandler(mockAuthenticationProvider.Object))
            {
                Assert.Null(auth.InnerHandler);
                Assert.NotNull(auth.AuthenticationProvider);
                Assert.NotNull(auth.AuthOption);
                Assert.IsType(typeof(AuthenticationHandler), auth);
            }
        }

        [Fact]
        public void AuthHandler_AuthProviderHttpMessageHandlerConstructor()
        {
            Assert.NotNull(authenticationHandler.InnerHandler);
            Assert.NotNull(authenticationHandler.AuthenticationProvider);
            Assert.NotNull(authenticationHandler.AuthOption);
            Assert.IsType(typeof(AuthenticationHandler), authenticationHandler);
        }

        [Fact]
        public void AuthHandler_AuthProviderAuthOptionConstructor()
        {
            var scopes = new string[] { "foo.bar" };
            using (AuthenticationHandler auth = new AuthenticationHandler(mockAuthenticationProvider.Object,
                new AuthenticationHandlerOption()))
            {
                Assert.Null(auth.InnerHandler);
                Assert.NotNull(auth.AuthenticationProvider);
                Assert.NotNull(auth.AuthOption);
                Assert.IsType(typeof(AuthenticationHandler), auth);
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.MovedPermanently)]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.GatewayTimeout)]
        public async Task AuthHandler_NonUnauthorizedStatusShouldPassThrough(HttpStatusCode statusCode)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            var expectedResponse = new HttpResponseMessage(statusCode);

            testHttpMessageHandler.SetHttpResponse(expectedResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.Same(response, expectedResponse);
            Assert.Same(response.RequestMessage, httpRequestMessage);
        }

        [Fact]
        public async Task AuthHandler_ShouldRetryUnauthorizedGetRequest()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.com/bar");
            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, expectedResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.Same(response, expectedResponse);
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.Null(response.RequestMessage.Content);
        }

        [Fact]
        public async Task AuthHandler_ShouldRetryUnauthorizedPostRequestWithNoContent()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.com/bar");
            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, expectedResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.Same(response, expectedResponse);
            Assert.NotSame(response, unauthorizedResponse);
            Assert.Null(response.RequestMessage.Content);
        }

        [Fact]
        public async Task AuthHandler_ShouldRetryUnauthorizedPostRequestWithBufferContent()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.com/bar");
            httpRequestMessage.Content = new StringContent("Hello World!");

            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var okResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, okResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.Same(response, okResponse);
            Assert.NotSame(response, unauthorizedResponse);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.Equal(response.RequestMessage.Content.ReadAsStringAsync().Result, "Hello World!");
        }

        [Fact]
        public async Task AuthHandler_ShouldRetryUnauthorizedPatchRequestWithBufferContent()
        {
            var httpRequestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), "http://example.com/bar");
            httpRequestMessage.Content = new StringContent("Hello World!");

            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var okResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, okResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.Same(response, okResponse);
            Assert.NotSame(response, unauthorizedResponse);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.Equal(response.RequestMessage.Content.ReadAsStringAsync().Result, "Hello World!");
        }

        [Fact]
        public async Task AuthHandler_ShouldNotRetryUnauthorizedPutRequestWithStreamContent()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, "http://example.com/bar");
            httpRequestMessage.Content = new StringContent("Jambo");
            httpRequestMessage.Content.Headers.ContentLength = -1;

            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var okResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, okResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.Same(response.RequestMessage, httpRequestMessage);
            Assert.NotSame(response, okResponse);
            Assert.Same(response, unauthorizedResponse);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.Equal(response.RequestMessage.Content.Headers.ContentLength, -1);
        }

        [Fact]
        public async Task AuthHandler_ShouldNotRetryUnauthorizedPatchRequestWithStreamContent()
        {
            var httpRequestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), "http://example.com/bar");
            httpRequestMessage.Content = new StringContent("Jambo");
            httpRequestMessage.Content.Headers.ContentLength = -1;

            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var okResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, okResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.Same(response.RequestMessage, httpRequestMessage);
            Assert.NotSame(response, okResponse);
            Assert.Same(response, unauthorizedResponse);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.Equal(response.RequestMessage.Content.Headers.ContentLength, -1);
        }

        [Fact]
        public async Task AuthHandler_ShouldReturnUnauthorizedRequestWithDefaultMaxRetryExceeded()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, "http://example.com/bar");
            httpRequestMessage.Content = new StringContent("Hello Mars!");

            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, expectedResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.Same(response, expectedResponse);
            Assert.Equal(response.RequestMessage.Content.ReadAsStringAsync().Result, "Hello Mars!");
        }
    }
}
