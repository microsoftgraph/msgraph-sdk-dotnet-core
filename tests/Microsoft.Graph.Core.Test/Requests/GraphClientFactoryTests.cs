// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Test.Requests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    [TestClass]
    public class GraphClientFactoryTests
    {
        private DelegatingHandler[] handlers = new DelegatingHandler[2];
        private MockRedirectHandler testHttpMessageHandler;


        [TestInitialize]
        public void Setup()
        {
            this.testHttpMessageHandler = new MockRedirectHandler();
            handlers[0] = new RetryHandler();
            handlers[1] = new RedirectHandler();

        }

        [TestCleanup]
        public void Teardown()
        {
            this.testHttpMessageHandler.Dispose();
        }

        [TestMethod]
        public void CreatePipelineWithoutHttpMessageHandlerInput()
        {
            using (RetryHandler handler = (RetryHandler)GraphClientFactory.CreatePipeline(null, handlers))
            using (RedirectHandler inner = (RedirectHandler)handler.InnerHandler)
            using (HttpMessageHandler innerMost = inner.InnerHandler)
            {
                Assert.IsNotNull(handler, "Create a middleware pipeline failed.");
                Assert.IsNotNull(inner, "Create a middleware pipeline failed.");
                Assert.IsNotNull(innerMost, "Create inner most HttpMessageHandler failed.");
                Assert.IsInstanceOfType(handler, typeof(RetryHandler), "Pass pipeline failed in first level.");
                Assert.IsInstanceOfType(inner, typeof(RedirectHandler), "Pass pipeline failed in seconde level.");
                Assert.IsInstanceOfType(innerMost, typeof(HttpMessageHandler), "Inner most HttpMessageHandler class error.");
            }

        }

        [TestMethod]
        public void CreatePipelineWithHttpMessageHandlerInput()
        {
            using (RetryHandler handler = (RetryHandler)GraphClientFactory.CreatePipeline(this.testHttpMessageHandler, handlers))
            using (RedirectHandler inner = (RedirectHandler)handler.InnerHandler)
            using (MockRedirectHandler innerMost = (MockRedirectHandler)inner.InnerHandler)
            {
                Assert.IsNotNull(handler, "Create a middleware pipeline failed.");
                Assert.IsNotNull(inner, "Create a middleware pipeline failed.");
                Assert.IsNotNull(innerMost, "Create inner most HttpMessageHandler failed.");
                Assert.IsInstanceOfType(handler, typeof(RetryHandler), "Pass pipeline failed in first level.");
                Assert.IsInstanceOfType(inner, typeof(RedirectHandler), "Pass pipeline failed in seconde level.");
                Assert.IsInstanceOfType(innerMost, typeof(MockRedirectHandler), "Inner most HttpMessageHandler class error.");
            }

        }


        [TestMethod]
        public void CreatePipelineWithoutPipeline()
        {
            using (MockRedirectHandler handler = (MockRedirectHandler)GraphClientFactory.CreatePipeline(this.testHttpMessageHandler, null))
            {
                Assert.IsNotNull(handler, "Create a middleware pipeline failed.");
                Assert.IsInstanceOfType(handler, typeof(MockRedirectHandler), "Inner most HttpMessageHandler class error.");
            }
        }

        [TestMethod]
        public void CreateClient_CustomHttpHandlingBehaviors()
        {
            var timeout = TimeSpan.FromSeconds(200);
            var baseAddress = new Uri("https://localhost");
            var cacheHeader = new CacheControlHeaderValue();
            var webProxy = new WebProxy("http://127.0.0.1:8888");
            using (HttpClient client = GraphClientFactory.CreateClient(timeout, baseAddress, cacheHeader, webProxy, handlers))
            {
                Assert.IsNotNull(client, "Create Http client failed.");
                Assert.AreEqual(client.Timeout, timeout, "Unexpected default timeout set.");
                Assert.IsFalse(client.DefaultRequestHeaders.CacheControl.NoCache, "NoCache true.");
                Assert.IsFalse(client.DefaultRequestHeaders.CacheControl.NoStore, "NoStore true.");
                Assert.AreEqual(client.BaseAddress, baseAddress, "Unexpected default baseAddress set.");
                

            }
        }

        [TestMethod]
        public void CreateClient_SelectedCloud()
        {

            using (HttpClient httpClient = GraphClientFactory.CreateClient(GraphServieCloudList.Germany, handlers))
            {
                Assert.IsNotNull(httpClient, "Create Http client failed.");
                Uri clouldEndpoint = new Uri("https://graph.microsoft.de/v1.0");
                Assert.AreEqual(httpClient.BaseAddress, clouldEndpoint, "Unexpected endpoint set.");
                Assert.AreEqual(httpClient.Timeout, TimeSpan.FromSeconds(100), "Default timeout not set.");
            }
        }

        [TestMethod]
        public void CreateClient_WithInnerHandler()
        {

            using (HttpClient httpClient = GraphClientFactory.CreateClient(this.testHttpMessageHandler, null))
            {
                Assert.IsNotNull(httpClient, "Create Http client failed.");
                Assert.IsTrue(httpClient.DefaultRequestHeaders.Contains(CoreConstants.Headers.SdkVersionHeaderName), "SDK version not set.");
                Version assemblyVersion = typeof(GraphClientFactory).GetTypeInfo().Assembly.GetName().Version;
                string value = string.Format(
                    CoreConstants.Headers.SdkVersionHeaderValueFormatString,
                    "Graph",
                    assemblyVersion.Major,
                    assemblyVersion.Minor,
                    assemblyVersion.Build);
                IEnumerable<string> values;
                Assert.IsTrue(httpClient.DefaultRequestHeaders.TryGetValues(CoreConstants.Headers.SdkVersionHeaderName, out values), "SDK version value not set");
                Assert.AreEqual(values.Count(), 1);
                Assert.AreEqual(values.First(), value);
            }
        }


        [TestMethod]
        public void CreateClient_WithHandlers()
        {
            using (HttpClient client = GraphClientFactory.CreateClient(handlers))
            {
                Assert.IsNotNull(client, "Create Http client failed.");

            }

        }

        [TestMethod]
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
                Assert.AreEqual(response, oKResponse, "Middleware pipeline not work.");
                Assert.AreEqual(response.RequestMessage.Method, httpRequestMessage.Method);
                Assert.AreNotSame(response.RequestMessage, httpRequestMessage);
            }

        }

        [TestMethod]
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
                Assert.AreSame(response, response_2);
                IEnumerable<string> values;
                Assert.IsTrue(httpRequestMessage.Headers.TryGetValues("Retry-Attempt", out values), "Don't set Retry-Attemp Header");
                Assert.AreEqual(values.Count(), 1);
                Assert.AreEqual(values.First(), 1.ToString());
            }

        }


    }
}
