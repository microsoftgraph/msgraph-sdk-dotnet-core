// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Test.Requests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;

    [TestClass]
    public class RedirectHandlerTests
    {
        private TestHttpMessageHandler testHttpMessageHandler;
        private RedirectHandler redirectHandler;

        [TestInitialize]
        public void Setup()
        { 
            this.testHttpMessageHandler = new TestHttpMessageHandler();
            this.redirectHandler = new RedirectHandler(this.testHttpMessageHandler);
        }

        [TestMethod]
        public void RedirectHandler_HttpMessageHandlerConstructor()
        {
            //var redirectHandler = new RedirectHandler(httpMessageHandler);
            Assert.IsNotNull(this.redirectHandler.InnerHandler, "HttpMessageHandler not initialized.");
            Assert.AreEqual(this.redirectHandler.InnerHandler, this.testHttpMessageHandler, "Unexpected message handler set.");
            Assert.IsInstanceOfType(this.redirectHandler, typeof(RedirectHandler), "Unexpected redirect handler set.");
        }

        [TestMethod]
        public async Task OkStatusShouldPassThrough()
        {
            var redirectResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var invoker = CreateInvoker(redirectResponse);
            var request = CreateRequest(HttpMethod.Get);
            var response = await invoker.SendAsync(request, new CancellationToken());

            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            Assert.AreSame(response.RequestMessage, request);

        }

        [DataTestMethod]
        [DataRow(HttpStatusCode.MovedPermanently)]  // 301
        [DataRow(HttpStatusCode.Found)]  // 302
        [DataRow(HttpStatusCode.TemporaryRedirect)]  // 307
        public async Task ShouldRedirectSameMethodAndContent(HttpStatusCode statusCode)
        {
            var redirectResponse = new HttpResponseMessage(statusCode);
            redirectResponse.Headers.Location = new Uri("http://example.org/bar");

            var invoker = CreateInvoker(redirectResponse,
                                            new HttpResponseMessage(HttpStatusCode.OK));

            var request = CreateRequest(HttpMethod.Post);
            request.Content = new StringContent("Hello World");

            var response = await invoker.SendAsync(request, new CancellationToken());

            Assert.AreEqual(response.RequestMessage.Method, request.Method, "Http request method changes");
            Assert.AreNotSame(response.RequestMessage, request, "Doesn't reissue new http request");
            Assert.IsNotNull(response.RequestMessage.Content, "Request content is removed.");
            Assert.AreEqual(response.RequestMessage.Content.ReadAsStringAsync().Result, "Hello World", "Request content changes.");

        }

        [TestMethod]
        public async Task ShouldRedirectChangeMethodAndContent()
        {
            var redirectResponse = new HttpResponseMessage(HttpStatusCode.SeeOther);
            redirectResponse.Headers.Location = new Uri("http://example.org/bar");

            var invoker = CreateInvoker(redirectResponse,
                                            new HttpResponseMessage(HttpStatusCode.OK));

            var request = CreateRequest(HttpMethod.Post);
            request.Content = new StringContent("Hello World");

            var response = await invoker.SendAsync(request, new CancellationToken());

            Assert.AreNotEqual(response.RequestMessage.Method, request.Method, "Http request method doesn't change");
            Assert.AreEqual(response.RequestMessage.Method, HttpMethod.Get, "Http request method changes wrongly.");
            Assert.AreNotSame(response.RequestMessage, request, "Doesn't reissue new http request");
            Assert.IsNull(response.RequestMessage.Content, "Request content still exists.");
        }


        [TestMethod]
        public async Task RedirectWithSameHostShouldKeepAuthHeader()
        {
            var redirectResponse = new HttpResponseMessage(HttpStatusCode.Redirect);
            redirectResponse.Headers.Location = new Uri("http://example.org/bar");

            var invoker = CreateInvoker(redirectResponse,
                                            new HttpResponseMessage(HttpStatusCode.OK));

            var request = CreateRequest(HttpMethod.Post);
            request.Headers.Authorization = new AuthenticationHeaderValue("fooAuth", "barAuth");

            var response = await invoker.SendAsync(request, new CancellationToken());

            Assert.AreNotSame(response.RequestMessage, request, "Doesn't reissue new http request");
            Assert.AreEqual(response.RequestMessage.Headers.Authorization.Scheme, "fooAuth", "AuthHeader changes");

        }

        [DataTestMethod]
        [DataRow(HttpStatusCode.MovedPermanently)]  // 301
        [DataRow(HttpStatusCode.Found)]  // 302
        [DataRow(HttpStatusCode.SeeOther)]  //303
        [DataRow(HttpStatusCode.TemporaryRedirect)]  // 307
       // [DataRow(HttpStatusCode.PermanentRedirect)]  // 308
        public async Task RedirectWithDifferentHostShouldRemoveAuthHeader(HttpStatusCode statusCode)
        {
            var redirectResponse = new HttpResponseMessage(statusCode);
            redirectResponse.Headers.Location = new Uri("http://example.net/bar");

            var invoker = CreateInvoker(redirectResponse,
                                            new HttpResponseMessage(HttpStatusCode.OK));

            var request = CreateRequest(HttpMethod.Get);
            request.Headers.Authorization = new AuthenticationHeaderValue("fooAuth", "aparam");

            var response = await invoker.SendAsync(request, new CancellationToken());

            Assert.AreNotSame(response.RequestMessage, request, "Doesn't reissue a new http request");
            Assert.AreNotSame(response.RequestMessage.RequestUri.Host, request.RequestUri.Host, "Hosts are same.");
            Assert.IsNull(response.RequestMessage.Headers.Authorization, "Authorization doesn't be removed.");

        }

        [TestMethod]
        public async Task ExceedMaxRedirectsShouldReturn()
        {
            var _response1 = new HttpResponseMessage(HttpStatusCode.Found);
            _response1.Headers.Location = new Uri("http://example.org/bar");

            var _response2 = new HttpResponseMessage(HttpStatusCode.TemporaryRedirect);
            _response2.Headers.Location = new Uri("http://example.org/foo");

            var invoker = CreateInvoker(_response1, _response2);
            var request = CreateRequest(HttpMethod.Get);

            var response = await invoker.SendAsync(request, new CancellationToken());

            Assert.AreNotSame(response.RequestMessage, request, "Doesn't reissue a new http request");
            Assert.AreEqual(response.StatusCode, HttpStatusCode.TemporaryRedirect, "Redirect count doesn't match");
            Assert.AreNotEqual(response.StatusCode, HttpStatusCode.Found, "Redirect count doesn't match.");
        }



        static HttpRequestMessage CreateRequest(HttpMethod method)
        {
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.RequestUri = new Uri("http://example.org/foo");
            httpRequestMessage.Method = method;
            return httpRequestMessage;
        }

        static HttpMessageInvoker CreateInvoker(HttpResponseMessage httpResponseMessage1, HttpResponseMessage httpResponseMessage2 = null)
        {
            RedirectHandler redirectHandler = new RedirectHandler(new MockRedirectHander(httpResponseMessage1, httpResponseMessage2));
            var invoker = new HttpMessageInvoker(redirectHandler);
            return invoker;
        }

    }

    public class MockRedirectHander : HttpMessageHandler
    {
        readonly HttpResponseMessage _response1;
        readonly HttpResponseMessage _response2;
        private bool _response1Sent = false;

        public MockRedirectHander(HttpResponseMessage response1, HttpResponseMessage response2 = null)
        {
            _response1 = response1;
            _response2 = response2;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
           

            if (!_response1Sent)
            {
                _response1Sent = true;
                _response1.RequestMessage = request;
                return _response1;
            }
            else
            {
                _response1Sent = false;
                _response2.RequestMessage = request;
                return _response2;
            }
        }
    }
}

