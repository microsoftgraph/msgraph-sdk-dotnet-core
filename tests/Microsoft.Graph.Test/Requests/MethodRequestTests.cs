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
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.Graph;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;

    [TestClass]
    public class MethodRequestTests : RequestTestBase
    {
        [TestMethod]
        public void ItemCreateLinkRequest_BuildRequest()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/drive/items/id/microsoft.graph.createLink");
            var createLinkRequestBuilder = this.graphServiceClient.Me.Drive.Items["id"].CreateLink("view") as DriveItemCreateLinkRequestBuilder;

            Assert.IsNotNull(createLinkRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(createLinkRequestBuilder.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual("view", createLinkRequestBuilder.Type, "Unexpected token.");

            var createLinkRequest = createLinkRequestBuilder.Request() as DriveItemCreateLinkRequest;
            Assert.IsNotNull(createLinkRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(createLinkRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual("POST", createLinkRequest.Method, "Unexpected method.");
            Assert.IsNotNull(createLinkRequest.RequestBody, "Request body not set.");
            Assert.AreEqual("view", createLinkRequest.RequestBody.Type, "Unexpected type in body.");
        }

        [TestMethod]
        public async Task ItemCreateLinkRequest_PostAsync()
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                var requestUrl = string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/drive/items/id/microsoft.graph.createLink";
                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().StartsWith(requestUrl))))
                    .Returns(Task.FromResult(httpResponseMessage));

                var expectedPermission = new Permission { Id = "id", Link = new SharingLink { Type = "edit" } };

                this.serializer.Setup(
                    serializer => serializer.SerializeObject(It.IsAny<DriveItemCreateLinkRequestBody>()))
                    .Returns("request body value");

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<Permission>(It.IsAny<string>()))
                    .Returns(expectedPermission);

                var permission = await this.graphServiceClient.Me.Drive.Items["id"].CreateLink("edit").Request().PostAsync();

                Assert.IsNotNull(permission, "Permission not returned.");
                Assert.AreEqual(expectedPermission, permission, "Unexpected permission returned.");
            }
        }

        [TestMethod]
        public void ItemSearch_BuildRequest()
        {
            var expectedRequesBuilderUri = new Uri(string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/drive/items/id/microsoft.graph.search");
            var expectedRequestUri = new Uri(string.Concat(expectedRequesBuilderUri, "(q='query')"));
            var searchRequestBuilder = this.graphServiceClient.Me.Drive.Items["id"].Search("query") as DriveItemSearchRequestBuilder;

            Assert.IsNotNull(searchRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequesBuilderUri, new Uri(searchRequestBuilder.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual("query", searchRequestBuilder.Q, "Unexpected query value.");

            var searchRequest = searchRequestBuilder.Request() as DriveItemSearchRequest;
            Assert.IsNotNull(searchRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(searchRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual("GET", searchRequest.Method, "Unexpected method.");
        }

        [TestMethod]
        public async Task ItemSearch_GetAsync()
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                var requestUrl = string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/drive/items/id/microsoft.graph.search(q='query')";
                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().StartsWith(requestUrl))))
                    .Returns(Task.FromResult(httpResponseMessage));

                var expectedResponse = new DriveItemSearchCollectionResponse
                {
                    Value = new DriveItemSearchCollectionPage(),
                };

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<DriveItemSearchCollectionResponse>(It.IsAny<string>()))
                    .Returns(expectedResponse);

                var items = await this.graphServiceClient.Me.Drive.Items["id"].Search("query").Request().GetAsync();

                Assert.IsNotNull(items, "Items not returned.");
                Assert.AreEqual(expectedResponse.Value, items, "Unexpected items returned.");
            }
        }
    }
}
