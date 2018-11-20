// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Graph.Core.Test.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Graph.Core.Test.Requests
{
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
        public void AuthHandler_DefaultConstructor()
        {
            using (AuthenticationHandler auth = new AuthenticationHandler())
            {
                Assert.IsNull(auth.InnerHandler, "Http message handler initialized");
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

        [TestMethod]
        public async Task AuthHandler_OkStatusShouldPassThrough()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(expectedResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.AreSame(response, expectedResponse, "Doesn't return a successful response");
            Assert.AreSame(response.RequestMessage, httpRequestMessage, "Http response message sets wrong request message");
        }

        [DataTestMethod]
        [DataRow(HttpStatusCode.Unauthorized)]
        public async Task AuthHandler_ShouldRetryUnAuthorizedResponse(HttpStatusCode statusCode)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.com/bar");
            var unAuthorizedResponse = new HttpResponseMessage(statusCode);
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unAuthorizedResponse, expectedResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.AreSame(response.RequestMessage, httpRequestMessage, "Http response message sets wrong request message");
            Assert.AreSame(response, expectedResponse, "Retry didn't happen");
        }
    }
}
