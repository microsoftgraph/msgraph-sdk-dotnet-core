// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Xunit;

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
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
        public void AuthHandler_DefaultConstructor()
        {
            using (AuthenticationHandler auth = new AuthenticationHandler())
            {
                Assert.Null(auth.InnerHandler);
                Assert.Null(auth.AuthenticationProvider);
                Assert.IsType(typeof(AuthenticationHandler), auth);
            }
        }

        [Fact]
        public void AuthHandler_AuthProviderHttpMessageHandlerConstructor()
        {
            Assert.NotNull(authenticationHandler.InnerHandler);
            Assert.NotNull(authenticationHandler.AuthenticationProvider);
            Assert.IsType(typeof(AuthenticationHandler), authenticationHandler);
        }

        [Fact]
        public async System.Threading.Tasks.Task AuthHandler_OkStatusShouldPassThroughAsync()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(httpResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.Equal(response.StatusCode, HttpStatusCode.OK);
            Assert.Same(response.RequestMessage, httpRequestMessage);
        }
    }
}
