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
        private DelegatingHandler[] handlers = new DelegatingHandler[3];
        private MockRedirectHandler testHttpMessageHandler;


        [TestInitialize]
        public void Setup()
        {
            this.testHttpMessageHandler = new MockRedirectHandler();
            handlers[0] = new RetryHandler();
            handlers[1] = new RedirectHandler();
            handlers[2] = new AuthenticationHandler();
        }

        [TestCleanup]
        public void Teardown()
        {
            this.testHttpMessageHandler.Dispose();
        }

        [TestMethod]
        public void CreatePipelineWithoutHttpMessageHandlerInput()
        {
            using (RetryHandler retryHandler = (RetryHandler)GraphClientFactory.CreatePipeline(null, handlers))
            using (RedirectHandler redirectHandler = (RedirectHandler)retryHandler.InnerHandler)
            using (AuthenticationHandler authenticationHandler = (AuthenticationHandler) redirectHandler.InnerHandler)
            using (HttpMessageHandler innerMost = authenticationHandler.InnerHandler)
            {
                Assert.IsNotNull(retryHandler, "Create a middleware pipeline failed.");
                Assert.IsNotNull(redirectHandler, "Create a middleware pipeline failed.");
                Assert.IsNotNull(authenticationHandler, "Create a middleware pipeline failed");
                Assert.IsNotNull(innerMost, "Create inner most HttpMessageHandler failed.");
                Assert.IsInstanceOfType(retryHandler, typeof(RetryHandler), "Pass pipeline failed in first level.");
                Assert.IsInstanceOfType(redirectHandler, typeof(RedirectHandler), "Pass pipeline failed in seconde level.");
                Assert.IsInstanceOfType(authenticationHandler, typeof(AuthenticationHandler), "Pass pipeline failed in third level.");
                Assert.IsInstanceOfType(innerMost, typeof(HttpMessageHandler), "Inner most HttpMessageHandler class error.");
            }

        }

        [TestMethod]
        public void CreatePipelineWithHttpMessageHandlerInput()
        {
            using (RetryHandler retryHandler = (RetryHandler)GraphClientFactory.CreatePipeline(this.testHttpMessageHandler, handlers))
            using (RedirectHandler redirectHandler = (RedirectHandler)retryHandler.InnerHandler)
            using (AuthenticationHandler authenticationHandler = (AuthenticationHandler)redirectHandler.InnerHandler)
            using (MockRedirectHandler innerMost = (MockRedirectHandler)authenticationHandler.InnerHandler)
            {
                Assert.IsNotNull(retryHandler, "Create a middleware pipeline failed.");
                Assert.IsNotNull(redirectHandler, "Create a middleware pipeline failed.");
                Assert.IsNotNull(authenticationHandler, "Create a middleware pipeline failed.");
                Assert.IsNotNull(innerMost, "Create inner most HttpMessageHandler failed.");
                Assert.IsInstanceOfType(retryHandler, typeof(RetryHandler), "Pass pipeline failed in first level.");
                Assert.IsInstanceOfType(redirectHandler, typeof(RedirectHandler), "Pass pipeline failed in seconde level.");
                Assert.IsInstanceOfType(authenticationHandler, typeof(AuthenticationHandler), "Pass pipeline failed in third level.");
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
            using (HttpClient client = GraphClientFactory.CreateClient(webProxy, handlers))
            {
                GraphClientFactory.Configure(client, timeout, baseAddress, cacheHeader);
                Assert.IsNotNull(client, "Create Http client failed.");
                Assert.AreEqual(client.Timeout, timeout, "Unexpected default timeout set.");
                Assert.IsFalse(client.DefaultRequestHeaders.CacheControl.NoCache, "NoCache true.");
                Assert.IsFalse(client.DefaultRequestHeaders.CacheControl.NoStore, "NoStore true.");
                Assert.AreEqual(client.BaseAddress, baseAddress, "Unexpected default baseAddress set.");
            }
        }

        [TestMethod]
        public void CreateClient_SelectedCloudAndVersion()
        {
            GraphClientFactory.Version = "beta";
            using (HttpClient httpClient = GraphClientFactory.CreateClient(GraphClientFactory.Germany_Cloud, handlers))
            {
                Assert.IsNotNull(httpClient, "Create Http client failed.");
                Uri clouldEndpoint = new Uri("https://graph.microsoft.de/beta");
                Assert.AreEqual(httpClient.BaseAddress, clouldEndpoint, "Unexpected endpoint set.");
                Assert.AreEqual(httpClient.Timeout, TimeSpan.FromSeconds(100), "Default timeout not set.");
            }
        }

        [TestMethod]
        public void CreateClient_SelectedCloudWithExceptions()
        {
            string nation = "Canada";
            try
            {
                HttpClient httpClient = GraphClientFactory.CreateClient(nation, handlers);
            }
            catch (ArgumentException exception)
            {
                Assert.IsInstanceOfType(exception, typeof(ArgumentException), "Eeception is not the right type");
                Assert.AreEqual(exception.Message, String.Format("{0} is an unexpected national cloud.", nation));
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

        [TestMethod]
        public async Task SendRequest_UnauthorizedWithNoAuthenticationProvider()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, "https://example.com/bar");
            httpRequestMessage.Content = new StringContent("Hello World");

            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var okResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, okResponse);

            using (HttpClient client = GraphClientFactory.CreateClient(testHttpMessageHandler, handlers))
            {
                var response = await client.SendAsync(httpRequestMessage, new CancellationToken());
                Assert.AreSame(response, unauthorizedResponse);
                Assert.AreSame(response.RequestMessage, httpRequestMessage);
            }
        }

        [TestMethod]
        public async Task SendRequest_UnauthorizedWithAuthenticationProvider()
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, "https://example.com/bar");
            httpRequestMessage.Content = new StringContent("Hello World");

            var unauthorizedResponse = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var okResponse = new HttpResponseMessage(HttpStatusCode.OK);

            testHttpMessageHandler.SetHttpResponse(unauthorizedResponse, okResponse);

            handlers[2] = new AuthenticationHandler(new MockAuthenticationProvider().Object);

            using (HttpClient client = GraphClientFactory.CreateClient(testHttpMessageHandler, handlers))
            {
                var response = await client.SendAsync(httpRequestMessage, new CancellationToken());
                Assert.AreSame(response, okResponse);
                Assert.AreSame(response.RequestMessage, httpRequestMessage);
            }
        }

        [TestMethod]
        public void CreateClient_WithHandlersHasExceptions()
        {
            handlers[handlers.Length - 1] = null;
            try
            {
                HttpClient client = GraphClientFactory.CreateClient(handlers);
            }
            catch (ArgumentNullException exception)
            {
                Assert.IsInstanceOfType(exception, typeof(ArgumentNullException), "Exception is not the right type");
                Assert.AreEqual(exception.ParamName, "handlers", "ParamName not right.");
            }
            handlers[handlers.Length - 1] = new RetryHandler(this.testHttpMessageHandler);
            try
            {
                HttpClient client = GraphClientFactory.CreateClient(handlers);
            }
            catch (ArgumentException exception)
            {
                Assert.IsInstanceOfType(exception, typeof(ArgumentException), "Exception is not the right type");
                Assert.AreEqual(
                    exception.Message,
                    String.Format("DelegatingHandler array has unexpected InnerHandler. {0} has unexpected InnerHandler.",
                    handlers[handlers.Length - 1]));

            }

        }
    }
}
