// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using Mocks;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net;
    using Xunit;
    using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;

    public class GraphClientFactoryTests : IDisposable
    {
        private MockRedirectHandler testHttpMessageHandler;
        private DelegatingHandler[] handlers;
        private const string expectedAccessToken = "graph-client-factory-infused-token";

        public GraphClientFactoryTests()
        {
            this.testHttpMessageHandler = new MockRedirectHandler();
            handlers = GraphClientFactory.CreateDefaultHandlers().ToArray();
        }

        public void Dispose()
        {
            this.testHttpMessageHandler.Dispose();
        }

        // Note:
        // 1. Xunit's IsType doesn't consider inheritance behind the classes.
        // 2. We can't control the order of execution for the tests
        // and 'GraphClientFactory.DefaultHttpHandler' can easily be modified
        // by other tests since it's a static delegate.

#if iOS || macOS
        [Fact]
        public void Should_CreatePipeline_Without_CompressionHandler()
        {
            using (RetryHandler retryHandler = (RetryHandler)GraphClientFactory.CreatePipeline(handlers))
            using (RedirectHandler redirectHandler = (RedirectHandler)retryHandler.InnerHandler)
#if iOS
            using (NSUrlSessionHandler innerMost = (NSUrlSessionHandler)redirectHandler.InnerHandler)
#elif macOS
            using (Foundation.NSUrlSessionHandler innerMost = (Foundation.NSUrlSessionHandler)redirectHandler.InnerHandler)
#endif
            {
                Assert.NotNull(retryHandler);
                Assert.NotNull(redirectHandler);
                Assert.NotNull(innerMost);
                Assert.IsType<RetryHandler>(retryHandler);
                Assert.IsType<RedirectHandler>(redirectHandler);
#if iOS
                Assert.IsType<NSUrlSessionHandler>(innerMost);
#elif macOS
                Assert.IsType<Foundation.NSUrlSessionHandler>(innerMost);
#endif
            }
        }
#else
        [Fact]
        public void Should_CreatePipeline_Without_HttpMessageHandlerInput()
        {
            using GraphTelemetryHandler telemetryHandler = (GraphTelemetryHandler)GraphClientFactory.CreatePipeline(handlers);
            using ParametersNameDecodingHandler odataQueryHandler = (ParametersNameDecodingHandler)telemetryHandler.InnerHandler;
            using CompressionHandler compressionHandler = (CompressionHandler)odataQueryHandler.InnerHandler;
            using RetryHandler retryHandler = (RetryHandler)compressionHandler.InnerHandler;
            using RedirectHandler redirectHandler = (RedirectHandler)retryHandler.InnerHandler;
            using HttpMessageHandler innerMost = redirectHandler.InnerHandler;

            Assert.NotNull(telemetryHandler);
            Assert.NotNull(odataQueryHandler);
            Assert.NotNull(compressionHandler);
            Assert.NotNull(retryHandler);
            Assert.NotNull(redirectHandler);
            Assert.NotNull(innerMost);
            Assert.IsType<GraphTelemetryHandler>(telemetryHandler);
            Assert.IsType<ParametersNameDecodingHandler>(odataQueryHandler);
            Assert.IsType<CompressionHandler>(compressionHandler);
            Assert.IsType<RetryHandler>(retryHandler);
            Assert.IsType<RedirectHandler>(redirectHandler);
            Assert.True(innerMost is HttpMessageHandler);
        }
#endif

        [Fact]
        public void CreatePipelineWithHttpMessageHandlerInput()
        {
            using GraphTelemetryHandler telemetryHandler = (GraphTelemetryHandler)GraphClientFactory.CreatePipeline(handlers, new MockRedirectHandler());
            using ParametersNameDecodingHandler odataQueryHandler = (ParametersNameDecodingHandler)telemetryHandler.InnerHandler;
            using CompressionHandler compressionHandler = (CompressionHandler)odataQueryHandler.InnerHandler;
            using RetryHandler retryHandler = (RetryHandler)compressionHandler.InnerHandler;
            using RedirectHandler redirectHandler = (RedirectHandler)retryHandler.InnerHandler;
            using MockRedirectHandler innerMost = (MockRedirectHandler)redirectHandler.InnerHandler;

            Assert.NotNull(telemetryHandler);
            Assert.NotNull(odataQueryHandler);
            Assert.NotNull(compressionHandler);
            Assert.NotNull(retryHandler);
            Assert.NotNull(redirectHandler);
            Assert.NotNull(innerMost);
            Assert.IsType<GraphTelemetryHandler>(telemetryHandler);
            Assert.IsType<ParametersNameDecodingHandler>(odataQueryHandler);
            Assert.IsType<CompressionHandler>(compressionHandler);
            Assert.IsType<RetryHandler>(retryHandler);
            Assert.IsType<RedirectHandler>(redirectHandler);
            Assert.IsType<MockRedirectHandler>(innerMost);
        }

        [Fact]
        public void CreatePipelineWithoutPipeline()
        {
            using (MockRedirectHandler handler = (MockRedirectHandler)GraphClientFactory.CreatePipeline(null, this.testHttpMessageHandler))
            {
                Assert.NotNull(handler);
                Assert.IsType<MockRedirectHandler>(handler);
            }
        }

        [Fact]
        public void CreatePipeline_Should_Throw_Exception_With_Duplicate_Handlers()
        {
            var handlers = GraphClientFactory.CreateDefaultHandlers();
            handlers.Add(new CompressionHandler());

            ArgumentException exception =  Assert.Throws<ArgumentException>(() => GraphClientFactory.CreatePipeline(handlers));

            Assert.Contains($"{typeof(CompressionHandler)} has a duplicate handler.", exception.Message);
        }

        [Fact]
        public void CreateClient_CustomHttpHandlingBehaviors()
        {
            var timeout = TimeSpan.FromSeconds(200);
            var baseAddress = new Uri("https://localhost");
            var cacheHeader = new CacheControlHeaderValue();
            
            using (HttpClient client = GraphClientFactory.Create())
            {
                client.Timeout = timeout;
                client.BaseAddress = baseAddress;
                Assert.NotNull(client);
                Assert.Equal(client.Timeout, timeout);
                Assert.Equal(client.BaseAddress, baseAddress);
            }
        }

        [Fact]
        public void CreateClient_SelectedCloud()
        {
            using (HttpClient httpClient = GraphClientFactory.Create(version: "beta", nationalCloud: GraphClientFactory.Germany_Cloud))
            {
                Assert.NotNull(httpClient);
                Uri clouldEndpoint = new Uri("https://graph.microsoft.de/beta/");
                Assert.Equal(httpClient.BaseAddress, clouldEndpoint);
                Assert.Equal(httpClient.Timeout, TimeSpan.FromSeconds(100));
            }
        }

        [Fact]
        public void CreateClient_SelectedCloudWithExceptions()
        {
            string nation = "Canada";
            try
            {
                HttpClient httpClient = GraphClientFactory.Create(nationalCloud: nation);
            }
            catch (ArgumentException exception)
            {
                Assert.IsType<ArgumentException>(exception);
                Assert.Equal(exception.Message, String.Format("{0} is an unexpected national cloud.", nation));
            }
        }

        [Fact]
        public void CreateClient_WithHandlers()
        {
            using (HttpClient client = GraphClientFactory.Create(handlers: GraphClientFactory.CreateDefaultHandlers()))
            {
                Assert.NotNull(client);
            }
        }

        [Fact]
        public async Task SendRequest_Redirect()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo");
            var redirectResponse = new HttpResponseMessage(HttpStatusCode.MovedPermanently);

            redirectResponse.Headers.Location = new Uri("http://example.org/bar");
            var oKResponse = new HttpResponseMessage(HttpStatusCode.OK);
            this.testHttpMessageHandler.SetHttpResponse(redirectResponse, oKResponse);

            using (HttpClient client = GraphClientFactory.Create(finalHandler: this.testHttpMessageHandler))
            {
                var response = await client.SendAsync(httpRequestMessage, new CancellationToken());
                Assert.Equal(response, oKResponse);
                Assert.Equal(response.RequestMessage.Method, httpRequestMessage.Method);
                Assert.NotSame(response.RequestMessage, httpRequestMessage);
            }

        }

        [Fact]
        public async Task SendRequest_Retry()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://example.org/foo");
            httpRequestMessage.Content = new StringContent("Hello World");

            var retryResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            retryResponse.Headers.TryAddWithoutValidation("Retry-After", 30.ToString());
            var response_2 = new HttpResponseMessage(HttpStatusCode.OK);

            this.testHttpMessageHandler.SetHttpResponse(retryResponse, response_2);

            using (HttpClient client = GraphClientFactory.Create(finalHandler: this.testHttpMessageHandler))
            {
                var response = await client.SendAsync(httpRequestMessage, new CancellationToken());
                Assert.Same(response, response_2);
                IEnumerable<string> values;
                Assert.True(response.RequestMessage.Headers.TryGetValues("Retry-Attempt", out values), "Don't set Retry-Attemp Header");
                Assert.Single(values);
                Assert.Equal(values.First(), 1.ToString());
                Assert.NotSame(response.RequestMessage, httpRequestMessage);
            }

        }

        [Fact]
        public void CreateClient_WithHandlersHasExceptions()
        {
            var pipelineHandlers = GraphClientFactory.CreateDefaultHandlers().ToArray();
            pipelineHandlers[pipelineHandlers.Length - 1] = null;
            try
            {
                HttpClient client = GraphClientFactory.Create(handlers: pipelineHandlers);
            }
            catch (ArgumentNullException exception)
            {
                Assert.IsType<ArgumentNullException>(exception);
                Assert.Equal("handlers", exception.ParamName);
            }
        }

        [Fact]
        public void CreateClient_WithInnerHandlerReference()
        {
            DelegatingHandler[] handlers = new DelegatingHandler[1];
            handlers[0] = new RetryHandler() 
            {
                InnerHandler = this.testHttpMessageHandler
            };
            // Creation should ignore the InnerHandler on RetryHandler
            HttpClient client = GraphClientFactory.Create(handlers: handlers);
            Assert.NotNull(client);
            Assert.IsType<HttpClientHandler>(handlers[0].InnerHandler);
        }

        [Fact]
        public void CreatePipelineWithFeatureFlags_Should_Set_FeatureFlag_For_Default_Handlers()
        {
            FeatureFlag expectedFlag = FeatureFlag.CompressionHandler | FeatureFlag.RetryHandler | FeatureFlag.RedirectHandler;
            string expectedFlagHeaderValue = Enum.Format(typeof(FeatureFlag), expectedFlag, "x");
            var handlers = GraphClientFactory.CreateDefaultHandlers();
            var pipelineWithHandlers = GraphClientFactory.CreatePipelineWithFeatureFlags(handlers);

            Assert.NotNull(pipelineWithHandlers.Pipeline);
            Assert.True(pipelineWithHandlers.FeatureFlags.HasFlag(expectedFlag));
        }

        [Fact]
        public void CreatePipelineWithFeatureFlags_Should_Set_FeatureFlag_For_Speficied_Handlers()
        {
            FeatureFlag expectedFlag = FeatureFlag.CompressionHandler | FeatureFlag.RetryHandler;
            var handlers = GraphClientFactory.CreateDefaultHandlers();
            //Exclude the redirect handler for this test
            handlers = handlers.Where(handler => !handler.GetType().Equals(typeof(RedirectHandler))).ToList();
            var pipelineWithHandlers = GraphClientFactory.CreatePipelineWithFeatureFlags(handlers);

            Assert.NotNull(pipelineWithHandlers.Pipeline);
            Assert.True(pipelineWithHandlers.FeatureFlags.HasFlag(expectedFlag));
        }
    }
}
