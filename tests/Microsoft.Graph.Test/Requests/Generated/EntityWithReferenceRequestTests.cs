// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Test.Requests.Generated
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Graph;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EntityWithReferenceRequestTests : RequestTestBase
    {
        /// <summary>
        /// Tests building a request for an entity that has a $ref navigation.
        /// </summary>
        [TestMethod]
        public void BuildRequest()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/manager");
            var managerRequestBuilder = this.graphServiceClient.Me.Manager as DirectoryObjectWithReferenceRequestBuilder;

            Assert.IsNotNull(managerRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(managerRequestBuilder.RequestUrl), "Unexpected request URL.");

            var namagerRequest = managerRequestBuilder.Request() as DirectoryObjectWithReferenceRequest;
            Assert.IsNotNull(namagerRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(namagerRequest.RequestUrl), "Unexpected request URL.");
        }

        /// <summary>
        /// Tests the GetAsync() method on an entity that has a $ref navigation.
        /// </summary>
        [TestMethod]
        public async System.Threading.Tasks.Task GetAsync()
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                var requestUrl = string.Format("{0}/me/manager", this.graphBaseUrl);

                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().StartsWith(requestUrl)
                                && request.Method == HttpMethod.Get),
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None))
                    .Returns(System.Threading.Tasks.Task.FromResult(httpResponseMessage));

                var expectedManager = new User();

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<DirectoryObject>(It.IsAny<string>()))
                    .Returns(expectedManager);

                var returnedManager = await this.graphServiceClient.Me.Manager.Request().GetAsync();

                Assert.AreEqual(expectedManager, returnedManager, "Unexpected manager returned.");
            }
        }

#if false // This test can no longer run at this time since the Graph does not have a $ref navigation that allows expand.
        /// <summary>
        /// Tests the Expand() method on the request for an entity with a $ref navigation.
        /// </summary>
        [TestMethod]
        public void Expand()
        {
            var expectedRequestUrl = string.Format("{0}/me/manager", this.graphBaseUrl);

            var managerRequest = this.graphServiceClient.Me.Manager.Request().Expand("value") as DirectoryObjectWithReferenceRequest;

            Assert.IsNotNull(managerRequest, "Unexpected request.");
            Assert.AreEqual(new Uri(expectedRequestUrl), new Uri(managerRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual(1, managerRequest.QueryOptions.Count, "Unexpected number of query options.");
            Assert.AreEqual("$expand", managerRequest.QueryOptions[0].Name, "Unexpected query option name.");
            Assert.AreEqual("value", managerRequest.QueryOptions[0].Value, "Unexpected query option value.");
        }
#endif

#if false // This test can no longer run at this time since the Graph does not have a $ref navigation that allows select.
        /// <summary>
        /// Tests the Select() method on the request for an entity with a $ref navigation.
        /// </summary>
        [TestMethod]
        public void Select()
        {
            var expectedRequestUrl = string.Format("{0}/me/manager", this.graphBaseUrl);

            var managerRequest = this.graphServiceClient.Me.Manager.Request().Select("value") as DirectoryObjectWithReferenceRequest;

            Assert.IsNotNull(managerRequest, "Unexpected request.");
            Assert.AreEqual(new Uri(expectedRequestUrl), new Uri(managerRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual(1, managerRequest.QueryOptions.Count, "Unexpected number of query options.");
            Assert.AreEqual("$select", managerRequest.QueryOptions[0].Name, "Unexpected query option name.");
            Assert.AreEqual("value", managerRequest.QueryOptions[0].Value, "Unexpected query option value.");
        }
#endif
    }
}
