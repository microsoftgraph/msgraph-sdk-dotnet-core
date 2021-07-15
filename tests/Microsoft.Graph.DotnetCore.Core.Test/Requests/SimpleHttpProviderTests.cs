// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using Moq;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    public class SimpleHttpProviderTests:IDisposable
    {
        private SimpleHttpProvider simpleHttpProvider;
        private readonly MockSerializer serializer;
        private readonly TestHttpMessageHandler testHttpMessageHandler;
        private readonly MockAuthenticationProvider authProvider;

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
        private const string JsonErrorResponseBody = "{\"error\":{\"code\":\"BadRequest\",\"message\":\"Resource not found for the segment 'mer'.\",\"innerError\":{\"request - id\":\"a9acfc00-2b19-44b5-a2c6-6c329b4337b3\",\"date\":\"2019-09-10T18:26:26\",\"code\":\"inner-error-code\"},\"target\":\"target-value\",\"unexpected-property\":\"unexpected-property-value\",\"details\":[{\"code\":\"details-code-value\",\"message\":\"details\",\"target\":\"details-target-value\",\"unexpected-details-property\":\"unexpected-details-property-value\"},{\"code\":\"details-code-value2\"}]}}";

        public SimpleHttpProviderTests()
        {
            this.testHttpMessageHandler = new TestHttpMessageHandler();
            this.authProvider = new MockAuthenticationProvider();
            this.serializer = new MockSerializer();

            var defaultHandlers = GraphClientFactory.CreateDefaultHandlers(authProvider.Object);
            var httpClient = GraphClientFactory.Create(handlers: defaultHandlers, finalHandler: testHttpMessageHandler);

            this.simpleHttpProvider = new SimpleHttpProvider(httpClient, this.serializer.Object);
        }

        public void Dispose()
        {
            this.simpleHttpProvider.Dispose();
        }

        [Fact]
        public void InitSuccessfullyWithoutHttpClient()
        {
            // Create a provider using a null client
            SimpleHttpProvider testSimpleHttpProvider = new SimpleHttpProvider(null, this.serializer.Object);
            // Assert that the httpclient is set (from the factory)
            Assert.NotNull(testSimpleHttpProvider.httpClient);
        }

        [Fact]
        public async Task InitSuccessfullyWithUsedHttpClient()
        {
            // Create a httpClient
            var defaultHandlers = GraphClientFactory.CreateDefaultHandlers(authProvider.Object);
            using (HttpClient httpClient = GraphClientFactory.Create(handlers: defaultHandlers, finalHandler: testHttpMessageHandler))
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);
                // use the httpClient to send something out
                await httpClient.SendAsync(httpRequestMessage);
                // Create a provider using the same client
                SimpleHttpProvider testSimpleHttpProvider = new SimpleHttpProvider(httpClient, this.serializer.Object);
                // Assert that using the used client throws no errors on initialization
                Assert.NotNull(testSimpleHttpProvider.Serializer);
                Assert.Equal(httpClient.Timeout, simpleHttpProvider.OverallTimeout);
            }
        }

        [Fact]
        public async Task SendAsync()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);
                var returnedResponseMessage = await this.simpleHttpProvider.SendAsync(httpRequestMessage);
                Assert.True(returnedResponseMessage.RequestMessage.Headers.Contains(CoreConstants.Headers.FeatureFlag));
                Assert.Equal(httpResponseMessage, returnedResponseMessage);
            }
        }


        [Fact]
        public async Task SendAsync_ThrowsClientGeneralException()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            {
                this.simpleHttpProvider.Dispose();
                var clientException = new Exception();

                var defaultHandlers = GraphClientFactory.CreateDefaultHandlers(authProvider.Object);
                var httpClient = GraphClientFactory.Create(handlers: defaultHandlers, finalHandler: new ExceptionHttpMessageHandler(clientException));
                this.simpleHttpProvider = new SimpleHttpProvider(httpClient, this.serializer.Object);

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.simpleHttpProvider.SendAsync(
                    httpRequestMessage, HttpCompletionOption.ResponseContentRead, CancellationToken.None));

                Assert.True(exception.IsMatch(ErrorConstants.Codes.GeneralException));
                Assert.Equal(ErrorConstants.Messages.UnexpectedExceptionOnSend, exception.Error.Message);
                Assert.Equal(clientException, exception.InnerException);
            }
        }

        [Fact]

        public async Task SendAsync_RethrowsTaskCancelledException()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            {
                this.simpleHttpProvider.Dispose();

                var message = "Task cancelled";
                var clientException = new TaskCanceledException(message);
                var defaultHandlers = GraphClientFactory.CreateDefaultHandlers(authProvider.Object);
                var httpClient = GraphClientFactory.Create(handlers: defaultHandlers, finalHandler: new ExceptionHttpMessageHandler(clientException));
                this.simpleHttpProvider = new SimpleHttpProvider(httpClient, this.serializer.Object);

                TaskCanceledException exception = await Assert.ThrowsAsync<TaskCanceledException>(async () => await this.simpleHttpProvider.SendAsync(
                    httpRequestMessage, HttpCompletionOption.ResponseContentRead, CancellationToken.None));

                Assert.Equal(message,exception.Message);
            }
        }

        [Fact]
        public async Task SendAsync_ThrowsServiceExceptionOnInvalidRedirectResponse()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.StatusCode = HttpStatusCode.Redirect;
                httpResponseMessage.RequestMessage = httpRequestMessage;
                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.simpleHttpProvider.SendAsync(httpRequestMessage));
                Assert.True(exception.IsMatch(ErrorConstants.Codes.GeneralException));
                Assert.Equal(
                    ErrorConstants.Messages.LocationHeaderNotSetOnRedirect,
                    exception.Error.Message);
            }
        }

        [Fact]
        public async Task SendAsync_VerifiesHeadersOnRedirect()
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

                var returnedResponseMessage = await this.simpleHttpProvider.SendAsync(httpRequestMessage);

                Assert.Equal(6, finalResponseMessage.RequestMessage.Headers.Count());

                foreach (var header in httpRequestMessage.Headers)
                {
                    var actualValues = finalResponseMessage.RequestMessage.Headers.GetValues(header.Key);

                    var enumerable = actualValues as string[] ?? actualValues.ToArray();
                    Assert.Equal(enumerable.Length, header.Value.Count());

                    foreach (var headerValue in header.Value)
                    {
                        Assert.Contains(headerValue, enumerable);
                    }
                }

                Assert.Equal(finalResponseMessage, returnedResponseMessage);
            }
        }

        [Fact]
        public async Task SendAsync_TThrowsServiceExceptionOnMaxRedirects()
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

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.simpleHttpProvider.SendAsync(
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
        public async Task SendAsync_ThrowsServiceExceptionWithEmptyMessageOnHTTPNotFoundWithoutErrorBody()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost"))
            using (var stringContent = new StringContent("test"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.Content = stringContent;
                httpResponseMessage.StatusCode = HttpStatusCode.NotFound;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);
                this.serializer.Setup(
                        mySerializer => mySerializer.DeserializeObject<ErrorResponse>(
                            It.IsAny<Stream>()))
                    .Returns((ErrorResponse)null);

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.simpleHttpProvider.SendAsync(httpRequestMessage));
                Assert.True(exception.IsMatch(ErrorConstants.Codes.ItemNotFound));
                Assert.True(string.IsNullOrEmpty(exception.Error.Message));
            }
        }

        [Fact]
        public async Task SendAsync_ThrowsServiceExceptionWithMessageOnHTTPNotFoundWithBody()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var stringContent = new StringContent("test"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.Content = stringContent;
                httpResponseMessage.StatusCode = HttpStatusCode.InternalServerError;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);
                var expectedError = new ErrorResponse
                {
                    Error = new Error
                    {
                        Code = ErrorConstants.Codes.ItemNotFound,
                        Message = "Error message"
                    }
                };

                this.serializer.Setup(mySerializer => mySerializer.DeserializeObject<ErrorResponse>(It.IsAny<Stream>())).Returns(expectedError);

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.simpleHttpProvider.SendAsync(httpRequestMessage));
                Assert.Equal(expectedError.Error.Code, exception.Error.Code);
                Assert.Equal(expectedError.Error.Message, exception.Error.Message);
            }
        }

        [Fact]
        public async Task SendAsync_ThrowsServiceExceptionWithThrowSiteHeader()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                const string throwSite = "throw site";

                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                httpResponseMessage.Headers.Add(CoreConstants.Headers.ThrowSiteHeaderName, throwSite);
                httpResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);

                this.serializer.Setup(
                        mySerializer => mySerializer.DeserializeObject<ErrorResponse>(
                            It.IsAny<Stream>()))
                    .Returns(new ErrorResponse { Error = new Error() });

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.simpleHttpProvider.SendAsync(httpRequestMessage));
                Assert.NotNull(exception.Error);
                Assert.Equal(throwSite, exception.Error.ThrowSite);
            }
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

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.simpleHttpProvider.SendAsync(httpRequestMessage));
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
            using (var stringContent = new StringContent(JsonErrorResponseBody))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.Content = stringContent;
                httpResponseMessage.Content.Headers.ContentType.MediaType = CoreConstants.MimeTypeNames.Application.Json;

                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                httpResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.simpleHttpProvider.SendAsync(httpRequestMessage));

                // Assert that we creating an GeneralException error.
                Assert.Same(ErrorConstants.Codes.GeneralException, exception.Error.Code);
                Assert.Same(ErrorConstants.Messages.UnexpectedExceptionResponse, exception.Error.Message);

                // Assert that we get the expected response body.
                Assert.Equal(JsonErrorResponseBody, exception.RawResponseBody);

            }
        }

        /// <summary>
        /// Testing that ErrorResponse can't be deserialized and causes the GeneralException 
        /// code to be thrown in a ServiceException.
        /// This test validates that the NullReference exception is no longer thrown according to
        /// https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/113
        /// </summary>
        [Fact]
        public async Task SendAsync_DoesNotThrowNullReferenceExceptionWhenHeaderContentTypeIsNull()
        {
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "https://localhost"))
            using (var stringContent = new StringContent(""))
            using (var httpResponseMessage = new HttpResponseMessage())
            {
                httpResponseMessage.Content = stringContent;
                httpResponseMessage.Content.Headers.ContentType = null;

                httpResponseMessage.StatusCode = HttpStatusCode.BadRequest;
                httpResponseMessage.RequestMessage = httpRequestMessage;

                this.testHttpMessageHandler.AddResponseMapping(httpRequestMessage.RequestUri.ToString(), httpResponseMessage);

                ServiceException exception = await Assert.ThrowsAsync<ServiceException>(async () => await this.simpleHttpProvider.SendAsync(httpRequestMessage));

                // Assert that we creating an GeneralException error.
                Assert.Same(ErrorConstants.Codes.GeneralException, exception.Error.Code);
                Assert.Same(ErrorConstants.Messages.UnexpectedExceptionResponse, exception.Error.Message);

                // Assert that the response is null since we have no contentType.
                Assert.Null(exception.RawResponseBody);

            }
        }
    }
}
