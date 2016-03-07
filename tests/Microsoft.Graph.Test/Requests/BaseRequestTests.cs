// ------------------------------------------------------------------------------
//  Copyright (c) 2016 Microsoft Corporation
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Test.Requests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Graph;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BaseRequestTests : RequestTestBase
    {
        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void BaseRequest_InitializeWithEmptyBaseUrl()
        {
            try
            {
                var baseRequest = new BaseRequest(null, this.graphServiceClient);
            }
            catch (ServiceException exception)
            {
                Assert.AreEqual(GraphErrorCode.InvalidRequest.ToString(), exception.Error.Code, "Unexpected error code.");
                Assert.AreEqual("Base URL is not initialized for the request.", exception.Error.Message, "Unexpected error message.");
                throw;
            }
        }

        [TestMethod]
        public void BaseRequest_InitializeWithQueryStringAndOptions()
        {
            var baseUrl = string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/drive/items/id";
            var requestUrl = baseUrl + "?key=value&key2";

            var options = new List<Option>
            {
                new QueryOption("key3", "value3"),
                new HeaderOption("header", "value"),
            };

            var baseRequest = new BaseRequest(requestUrl, this.graphServiceClient, options);

            Assert.AreEqual(new Uri(baseUrl), new Uri(baseRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual(3, baseRequest.QueryOptions.Count, "Unexpected number of query options.");
            Assert.IsTrue(baseRequest.QueryOptions[0].Name.Equals("key") && baseRequest.QueryOptions[0].Value.Equals("value"), "Unexpected first query option.");
            Assert.IsTrue(baseRequest.QueryOptions[1].Name.Equals("key2") && string.IsNullOrEmpty(baseRequest.QueryOptions[1].Value), "Unexpected second query option.");
            Assert.IsTrue(baseRequest.QueryOptions[2].Name.Equals("key3") && baseRequest.QueryOptions[2].Value.Equals("value3"), "Unexpected third query option.");
            Assert.AreEqual(1, baseRequest.Headers.Count, "Unexpected number of header options.");
            Assert.IsTrue(baseRequest.Headers[0].Name.Equals("header") && baseRequest.Headers[0].Value.Equals("value"), "Unexpected header option.");
        }

        [TestMethod]
        public void GetWebRequestWithHeadersAndQueryOptions()
        {
            var requestUrl = string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/drive/items/id";

            var options = new List<Option>
            {
                new HeaderOption("header1", "value1"),
                new HeaderOption("header2", "value2"),
                new QueryOption("query1", "value1"),
                new QueryOption("query2", "value2"),
            };

            var baseRequest = new BaseRequest(requestUrl, this.graphServiceClient, options) { Method = "PUT" };

            var httpRequestMessage = baseRequest.GetHttpRequestMessage();
            Assert.AreEqual(HttpMethod.Put, httpRequestMessage.Method, "Unexpected HTTP method in request.");
            Assert.AreEqual(requestUrl + "?query1=value1&query2=value2",
                httpRequestMessage.RequestUri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Port, UriFormat.Unescaped),
                "Unexpected base URL in request.");
            Assert.AreEqual("value1", httpRequestMessage.Headers.GetValues("header1").First(), "Unexpected first header in request.");
            Assert.AreEqual("value2", httpRequestMessage.Headers.GetValues("header2").First(), "Unexpected second header in request.");

            var expectedVersionNumber = typeof(BaseRequest).GetTypeInfo().Assembly.GetName().Version;
            Assert.AreEqual(
                string.Format(Constants.Headers.SdkVersionHeaderValue, expectedVersionNumber),
                httpRequestMessage.Headers.GetValues(Constants.Headers.SdkVersionHeaderName).First(), "Unexpected request stats header.");
        }

        [TestMethod]
        public void GetWebRequestNoOptions()
        {
            var requestUrl = string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/drive/items/id";

            var baseRequest = new BaseRequest(requestUrl, this.graphServiceClient) { Method = "DELETE" };

            var httpRequestMessage = baseRequest.GetHttpRequestMessage();
            Assert.AreEqual(HttpMethod.Delete, httpRequestMessage.Method, "Unexpected HTTP method in request.");
            Assert.AreEqual(requestUrl,
                httpRequestMessage.RequestUri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Port, UriFormat.Unescaped),
                "Unexpected base URL in request.");
            Assert.AreEqual(1, httpRequestMessage.Headers.Count(), "Unexpected headers in request.");

            var expectedVersionNumber = typeof(BaseRequest).GetTypeInfo().Assembly.GetName().Version;
            Assert.AreEqual(
                string.Format(Constants.Headers.SdkVersionHeaderValue, expectedVersionNumber),
                httpRequestMessage.Headers.GetValues(Constants.Headers.SdkVersionHeaderName).First(), "Unexpected request stats header.");
        }

        [TestMethod]
        public async Task SendAsync()
        {
            var requestUrl = string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/drive/items/id";

            var baseRequest = new BaseRequest(requestUrl, this.graphServiceClient) { ContentType = "application/json" };

            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request =>
                                string.Equals(request.Content.Headers.ContentType.ToString(), "application/json")
                               && request.RequestUri.ToString().Equals(requestUrl))))
                        .Returns(Task.FromResult(httpResponseMessage));

                var expectedResponseItem = new DriveItem { Id = "id" };
                this.serializer.Setup(
                    serializer => serializer.SerializeObject(It.IsAny<string>()))
                    .Returns(string.Empty);
                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<DriveItem>(It.IsAny<string>()))
                    .Returns(expectedResponseItem);

                var responseItem = await baseRequest.SendAsync<DriveItem>("string", HttpCompletionOption.ResponseContentRead, CancellationToken.None);

                Assert.IsNotNull(responseItem, "DriveItem not returned.");
                Assert.AreEqual(expectedResponseItem.Id, responseItem.Id, "Unexpected item ID.");

                this.authenticationProvider.Verify(provider => provider.AuthenticateRequestAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public async Task SendAsync_AuthenticationProviderNotSet()
        {
            var client = new GraphServiceClient("https://localhost", null);

            var baseRequest = new BaseRequest("https://localhost", client);

            try
            {
                await baseRequest.SendAsync<DriveItem>("string", HttpCompletionOption.ResponseContentRead, CancellationToken.None);
            }
            catch (ServiceException exception)
            {
                Assert.AreEqual(GraphErrorCode.InvalidRequest.ToString(), exception.Error.Code, "Unexpected error code.");
                Assert.AreEqual("Authentication provider is required before sending a request.", exception.Error.Message, "Unexpected error message.");
                throw;
            }
        }

        [TestMethod]
        public async Task SendAsync_NoReturnObject()
        {
            var requestUrl = string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/drive/items/id";

            var baseRequest = new BaseRequest(requestUrl, this.graphServiceClient) { ContentType = "application/json" };

            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request =>
                                string.Equals(request.Content.Headers.ContentType.ToString(), "application/json")
                               && request.RequestUri.ToString().Equals(requestUrl))))
                        .Returns(Task.FromResult(httpResponseMessage));
                
                this.serializer.Setup(
                    serializer => serializer.SerializeObject(It.IsAny<string>()))
                    .Returns(string.Empty);

                await baseRequest.SendAsync("string", HttpCompletionOption.ResponseContentRead, CancellationToken.None);

                this.authenticationProvider.Verify(provider => provider.AuthenticateRequestAsync(It.IsAny<HttpRequestMessage>()), Times.Once);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public async Task SendAsync_RequestUrlNotSet()
        {
            var baseRequest = new BaseRequest("https://localhost", this.graphServiceClient);

            baseRequest.RequestUrl = null;

            try
            {
                await baseRequest.SendAsync<DriveItem>("string", HttpCompletionOption.ResponseContentRead, CancellationToken.None);
            }
            catch (ServiceException exception)
            {
                Assert.AreEqual(GraphErrorCode.InvalidRequest.ToString(), exception.Error.Code, "Unexpected error code.");
                Assert.AreEqual("Request URL is required to send a request.", exception.Error.Message, "Unexpected error message.");
                throw;
            }
        }

        [TestMethod]
        public void BuildQueryString_NullQueryOptions()
        {
            var baseRequest = new BaseRequest("https://localhost", this.graphServiceClient);

            baseRequest.QueryOptions = null;

            var queryString = baseRequest.BuildQueryString();

            Assert.IsNull(queryString, "Unexpected query string returned.");
        }
    }
}
