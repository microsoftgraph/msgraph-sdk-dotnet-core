// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Test.Requests
{
    using Microsoft.Graph.Core.Test.Mocks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Threading;

    [TestClass]
    public class AuthenticationHandlerTests
    {
        private MockRedirectHandler testHttpMessageHandler;
        private AuthenticationHandler authenticationHandler;
        private MockAuthenticationProvider mockAuthenticationProvider;
        private HttpMessageInvoker invoker;

        [TestInitialize]
        public void Setup()
        {
            mockAuthenticationProvider = new MockAuthenticationProvider();
            testHttpMessageHandler = new MockRedirectHandler();
            authenticationHandler = new AuthenticationHandler(mockAuthenticationProvider.Object, testHttpMessageHandler);
            invoker = new HttpMessageInvoker(authenticationHandler);
        }

        [TestCleanup]
        public void TearDown()
        {
            invoker.Dispose();
        }

        [TestMethod]
        public void AuthHandler_AuthProviderConstructor()
        {
            using (AuthenticationHandler auth = new AuthenticationHandler(mockAuthenticationProvider.Object))
            {
                Assert.IsNull(auth.InnerHandler, "Http message handler initialized");
                Assert.IsNotNull(auth.AuthenticationProvider, "Authentication provider initialized");
                Assert.IsInstanceOfType(auth, typeof(AuthenticationHandler), "Unexpected authentication handler set");
            }
        }

        [TestMethod]
        public void AuthHandler_AuthProviderHttpMessageHandlerConstructor()
        {
            Assert.IsNotNull(authenticationHandler.InnerHandler, "Http message handler not initialized");
            Assert.IsNotNull(authenticationHandler.AuthenticationProvider, "Authentication provider not initialized");
            Assert.AreEqual(authenticationHandler.InnerHandler, testHttpMessageHandler, "Unexpected http message handler set");
            Assert.AreEqual(authenticationHandler.AuthenticationProvider, mockAuthenticationProvider.Object, "Unexpected auhtentication provider set");
            Assert.IsInstanceOfType(authenticationHandler, typeof(AuthenticationHandler), "Unexpected authentication handler set");
        }

        [DataTestMethod]
        [DataRow(HttpStatusCode.OK)]
        [DataRow(HttpStatusCode.MovedPermanently)]
        [DataRow(HttpStatusCode.NotFound)]
        [DataRow(HttpStatusCode.BadRequest)]
        [DataRow(HttpStatusCode.Forbidden)]
        [DataRow(HttpStatusCode.GatewayTimeout)]
        public async Task AuthHandler_NonUnauthorizedStatusShouldPassThrough(HttpStatusCode statusCode)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            var expectedResponse = new HttpResponseMessage(statusCode);

            testHttpMessageHandler.SetHttpResponse(expectedResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.AreSame(response, expectedResponse, "Doesn't return a successful response");
            Assert.AreSame(response.RequestMessage, httpRequestMessage, "Http response message sets wrong request message");
        }

        [TestMethod]
        public async Task AuthHandler_ShouldRetryUnauthorizedGetRequest()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.com/bar");
            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, expectedResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.AreSame(response.RequestMessage, httpRequestMessage, "Http response message sets wrong request message");
            Assert.AreSame(response, expectedResponse, "Retry didn't happen");
            Assert.IsNull(response.RequestMessage.Content, "Content is not null");
        }

        [TestMethod]
        public async Task AuthHandler_ShouldRetryUnauthorizedPostRequestWithNoContent()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.com/bar");
            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, expectedResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.AreSame(response.RequestMessage, httpRequestMessage, "Http response message sets wrong request message");
            Assert.AreSame(response, expectedResponse, "Retry didn't happen");
            Assert.IsNull(response.RequestMessage.Content, "Content is not null");
        }

        [TestMethod]
        public async Task AuthHandler_ShouldRetryUnauthorizedPostRequestWithBufferContent()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.com/bar");
            httpRequestMessage.Content = new StringContent("Hello World!");

            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var okResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, okResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.AreSame(response, okResponse, "Retry didn't happen");
            Assert.AreNotSame(response, unauthorizedResponse, "Retry didn't happen");
            Assert.IsNotNull(response.RequestMessage.Content, "The request content is null");
            Assert.AreEqual(response.RequestMessage.Content.ReadAsStringAsync().Result, "Hello World!", "Content changed");
        }

        [TestMethod]
        public async Task AuthHandler_ShouldRetryUnauthorizedPatchRequestWithBufferContent()
        {
            var httpRequestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), "http://example.com/bar");
            httpRequestMessage.Content = new StringContent("Hello World!");

            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var okResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, okResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.AreSame(response, okResponse, "Retry didn't happen");
            Assert.AreNotSame(response, unauthorizedResponse, "Retry didn't happen");
            Assert.IsNotNull(response.RequestMessage.Content, "The request content is null");
            Assert.AreEqual(response.RequestMessage.Content.ReadAsStringAsync().Result, "Hello World!", "Content changed");
        }

        [TestMethod]
        public async Task AuthHandler_ShouldNotRetryUnauthorizedPutRequestWithStreamContent()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, "http://example.com/bar");
            httpRequestMessage.Content = new StringContent("Jambo");
            httpRequestMessage.Content.Headers.ContentLength = -1;

            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var okResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, okResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.AreNotSame(response, okResponse, "Unexpected retry");
            Assert.AreSame(response, unauthorizedResponse, "Unexpected retry");
            Assert.IsNotNull(response.RequestMessage.Content, "Request content is null");
            Assert.AreEqual(response.RequestMessage.Content.Headers.ContentLength, -1, "Request content length changed");
        }

        [TestMethod]
        public async Task AuthHandler_ShouldNotRetryUnauthorizedPatchRequestWithStreamContent()
        {
            var httpRequestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), "http://example.com/bar");
            httpRequestMessage.Content = new StringContent("Jambo");
            httpRequestMessage.Content.Headers.ContentLength = -1;

            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var okResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, okResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.AreNotSame(response, okResponse, "Unexpected retry");
            Assert.AreSame(response, unauthorizedResponse, "Unexpected retry");
            Assert.IsNotNull(response.RequestMessage.Content, "Request content is null");
            Assert.AreEqual(response.RequestMessage.Content.Headers.ContentLength, -1, "Request content length changed");
        }

        [TestMethod]
        public async Task AuthHandler_ShouldReturnUnauthorizedRequestWithDefaultMaxRetryExceeded()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, "http://example.com/bar");
            httpRequestMessage.Content = new StringContent("Hello Mars!");
            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, expectedResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.AreSame(response, expectedResponse, "Unexpected code returned");
            Assert.AreEqual(response.RequestMessage.Content.ReadAsStringAsync().Result, "Hello Mars!");
        }
    }
}
