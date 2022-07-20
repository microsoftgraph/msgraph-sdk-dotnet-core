// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using Xunit;
    using System.Threading.Tasks;
    using System.Collections.Generic;

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
            authenticationHandler.Dispose();
            testHttpMessageHandler.Dispose();
        }

        [Fact]
        public void AuthHandler_AuthProviderConstructor()
        {
            using (AuthenticationHandler auth = new AuthenticationHandler(mockAuthenticationProvider.Object))
            {
                Assert.Null(auth.InnerHandler);
                Assert.NotNull(auth.AuthenticationProvider);
                Assert.NotNull(auth.AuthOption);
                Assert.IsType<AuthenticationHandler>(auth);
            }
        }

        [Fact]
        public void AuthHandler_AuthProviderHttpMessageHandlerConstructor()
        {
            Assert.NotNull(authenticationHandler.InnerHandler);
            Assert.NotNull(authenticationHandler.AuthenticationProvider);
            Assert.NotNull(authenticationHandler.AuthOption);
            Assert.IsType<AuthenticationHandler>(authenticationHandler);
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
                Assert.IsType<AuthenticationHandler>(auth);
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
        public async Task AuthHandler_ShouldRetryUnauthorizedGetRequestUsingAuthHandlerOption()
        {
            DelegatingHandler authHandler = new AuthenticationHandler(null, testHttpMessageHandler);
            using (HttpMessageInvoker msgInvoker = new HttpMessageInvoker(authHandler))
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.com/bar"))
            using (var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized))
            using (var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK))
            {
                httpRequestMessage.Properties.Add(nameof(GraphRequestContext), new GraphRequestContext
                {
                    MiddlewareOptions = new Dictionary<string, IMiddlewareOption>() {
                        {
                            nameof(AuthenticationHandlerOption),
                            new AuthenticationHandlerOption { AuthenticationProvider = mockAuthenticationProvider.Object }
                        }
                    }
                });
                testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, expectedResponse);

                var response = await msgInvoker.SendAsync(httpRequestMessage, new CancellationToken());

                Assert.NotSame(response.RequestMessage, httpRequestMessage);
                Assert.Same(response, expectedResponse);
                Assert.Null(response.RequestMessage.Content);
            }
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
            Assert.Equal("Hello World!", response.RequestMessage.Content.ReadAsStringAsync().Result);
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
            Assert.Equal("Hello World!", response.RequestMessage.Content.ReadAsStringAsync().Result);
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
            Assert.Equal("Hello Mars!", response.RequestMessage.Content.ReadAsStringAsync().Result);
        }

        [Fact(Skip = "In order to support HttpProvider, we'll skip authentication if no provider is set. We will add enable this once we re-write a new HttpProvider.")]
        public async Task AuthHandler_ShouldThrowExceptionWhenAuthProviderIsNotSet()
        {
            DelegatingHandler authHandler = new AuthenticationHandler(null, testHttpMessageHandler);
            using (HttpMessageInvoker msgInvoker = new HttpMessageInvoker(authHandler))
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.com/bar"))
            using (var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized))
            using (var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK))
            {
                testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, expectedResponse);

                ServiceException ex = await Assert.ThrowsAsync<ServiceException>(() => msgInvoker.SendAsync(httpRequestMessage, new CancellationToken()));

                Assert.Same(ex.Error.Code, ErrorConstants.Codes.InvalidRequest);
                Assert.Same(ex.Error.Message, ErrorConstants.Messages.AuthenticationProviderMissing);
            }
        }

        [Theory]
        [InlineData("authorization_url =\"https://login.microsoftonline.com/common/oauth2/authorize\",error=\"insufficient_claims\",claims=\"eyJhY2Nlc3NfdG9rZW4iOnsibmJmIjp7ImVzc2VudGlhbCI6ZmFsc2UsInZhbHVlIjoxNTM5Mjg0Mzc2fX19\"", "{\"access_token\":{\"nbf\":{\"essential\":false,\"value\":1539284376}}}")]
        [InlineData("Bearer realm=\"\", authorization_uri=https://login.microsoftonline.com/common/oauth2/authorize, client_id=\"00000003-0000-0000-c000-000000000000\", error=\"insufficient_claims\", claims=\"eyJhY2Nlc3NfdG9rZW4iOnsibmJmIjp7ImVzc2VudGlhbCI6dHJ1ZSwgInZhbHVlIjoiMTY1NzgzNDQxNyJ9fX0=\", errorDescription=\"Continuous access evaluation resulted in claims challenge with result: InteractionRequired and code: TokenIssuedBeforeRevocationTimestamp\"", "{\"access_token\":{\"nbf\":{\"essential\":true, \"value\":\"1657834417\"}}}")]
        public async Task AuthHandler_ShouldRetryUnauthorizedGetRequestAndExtractWWWAuthenticateHeaders(string headerValue, string expectedClaims)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.com/bar");
            
            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            unauthorizedResponse.Headers.WwwAuthenticate.Add(
                new AuthenticationHeaderValue("authorization_url", headerValue));

            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, expectedResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            var requestContext = response.RequestMessage.GetRequestContext();

            var middleWareOption = requestContext.MiddlewareOptions[nameof(AuthenticationHandlerOption)] as AuthenticationHandlerOption;
            Assert.NotNull(middleWareOption);

            var authProviderOption = middleWareOption.AuthenticationProviderOption as ICaeAuthenticationProviderOption;
            Assert.NotNull(authProviderOption);

            // Assert the decoded claims string is as expected
            Assert.Equal(expectedClaims, authProviderOption.Claims);
            Assert.Same(response, expectedResponse);
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
        }

        [Fact]
        // Test with a request that already has an auth provider option present
        public async Task AuthHandler_ShouldRetryUnauthorizedGetRequestAndExtractWWWAuthenticateHeadersShouldNotLoseScopesInformation()
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.com/bar");
            var authenticationHandlerOption = new AuthenticationHandlerOption();
            var authenticationProviderOption = new AuthenticationProviderOptionTest
            {
                Scopes = new string[] { "User.Read" }
            };
            authenticationHandlerOption.AuthenticationProviderOption = authenticationProviderOption;

            // set the original AuthenticationProviderOptionTest as the auth provider
            var originalRequestContext = httpRequestMessage.GetRequestContext();
            originalRequestContext.MiddlewareOptions[nameof(AuthenticationHandlerOption)] = authenticationHandlerOption;
            httpRequestMessage.Properties[nameof(GraphRequestContext)] = originalRequestContext;

            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            unauthorizedResponse.Headers.WwwAuthenticate.Add(
                new AuthenticationHeaderValue("authorization_url",
                    "authorization_url=\"https://login.microsoftonline.com/common/oauth2/authorize\"," +
                    "error=\"insufficient_claims\"," +
                    "claims=\"eyJhY2Nlc3NfdG9rZW4iOnsibmJmIjp7ImVzc2VudGlhbCI6ZmFsc2UsInZhbHVlIjoxNTM5Mjg0Mzc2fX19\""));

            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            
            // Act
            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, expectedResponse);
            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());
            var requestContext = response.RequestMessage.GetRequestContext();

            // Assert
            var middleWareOption = requestContext.MiddlewareOptions[nameof(AuthenticationHandlerOption)] as AuthenticationHandlerOption;
            Assert.NotNull(middleWareOption);

            var authProviderOption = middleWareOption.AuthenticationProviderOption as ICaeAuthenticationProviderOption;
            Assert.NotNull(authProviderOption);

            // Assert the decoded claims string is as expected
            Assert.Equal("{\"access_token\":{\"nbf\":{\"essential\":false,\"value\":1539284376}}}", authProviderOption.Claims);
            Assert.Same(response, expectedResponse);
            Assert.NotSame(response.RequestMessage, httpRequestMessage);

            // Assert that we still have the original scopes information
            Assert.Single(authProviderOption.Scopes);
            Assert.Equal("User.Read", authProviderOption.Scopes[0]);
        }
    }

    /// <summary>
    /// Test class that implements the <see cref="IAuthenticationProviderOption"/> interface
    /// </summary>
    internal class AuthenticationProviderOptionTest : IAuthenticationProviderOption
    {
        public string[] Scopes { get; set; }
    }
}
