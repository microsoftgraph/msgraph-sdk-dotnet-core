// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    public class HttpProviderTests : IDisposable
    {
        private HttpProvider httpProvider;
        private MockSerializer serializer = new MockSerializer();
        private TestHttpMessageHandler testHttpMessageHandler;
        private MockAuthenticationProvider authProvider;

        /*
         {
            "error": {
                "code": "BadRequest",
                "message": "Resource not found for the segment 'mer'.",
                "innerError": {
                    "request - id": "a9acfc00-2b19-44b5-a2c6-6c329b4337b3",
                    "date": "2019-09-10T18:26:26",
                    "code": "inner-error-code"
                },
                "target": "target-value",
                "unexpected-property": "unexpected-property-value",
                "details": [
                    {
                        "code": "details-code-value",
                        "message": "details",
                        "target": "details-target-value",
                        "unexpected-details-property": "unexpected-details-property-value"
                    },
                    {
                        "code": "details-code-value2"
                    }
                ]
            }
        }
        */
        // Use https://www.minifyjson.org/ if you need minify or beautify as part of an update.
        private const string jsonErrorResponseBody = "{\"error\":{\"code\":\"BadRequest\",\"message\":\"Resource not found for the segment 'mer'.\",\"innerError\":{\"request - id\":\"a9acfc00-2b19-44b5-a2c6-6c329b4337b3\",\"date\":\"2019-09-10T18:26:26\",\"code\":\"inner-error-code\"},\"target\":\"target-value\",\"unexpected-property\":\"unexpected-property-value\",\"details\":[{\"code\":\"details-code-value\",\"message\":\"details\",\"target\":\"details-target-value\",\"unexpected-details-property\":\"unexpected-details-property-value\"},{\"code\":\"details-code-value2\"}]}}";

        public HttpProviderTests()
        {
            this.testHttpMessageHandler = new TestHttpMessageHandler();
            this.authProvider = new MockAuthenticationProvider();

            var defaultHandlers = GraphClientFactory.CreateDefaultHandlers(authProvider.Object);
            var pipeline = GraphClientFactory.CreatePipeline(defaultHandlers, this.testHttpMessageHandler);

            this.httpProvider = new HttpProvider(pipeline, true, this.serializer.Object);
        }

        public void Dispose()
        {
            this.httpProvider.Dispose();
        }

        [Fact]
        public void HttpProvider_CustomCacheHeaderAndTimeout()
        {
            var timeout = TimeSpan.FromSeconds(200);
            var cacheHeader = new CacheControlHeaderValue();
            using (var defaultHttpProvider = new HttpProvider(null) { CacheControlHeader = cacheHeader, OverallTimeout = timeout })
            {
                Assert.False(defaultHttpProvider.httpClient.DefaultRequestHeaders.CacheControl.NoCache);
                Assert.False(defaultHttpProvider.httpClient.DefaultRequestHeaders.CacheControl.NoStore);
                Assert.True(defaultHttpProvider.httpClient.DefaultRequestHeaders.Contains(CoreConstants.Headers.FeatureFlag));
                Assert.Equal(timeout, defaultHttpProvider.httpClient.Timeout);
                Assert.NotNull(defaultHttpProvider.Serializer);
                Assert.IsType<Serializer>(defaultHttpProvider.Serializer);
            }
        }

        [Fact]
        public void HttpProvider_CustomHttpClientHandler()
        {
            using (var httpClientHandler = new HttpClientHandler())
            using (var httpProvider = new HttpProvider(httpClientHandler, false, null))
            {
                Assert.Equal(httpClientHandler, httpProvider.httpMessageHandler);
                Assert.True(httpProvider.httpClient.DefaultRequestHeaders.Contains(CoreConstants.Headers.FeatureFlag));
                Assert.False(httpProvider.disposeHandler);
            }
        }

        [Fact]
        public void HttpProvider_DefaultConstructor()
        {
            using (var defaultHttpProvider = new HttpProvider())
            {
                Assert.True(defaultHttpProvider.httpClient.DefaultRequestHeaders.CacheControl.NoCache);
                Assert.True(defaultHttpProvider.httpClient.DefaultRequestHeaders.CacheControl.NoStore);
                Assert.True(defaultHttpProvider.httpClient.DefaultRequestHeaders.Contains(CoreConstants.Headers.FeatureFlag));
                Assert.True(defaultHttpProvider.disposeHandler);
                Assert.NotNull(defaultHttpProvider.httpMessageHandler);
                Assert.Equal(TimeSpan.FromSeconds(100), defaultHttpProvider.httpClient.Timeout);
                Assert.IsType<Serializer>(defaultHttpProvider.Serializer);
#if ANDROID
                Assert.IsType<Xamarin.Android.Net.AndroidClientHandler>(defaultHttpProvider.httpMessageHandler);
                Assert.False((defaultHttpProvider.httpMessageHandler as Xamarin.Android.Net.AndroidClientHandler).AllowAutoRedirect);
#elif iOS
                Assert.IsType<NSUrlSessionHandler>(defaultHttpProvider.httpMessageHandler);
                Assert.False((defaultHttpProvider.httpMessageHandler as NSUrlSessionHandler).AllowAutoRedirect);
#elif macOS
                Assert.IsType<Foundation.NSUrlSessionHandler>(defaultHttpProvider.httpMessageHandler);
                Assert.False((defaultHttpProvider.httpMessageHandler as Foundation.NSUrlSessionHandler).AllowAutoRedirect);
#else
                Assert.IsType<HttpClientHandler>(defaultHttpProvider.httpMessageHandler);
                Assert.False((defaultHttpProvider.httpMessageHandler as HttpClientHandler).AllowAutoRedirect);
#endif
            }
        }

        [Fact]
        public void HttpProvider_HttpMessageHandlerConstructor() {
           
            using (var httpProvider = new HttpProvider(this.testHttpMessageHandler, false, null))
            {
                Assert.NotNull(httpProvider.httpMessageHandler);
                Assert.True(httpProvider.httpClient.DefaultRequestHeaders.Contains(CoreConstants.Headers.FeatureFlag));
                Assert.Equal(httpProvider.httpMessageHandler, this.testHttpMessageHandler);
                Assert.False(httpProvider.disposeHandler);
                Assert.IsType<Serializer>(httpProvider.Serializer);
            }
        }

        [Fact]
        public async Task SendAsync()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);
                this.AddGraphRequestContextToRequest(httpRequestMessage);
                var returnedResponseMessage = await this.httpProvider.SendAsync(httpRequestMessage);
                Assert.True(returnedResponseMessage.RequestMessage.Headers.Contains(CoreConstants.Headers.FeatureFlag));
                Assert.Equal(httpResponseMessage, returnedResponseMessage);
            }
        }

        [Fact]
        public async Task SendAsync_ClientGeneralException()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            {
                this.httpProvider.Dispose();

                var clientException = new Exception();
                this.httpProvider = new HttpProvider(new ExceptionHttpMessageHandler(clientException), /* disposeHandler */ true, null);
                this.AddGraphRequestContextToRequest(httpRequestMessage);

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.httpProvider.SendRequestAsync(
                    httpRequestMessage, HttpCompletionOption.ResponseContentRead, CancellationToken.None));

                Assert.True(exception.IsMatch(ErrorConstants.Codes.GeneralException));
                Assert.Equal(ErrorConstants.Messages.UnexpectedExceptionOnSend, exception.Error.Message);
                Assert.Equal(clientException, exception.InnerException);
            }
        }

        [Fact]
        public async Task SendAsync_ClientTimeout()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            {
                this.httpProvider.Dispose();

                var clientException = new TaskCanceledException();
                this.httpProvider = new HttpProvider(new ExceptionHttpMessageHandler(clientException), /* disposeHandler */ true, null);
                this.AddGraphRequestContextToRequest(httpRequestMessage);

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.httpProvider.SendRequestAsync(
                        httpRequestMessage, HttpCompletionOption.ResponseContentRead, CancellationToken.None));

                Assert.True(exception.IsMatch(ErrorConstants.Codes.Timeout));
                Assert.Equal(ErrorConstants.Messages.RequestTimedOut, exception.Error.Message);
                Assert.Equal(clientException, exception.InnerException);
            }
        }

        [Fact]
        public async Task SendAsync_InvalidRedirectResponse()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.StatusCode = HttpStatusCode.Redirect;
                httpResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);
                this.AddGraphRequestContextToRequest(httpRequestMessage);

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.httpProvider.SendAsync(httpRequestMessage));
                Assert.True(exception.IsMatch(ErrorConstants.Codes.GeneralException));
                Assert.Equal(
                    ErrorConstants.Messages.LocationHeaderNotSetOnRedirect,
                    exception.Error.Message);
            }
        }

        [Fact]
        public async Task SendAsync_RedirectResponse_VerifyHeadersOnRedirect()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var redirectResponseMessage = new HttpResponseMessage())
            using (var finalResponseMessage = new HttpResponseMessage())
            {
                httpRequestMessage.Headers.Add("testHeader", "testValue");

                redirectResponseMessage.StatusCode = HttpStatusCode.Redirect;
                redirectResponseMessage.Headers.Location = new Uri("https://localhost/redirect");
                redirectResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), redirectResponseMessage);
                this.testHttpMessageHandler.AddResponseMapping(redirectResponseMessage.Headers.Location.ToString(), finalResponseMessage);
                this.AddGraphRequestContextToRequest(httpRequestMessage);

                var returnedResponseMessage = await this.httpProvider.SendAsync(httpRequestMessage);

                Assert.Equal(4, finalResponseMessage.RequestMessage.Headers.Count());

                foreach (var header in httpRequestMessage.Headers)
                {
                    var actualValues = finalResponseMessage.RequestMessage.Headers.GetValues(header.Key);

                    Assert.Equal(actualValues.Count(), header.Value.Count());

                    foreach (var headerValue in header.Value)
                    {
                        Assert.Contains(headerValue, actualValues);
                    }
                }
            }
        }

        [Fact]
        public async Task SendAsync_MaxRedirects()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var redirectResponseMessage = new HttpResponseMessage())
            using (var tooManyRedirectsResponseMessage = new HttpResponseMessage())
            {
                redirectResponseMessage.StatusCode = HttpStatusCode.Redirect;
                redirectResponseMessage.Headers.Location = new Uri("https://localhost/redirect");
                tooManyRedirectsResponseMessage.StatusCode = HttpStatusCode.Redirect;
                tooManyRedirectsResponseMessage.Headers.Location = new Uri("https://localhost");

                redirectResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), redirectResponseMessage);
                this.testHttpMessageHandler.AddResponseMapping(redirectResponseMessage.Headers.Location.ToString(), tooManyRedirectsResponseMessage);

                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(CoreConstants.Headers.Bearer, "ticket");
                this.AddGraphRequestContextToRequest(httpRequestMessage);

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.httpProvider.SendAsync(
                        httpRequestMessage,
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None));
                Assert.True(exception.IsMatch(ErrorConstants.Codes.TooManyRedirects));
                Assert.Equal(
                    string.Format(ErrorConstants.Messages.TooManyRedirectsFormatString, "5"),
                    exception.Error.Message);
            }
        }

        [Fact]
        public async Task SendAsync_NotFoundWithoutErrorBody()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost"))
            using (var stringContent = new StringContent("test"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.Content = stringContent;
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);
                this.AddGraphRequestContextToRequest(httpRequestMessage);
                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<ErrorResponse>(
                        It.IsAny<Stream>()))
                    .Returns((ErrorResponse)null);

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.httpProvider.SendAsync(httpRequestMessage));
                Assert.True(exception.IsMatch(ErrorConstants.Codes.ItemNotFound));
                Assert.True(string.IsNullOrEmpty(exception.Error.Message));
            }
        }

        [Fact]
        public async Task SendAsync_NotFoundWithBody()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var stringContent = new StringContent("test"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.Content = stringContent;
                httpResponseMessage.StatusCode = HttpStatusCode.InternalServerError;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);
                this.AddGraphRequestContextToRequest(httpRequestMessage);
                var expectedError = new ErrorResponse
                {
                    Error = new Error
                    {
                        Code = ErrorConstants.Codes.ItemNotFound,
                        Message = "Error message"
                    }
                };

                this.serializer.Setup(serializer => serializer.DeserializeObject<ErrorResponse>(It.IsAny<Stream>())).Returns(expectedError);

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.httpProvider.SendAsync(httpRequestMessage));
                Assert.Equal(expectedError.Error.Code, exception.Error.Code);
                Assert.Equal(expectedError.Error.Message, exception.Error.Message);
            }
        }

        [Fact]
        public async Task SendAsync_CopyThrowSiteHeader()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                const string throwSite = "throw site";

                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                httpResponseMessage.Headers.Add(CoreConstants.Headers.ThrowSiteHeaderName, throwSite);
                httpResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);
                this.AddGraphRequestContextToRequest(httpRequestMessage);

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<ErrorResponse>(
                        It.IsAny<Stream>()))
                    .Returns(new ErrorResponse { Error = new Error() });

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.httpProvider.SendAsync(httpRequestMessage));
                Assert.NotNull(exception.Error);
                Assert.Equal(throwSite, exception.Error.ThrowSite);
            }
        }

        [Fact]
        public async Task SendAsync_CopyThrowSiteHeader_ThrowSiteAlreadyInError()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var stringContent = new StringContent("test"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.Content = stringContent;

                const string throwSiteBodyValue = "throw site in body";
                const string throwSiteHeaderValue = "throw site in header";

                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                httpResponseMessage.Headers.Add(CoreConstants.Headers.ThrowSiteHeaderName, throwSiteHeaderValue);
                httpResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);
                this.AddGraphRequestContextToRequest(httpRequestMessage);

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<ErrorResponse>(
                        It.IsAny<Stream>()))
                    .Returns(new ErrorResponse { Error = new Error { ThrowSite = throwSiteBodyValue } });

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.httpProvider.SendAsync(httpRequestMessage));
                Assert.NotNull(exception.Error);
                Assert.Equal(throwSiteBodyValue, exception.Error.ThrowSite);
            }
        }

        [Fact]
        public async Task SendAsync_WithCustomHandler()
        {
            string expectedToken = "send_with_custom_handler";
            var authHandler = new AuthenticationHandler(new MockAuthenticationProvider(expectedToken).Object, this.testHttpMessageHandler);
            using (var myHttpProvider = new HttpProvider(authHandler, true, new Serializer()))
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost");
                var httpResponseMessage = new HttpResponseMessage();
                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);

                var returnedResponseMessage = await myHttpProvider.SendAsync(httpRequestMessage);

                Assert.Equal(httpResponseMessage, returnedResponseMessage);
                Assert.NotNull(returnedResponseMessage.RequestMessage.Headers.Authorization);
                Assert.Equal(expectedToken, returnedResponseMessage.RequestMessage.Headers.Authorization.Parameter);
            }
        }

        private void AddGraphRequestContextToRequest(HttpRequestMessage httpRequestMessage)
        {
            var requestContext = new GraphRequestContext
            {
                MiddlewareOptions = new Dictionary<string, IMiddlewareOption>() {
                    {
                        typeof(AuthenticationHandlerOption).ToString(),
                        new AuthenticationHandlerOption { AuthenticationProvider = authProvider .Object }
                    }
                },
                ClientRequestId = "client-request-id"
            };
            httpRequestMessage.Properties.Add(typeof(GraphRequestContext).ToString(), requestContext);
        }

        [Fact]
        public async Task SendAsync_CopyClientRequestIdHeader_AddClientRequestIdToError()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var stringContent = new StringContent("test"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.Content = stringContent;

                const string clientRequestId = "3c9c5bc6-42d2-49ac-a99c-49c10513339a";

                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                httpResponseMessage.Headers.Add(CoreConstants.Headers.ClientRequestId, clientRequestId);
                httpResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);
                this.AddGraphRequestContextToRequest(httpRequestMessage);
                
                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.httpProvider.SendAsync(httpRequestMessage));
                Assert.NotNull(exception.Error);
                Assert.Equal(clientRequestId, exception.Error.ClientRequestId);
            }
        }

        /// <summary>
        /// Testing that ErrorResponse can't be deserialized and causes the GeneralException 
        /// code to be thrown in a ServiceException. We are testing whether we can
        /// get the response body.
        /// </summary>
        [Fact]
        public async Task SendAsync_AddRawResponseToErrorWithErrorResponseDeserializeException()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var stringContent = new StringContent(jsonErrorResponseBody))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.Content = stringContent;
                httpResponseMessage.Content.Headers.ContentType.MediaType = "application/json";

                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                httpResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);
                this.AddGraphRequestContextToRequest(httpRequestMessage);
                
                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.httpProvider.SendAsync(httpRequestMessage));

                // Assert that we creating an GeneralException error.
                Assert.Same(ErrorConstants.Codes.GeneralException, exception.Error.Code);
                Assert.Same(ErrorConstants.Messages.UnexpectedExceptionResponse, exception.Error.Message);

                // Assert that we get the expected response body.
                Assert.Equal(jsonErrorResponseBody, exception.RawResponseBody);

            }
        }

        /// <summary>
        /// Test whether the raw response body is provided on the ServiceException for E2E scenario
        /// </summary>
        /// <param name="authenticationToken">An invalid access token.</param>
        [Theory]
        [InlineData("Invalid token")]
        public async Task SendAsync_E2E_ValidateHasRawResponseBody(string authenticationToken)
        {
            var authenticationProvider = new DelegateAuthenticationProvider(
                (requestMessage) =>
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(CoreConstants.Headers.Bearer, authenticationToken);
                    return Task.FromResult(0);
                });

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/fail");
            var httpProvider = new HttpProvider();

            HttpResponseMessage response = null;

            var exception = (ServiceException) await Record.ExceptionAsync(async () =>
            {
                await authenticationProvider.AuthenticateRequestAsync(httpRequestMessage);
                response = await httpProvider.SendAsync(httpRequestMessage);
            });

            // Assert expected exception
            Assert.Null(response);
            Assert.NotNull(exception);
            Assert.NotNull(exception.Error);
            Assert.Contains("InvalidAuthenticationToken", exception.RawResponseBody);
            Assert.Equal("InvalidAuthenticationToken", exception.Error.Code);

            // Assert not unexpected deserialization exception
            Assert.NotSame(ErrorConstants.Codes.GeneralException, exception.Error.Code);
            Assert.NotSame(ErrorConstants.Messages.UnexpectedExceptionResponse, exception.Error.Message);
        }

        #region NETCORE
        // Skip this test for Xamain since it never throws an exception.
#if NETCORE
        [Fact]
        public async Task OverallTimeout_RequestAlreadySent()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);
                this.AddGraphRequestContextToRequest(httpRequestMessage);
                var returnedResponseMessage = await this.httpProvider.SendAsync(httpRequestMessage);
            }

            ServiceException serviceException = Assert.Throws<ServiceException>(() => this.httpProvider.OverallTimeout = new TimeSpan(0, 0, 30));
            Assert.True(serviceException.IsMatch(ErrorConstants.Codes.NotAllowed));
            Assert.Equal(
                ErrorConstants.Messages.OverallTimeoutCannotBeSet,
                serviceException.Error.Message);
            Assert.IsType<InvalidOperationException>(serviceException.InnerException);
        }
#endif
        #endregion

        #region ANDROID
#if ANDROID
        [Fact]
        public void HttpProvider_CustomAndroidClientHandler()
        {
            var proxy = new WebProxy("https://test.com");
            using (var httpClientHandler = new Xamarin.Android.Net.AndroidClientHandler { Proxy = proxy })
            using (var httpProvider = new HttpProvider(httpClientHandler, false, null))
            {
                Assert.Equal(httpClientHandler, httpProvider.httpMessageHandler);
                Assert.True(httpProvider.httpClient.DefaultRequestHeaders.Contains(CoreConstants.Headers.FeatureFlag));
                Assert.False(httpProvider.disposeHandler);
                Assert.Same((httpProvider.httpMessageHandler as Xamarin.Android.Net.AndroidClientHandler).Proxy, proxy);
            }
        }
#endif
        #endregion

        #region iOS_macOS
#if iOS || macOS
        [Fact]
        public void HttpProvider_CustomNSUrlSessionHandler()
        {
#if iOS
            using (var httpClientHandler = new NSUrlSessionHandler())
#elif macOS
            using (var httpClientHandler = new Foundation.NSUrlSessionHandler())
#endif
            using (var httpProvider = new HttpProvider(httpClientHandler, false, null))
            {
                Assert.Equal(httpClientHandler, httpProvider.httpMessageHandler);
                Assert.True(httpProvider.httpClient.DefaultRequestHeaders.Contains(CoreConstants.Headers.FeatureFlag));
                Assert.False(httpProvider.disposeHandler);
            }
        }
#endif
#endregion
        }
    }