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
    using System.Threading.Tasks;
    using Xunit;
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using Moq.Protected;

    public class RetryHandlerTests : IDisposable
    {
        private MockRedirectHandler testHttpMessageHandler;
        private RetryHandler retryHandler;
        private HttpMessageInvoker invoker;
        private const string RETRY_AFTER = "Retry-After";
        private const string RETRY_ATTEMPT = "Retry-Attempt";

        public RetryHandlerTests()
        {
            this.testHttpMessageHandler = new MockRedirectHandler();
            this.retryHandler = new RetryHandler(this.testHttpMessageHandler);
            this.invoker = new HttpMessageInvoker(this.retryHandler);
        }

        public void Dispose()
        {
            this.invoker.Dispose();
        }

        [Fact]
        public void retryHandler_Constructor()
        {
            using (RetryHandler retry = new RetryHandler())
            {
                Assert.Null(retry.InnerHandler);
                Assert.NotNull(retry.RetryOption);
                Assert.Equal(RetryHandlerOption.DEFAULT_MAX_RETRY, retry.RetryOption.MaxRetry);
                Assert.IsType<RetryHandler>(retry);
            }
        }


        [Fact]
        public void retryHandler_HttpMessageHandlerConstructor()
        {
            Assert.NotNull(retryHandler.InnerHandler);
            Assert.NotNull(retryHandler.RetryOption);
            Assert.Equal(RetryHandlerOption.DEFAULT_MAX_RETRY, retryHandler.RetryOption.MaxRetry);
            Assert.Equal(retryHandler.InnerHandler, testHttpMessageHandler);
            Assert.IsType<RetryHandler>(retryHandler);
        }

        [Fact]
        public void retryHandler_RetryOptionConstructor()
        {
            using (RetryHandler retry = new RetryHandler(new RetryHandlerOption { MaxRetry = 5, ShouldRetry = (d, a, r) => true }))
            {
                Assert.Null(retry.InnerHandler);
                Assert.NotNull(retry.RetryOption);
                Assert.Equal(5, retry.RetryOption.MaxRetry);
                Assert.IsType<RetryHandler>(retry);
            }
        }

        [Fact]
        public async Task OkStatusShouldPassThrough()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");

            var retryResponse = new HttpResponseMessage(HttpStatusCode.OK);
            this.testHttpMessageHandler.SetHttpResponse(retryResponse);

            var response = await this.invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.Same(response, retryResponse);
            Assert.Same(response.RequestMessage, httpRequestMessage);
            Assert.False(response.RequestMessage.Headers.Contains(RETRY_ATTEMPT), "The request add header wrong.");

        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldRetryWithAddRetryAttemptHeader(HttpStatusCode statusCode)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://example.org/foo");

            var retryResponse = new HttpResponseMessage(statusCode);

            var response_2 = new HttpResponseMessage(HttpStatusCode.OK);

            this.testHttpMessageHandler.SetHttpResponse(retryResponse, response_2);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.Same(response, response_2);
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.NotNull(response.RequestMessage.Headers);
            Assert.True(response.RequestMessage.Headers.Contains(RETRY_ATTEMPT));
            IEnumerable<string> values;
            Assert.True(response.RequestMessage.Headers.TryGetValues(RETRY_ATTEMPT, out values));
            Assert.Single(values);
            Assert.Equal(values.First(), 1.ToString());
        }


        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldRetryWithBuffedContent(HttpStatusCode statusCode)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo");
            httpRequestMessage.Content = new StringContent("Hello World");

            var retryResponse = new HttpResponseMessage(statusCode);

            var response_2 = new HttpResponseMessage(HttpStatusCode.OK);

            this.testHttpMessageHandler.SetHttpResponse(retryResponse, response_2);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.Same(response, response_2);
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.NotNull(response.RequestMessage.Content.Headers.ContentLength);
            Assert.Equal("Hello World", response.RequestMessage.Content.ReadAsStringAsync().Result);

        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldNotRetryWithPostStreaming(HttpStatusCode statusCode)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo");
            httpRequestMessage.Content = new StringContent("Test Content");
            httpRequestMessage.Content.Headers.ContentLength = -1;

            var retryResponse = new HttpResponseMessage(statusCode);

            var response_2 = new HttpResponseMessage(HttpStatusCode.OK);

            this.testHttpMessageHandler.SetHttpResponse(retryResponse, response_2);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.NotEqual(response, response_2);
            Assert.Same(response, retryResponse);
            Assert.Same(response.RequestMessage, httpRequestMessage);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.NotNull(response.RequestMessage.Content.Headers.ContentLength);
            Assert.Equal(response.RequestMessage.Content.Headers.ContentLength, -1);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldNotRetryWithPutStreaming(HttpStatusCode statusCode)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, "http://example.org/foo");
            httpRequestMessage.Content = new StringContent("Test Content");
            httpRequestMessage.Content.Headers.ContentLength = -1;

            var retryResponse = new HttpResponseMessage(statusCode);

            var response_2 = new HttpResponseMessage(HttpStatusCode.OK);

            this.testHttpMessageHandler.SetHttpResponse(retryResponse, response_2);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.NotEqual(response, response_2);
            Assert.Same(response.RequestMessage, httpRequestMessage);
            Assert.Same(response, retryResponse);
            Assert.NotNull(response.RequestMessage.Content);
            Assert.Equal(response.RequestMessage.Content.Headers.ContentLength, -1);
        }

      
        [Theory(Skip = "skip test")]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ExceedMaxRetryShouldReturn(HttpStatusCode statusCode)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo");

            var retryResponse = new HttpResponseMessage(statusCode);
            var response_2 = new HttpResponseMessage(statusCode);

            this.testHttpMessageHandler.SetHttpResponse(retryResponse, response_2);
            try
            {
                var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());
            }
            catch (ServiceException exception)
            {
                Assert.IsType<ServiceException>(exception);
                IEnumerable<string> values;
                Assert.True(httpRequestMessage.Headers.TryGetValues(RETRY_ATTEMPT, out values), "Don't set Retry-Attemp Header");
                Assert.Single(values);
                Assert.Equal(values.First(), 10.ToString());
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldDelayBasedOnRetryAfterHeader(HttpStatusCode statusCode)
        {
            var retryResponse = new HttpResponseMessage(statusCode);
            retryResponse.Headers.TryAddWithoutValidation(RETRY_AFTER, 1.ToString());
           
            await DelayTestWithMessage(retryResponse, 1, "Init");
        
            Assert.Equal("Init Work 1", Message);
        }


        [Theory(Skip = "skip test")] // Takes 9 minutes to run for each scenario
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldDelayBasedOnExponentialBackOff(HttpStatusCode statusCode)
        {
            var retryResponse = new HttpResponseMessage(statusCode);
            String compareMessage = "Init Work ";
           
            for (int count = 0; count < 3; count++)
            {
                await DelayTestWithMessage(retryResponse, count, "Init");
                Assert.Equal(Message, compareMessage + count.ToString());
            }
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShoulReturnSameStatusCodeWhenDelayIsGreaterThanRetryTimeLimit(HttpStatusCode statusCode)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo");
            httpRequestMessage.Content = new StringContent("Hello World");

            var retryResponse = new HttpResponseMessage(statusCode);
            retryResponse.Headers.TryAddWithoutValidation(RETRY_AFTER, 20.ToString());
            retryHandler.RetryOption.RetriesTimeLimit = TimeSpan.FromSeconds(10);

            this.testHttpMessageHandler.SetHttpResponse(retryResponse);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.Same(response, retryResponse);
        }

        [Theory]
        [InlineData(HttpStatusCode.GatewayTimeout)]  // 504
        [InlineData(HttpStatusCode.ServiceUnavailable)]  // 503
        [InlineData((HttpStatusCode)429)] // 429
        public async Task ShouldRetrytBasedOnRetryAfter(HttpStatusCode statusCode)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo");
            httpRequestMessage.Content = new StringContent("Hello World");

            var retryResponse = new HttpResponseMessage(statusCode);
            retryResponse.Headers.TryAddWithoutValidation(RETRY_AFTER, 30.ToString());

            var response_2 = new HttpResponseMessage(HttpStatusCode.OK);

            this.testHttpMessageHandler.SetHttpResponse(retryResponse, response_2);

            var response = await invoker.SendAsync(httpRequestMessage, new CancellationToken());

            Assert.Same(response, response_2);
            IEnumerable<string> values;
            Assert.True(response.RequestMessage.Headers.TryGetValues(RETRY_ATTEMPT, out values), "Don't set Retry-Attempt Header");
            Assert.Single(values);
            Assert.Equal(values.First(), 1.ToString());
            Assert.NotSame(response.RequestMessage, httpRequestMessage);
        }


        [Theory]
        [InlineData(1, HttpStatusCode.BadGateway)]
        [InlineData(2, HttpStatusCode.BadGateway)]
        [InlineData(3, HttpStatusCode.BadGateway)]
        [InlineData(4, HttpStatusCode.OK)]
        public async Task ShouldRetryBasedOnCustomShouldRetryDelegate(int expectedMaxRetry, HttpStatusCode expectedStatusCode)
        {

            var request = new HttpRequestMessage();
            Queue<HttpResponseMessage> httpResponseQueue = new(new HttpResponseMessage[]
            {
                new(HttpStatusCode.BadGateway) { RequestMessage = request },
                new(HttpStatusCode.BadGateway) { RequestMessage = request },
                new(HttpStatusCode.BadGateway) { RequestMessage = request },
                new(HttpStatusCode.BadGateway) { RequestMessage = request },
                new(HttpStatusCode.OK) { RequestMessage = request },
            });

            var mockHttpMessageHandler = new Moq.Mock<HttpMessageHandler>(Moq.MockBehavior.Loose);
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>())
                .Returns(() => Task.FromResult(httpResponseQueue.TryDequeue(out HttpResponseMessage r) ? r : new(HttpStatusCode.OK) { RequestMessage = request }))
                .Verifiable();

            RetryHandler retryHandler = new(mockHttpMessageHandler.Object, new RetryHandlerOption()
            {
                ShouldRetry = (delay, attempt, httpResponseMessage) => httpResponseMessage.StatusCode == HttpStatusCode.BadGateway,
                MaxRetry = expectedMaxRetry,
                Delay = 0
            });

            HttpMessageInvoker httpMessageInvoker = new(retryHandler);

            // Act
            var response = await httpMessageInvoker.SendAsync(request, new CancellationToken());

            // Assert
            mockHttpMessageHandler.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Exactly(1 + expectedMaxRetry), ItExpr.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>());
            Assert.True(response.StatusCode == expectedStatusCode, $"Expected {expectedStatusCode} but was {response.StatusCode}");
        }

        private async Task DelayTestWithMessage(HttpResponseMessage response, int count, string message, int delay = RetryHandlerOption.MAX_DELAY)
        {
            Message = message;
            await Task.Run(async () =>
            {
                await this.retryHandler.Delay(response, count, delay, out double delayInSeconds, new CancellationToken());
                Message += " Work " + count.ToString();
            });
        }

        public string Message { get; private set; }
    }
}