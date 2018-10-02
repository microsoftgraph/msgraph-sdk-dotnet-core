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

    public class GraphClientFactoryTests : IDisposable
    {
        private DelegatingHandler[] handlers = new DelegatingHandler[2];
        private MockRedirectHandler testHttpMessageHandler;


        public GraphClientFactoryTests()
        {
            this.testHttpMessageHandler = new MockRedirectHandler();
            handlers[0] = new RetryHandler();
            handlers[1] = new RedirectHandler();
        }

        public void Dispose()
        {
            this.testHttpMessageHandler.Dispose();
        }

        [Fact]
        public void CreatePipelineWithoutHttpMessageHandlerInput()
        {
            using (RetryHandler handler = (RetryHandler)GraphClientFactory.CreatePipeline(null, handlers))
            using (RedirectHandler inner = (RedirectHandler)handler.InnerHandler)
            using (HttpClientHandler innerMost = (HttpClientHandler)inner.InnerHandler)
            {
                Assert.NotNull(handler);
                Assert.NotNull(inner);
                Assert.NotNull(innerMost);
                Assert.IsType(typeof(RetryHandler), handler);
                Assert.IsType(typeof(RedirectHandler), inner);
                Assert.IsType(typeof(HttpClientHandler), innerMost);
            }

        }

        [Fact]
        public void CreatePipelineWithHttpMessageHandlerInput()
        {
            using (RetryHandler handler = (RetryHandler)GraphClientFactory.CreatePipeline(this.testHttpMessageHandler, handlers))
            using (RedirectHandler inner = (RedirectHandler)handler.InnerHandler)
            using (MockRedirectHandler innerMost = (MockRedirectHandler)inner.InnerHandler)
            {
                Assert.NotNull(handler);
                Assert.NotNull(inner);
                Assert.NotNull(innerMost);
                Assert.IsType(typeof(RetryHandler), handler);
                Assert.IsType(typeof(RedirectHandler), inner);
                Assert.IsType(typeof(MockRedirectHandler), innerMost);
            }

        }


        [Fact]
        public void CreatePipelineWithoutPipeline()
        {
            using (MockRedirectHandler handler = (MockRedirectHandler)GraphClientFactory.CreatePipeline(this.testHttpMessageHandler, null))
            {
                Assert.NotNull(handler);
                Assert.IsType(typeof(MockRedirectHandler), handler);
            }
        }

        [Fact]
        public void CreateClient_CustomHttpHandlingBehaviors()
        {
            var timeout = TimeSpan.FromSeconds(200);
            var baseAddress = new Uri("https://localhost");
            var cacheHeader = new CacheControlHeaderValue();
            
            using (HttpClient client = GraphClientFactory.CreateClient(handlers))
            {
                GraphClientFactory.Configure(client, timeout, baseAddress, cacheHeader);
                Assert.NotNull(client);
                Assert.Equal(client.Timeout, timeout);
                Assert.False(client.DefaultRequestHeaders.CacheControl.NoCache, "NoCache true.");
                Assert.False(client.DefaultRequestHeaders.CacheControl.NoStore, "NoStore true.");
                Assert.Equal(client.BaseAddress, baseAddress);

            }
        }

        [Fact]
        public void CreateClient_SelectedCloud()
        {
            GraphClientFactory.Version = "beta";
            using (HttpClient httpClient = GraphClientFactory.CreateClient(GraphClientFactory.Germany_Cloud, handlers))
            {
                Assert.NotNull(httpClient);
                Uri clouldEndpoint = new Uri("https://graph.microsoft.de/beta");
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
                HttpClient httpClient = GraphClientFactory.CreateClient(nation, handlers);
            }
            catch (ArgumentException exception)
            {
                Assert.IsType(typeof(ArgumentException), exception);
                Assert.Equal(exception.Message, String.Format("{0} is an unexpected national cloud.", nation));
            }
        }

        [Fact]
        public void CreateClient_WithInnerHandler()
        {

            using (HttpClient httpClient = GraphClientFactory.CreateClient(this.testHttpMessageHandler, null))
            {
                Assert.NotNull(httpClient);
                Assert.True(httpClient.DefaultRequestHeaders.Contains(CoreConstants.Headers.SdkVersionHeaderName), "SDK version not set.");
                Version assemblyVersion = typeof(GraphClientFactory).GetTypeInfo().Assembly.GetName().Version;
                string value = string.Format(
                    CoreConstants.Headers.SdkVersionHeaderValueFormatString,
                    "Graph",
                    assemblyVersion.Major,
                    assemblyVersion.Minor,
                    assemblyVersion.Build);
                IEnumerable<string> values;
                Assert.True(httpClient.DefaultRequestHeaders.TryGetValues(CoreConstants.Headers.SdkVersionHeaderName, out values), "SDK version value not set");
                Assert.Equal(values.Count(), 1);
                Assert.Equal(values.First(), value);
            }
        }


        [Fact]
        public void CreateClient_WithHandlers()
        {
            using (HttpClient client = GraphClientFactory.CreateClient(handlers))
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

            using (HttpClient client = GraphClientFactory.CreateClient(this.testHttpMessageHandler, handlers))
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

            using (HttpClient client = GraphClientFactory.CreateClient(this.testHttpMessageHandler, handlers))
            {
                var response = await client.SendAsync(httpRequestMessage, new CancellationToken());
                Assert.Same(response, response_2);
                IEnumerable<string> values;
                Assert.True(httpRequestMessage.Headers.TryGetValues("Retry-Attempt", out values), "Don't set Retry-Attemp Header");
                Assert.Equal(values.Count(), 1);
                Assert.Equal(values.First(), 1.ToString());
            }

        }


        [Fact]
        public void CreateClient_WithHandlersHasExceptions()
        {
            handlers[1] = null;
            try
            {
                HttpClient client = GraphClientFactory.CreateClient(handlers);
            }
            catch (ArgumentNullException exception)
            {
                Assert.IsType(typeof(ArgumentNullException), exception);
                Assert.Equal(exception.ParamName, "handlers");
            }

            handlers[1] = new RetryHandler(this.testHttpMessageHandler);
            try
            {
                HttpClient client = GraphClientFactory.CreateClient(handlers);
            }
            catch (ArgumentException exception)
            {
                Assert.IsType(typeof(ArgumentException), exception);
                Assert.Equal(exception.Message, String.Format("DelegatingHandler array has unexpected InnerHandler. {0} has unexpected InnerHandler.", handlers[1]));

            }

        }
    }
}
