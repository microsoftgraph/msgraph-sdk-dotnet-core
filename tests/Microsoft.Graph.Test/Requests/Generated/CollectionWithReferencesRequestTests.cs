// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Test.Requests.Generated
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Graph;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CollectionWithReferencesRequestTests : RequestTestBase
    {
        /// <summary>
        /// Tests building a request for an entity collection that has a $ref navigation.
        /// </summary>
        [TestMethod]
        public void BuildRequest()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/groups/groupId/members");
            var membersCollectionRequestBuilder = this.graphServiceClient.Groups["groupId"].Members as GroupMembersCollectionWithReferencesRequestBuilder;

            Assert.IsNotNull(membersCollectionRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(membersCollectionRequestBuilder.RequestUrl), "Unexpected request URL.");

            var membersCollectionRequest = membersCollectionRequestBuilder.Request() as GroupMembersCollectionWithReferencesRequest;
            Assert.IsNotNull(membersCollectionRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(membersCollectionRequest.RequestUrl), "Unexpected request URL.");
        }

        /// <summary>
        /// Tests the GetAsync() method on the request for an entity collection that has a $ref navigation.
        /// </summary>
        [TestMethod]
        public async System.Threading.Tasks.Task GetAsync()
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                var nextQueryKey = "key";
                var nextQueryValue = "value";

                var requestUrl = string.Format("{0}/groups/groupId/members", this.graphBaseUrl);
                var nextPageRequestUrl = string.Format("{0}?{1}={2}", requestUrl, nextQueryKey, nextQueryValue);

                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().StartsWith(requestUrl)
                                && request.Method == HttpMethod.Get),
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None))
                    .Returns(System.Threading.Tasks.Task.FromResult(httpResponseMessage));

                var membersCollectionPage = new GroupMembersCollectionWithReferencesPage
                {
                    new User(),
                };

                var membersCollectionResponse = new GroupMembersCollectionWithReferencesResponse
                {
                    Value = membersCollectionPage,
                    AdditionalData = new Dictionary<string, object> { { "@odata.nextLink", nextPageRequestUrl } },
                };

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<GroupMembersCollectionWithReferencesResponse>(It.IsAny<string>()))
                    .Returns(membersCollectionResponse);

                var returnedCollectionPage = await this.graphServiceClient.Groups["groupId"].Members.Request().GetAsync() as GroupMembersCollectionWithReferencesPage;

                Assert.IsNotNull(returnedCollectionPage, "Collection page not returned.");
                Assert.AreEqual(membersCollectionPage, returnedCollectionPage, "Unexpected collection page returned.");
                Assert.AreEqual(
                    membersCollectionResponse.AdditionalData,
                    returnedCollectionPage.AdditionalData,
                    "Additional data not initialized on collection page.");

                var nextPageRequest = returnedCollectionPage.NextPageRequest as GroupMembersCollectionWithReferencesRequest;

                Assert.IsNotNull(nextPageRequest, "Next page request not returned.");
                Assert.AreEqual(new Uri(requestUrl), new Uri(nextPageRequest.RequestUrl), "Unexpected URL initialized for next page request.");
                Assert.AreEqual(1, nextPageRequest.QueryOptions.Count, "Unexpected query options initialized.");
                Assert.AreEqual(nextQueryKey, nextPageRequest.QueryOptions[0].Name, "Unexpected query option name initialized.");
                Assert.AreEqual(nextQueryValue, nextPageRequest.QueryOptions[0].Value, "Unexpected query option value initialized.");
            }
        }

#if false // This test can no longer run at this time since the Graph does not have a $ref navigation that allows expand.
        /// <summary>
        /// Tests the Expand() method on the request for an entity collection that has a $ref navigation.
        /// </summary>
        [TestMethod]
        public void Expand()
        {
            var expectedRequestUrl = string.Format("{0}/groups/groupId/members", this.graphBaseUrl);

            var groupMembersRequest = this.graphServiceClient.Groups["groupId"].Members.Request().Expand("value") as GroupMembersCollectionWithReferencesRequest;

            Assert.IsNotNull(groupMembersRequest, "Unexpected request.");
            Assert.AreEqual(new Uri(expectedRequestUrl), new Uri(groupMembersRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual(1, groupMembersRequest.QueryOptions.Count, "Unexpected number of query options.");
            Assert.AreEqual("$expand", groupMembersRequest.QueryOptions[0].Name, "Unexpected query option name.");
            Assert.AreEqual("value", groupMembersRequest.QueryOptions[0].Value, "Unexpected query option value.");
        }
#endif

#if false // This test can no longer run at this time since the Graph does not have a $ref navigation that allows select.
        /// <summary>
        /// Tests the Select() method on an entity collection that has a $ref navigation.
        /// </summary>
        [TestMethod]
        public void Select()
        {
            var expectedRequestUrl = string.Format("{0}/groups/groupId/members", this.graphBaseUrl);

            var groupMembersRequest = this.graphServiceClient.Groups["groupId"].Members.Request().Select("value") as GroupMembersCollectionWithReferencesRequest;

            Assert.IsNotNull(groupMembersRequest, "Unexpected request.");
            Assert.AreEqual(new Uri(expectedRequestUrl), new Uri(groupMembersRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual(1, groupMembersRequest.QueryOptions.Count, "Unexpected number of query options.");
            Assert.AreEqual("$select", groupMembersRequest.QueryOptions[0].Name, "Unexpected query option name.");
            Assert.AreEqual("value", groupMembersRequest.QueryOptions[0].Value, "Unexpected query option value.");
        }
#endif

        /// <summary>
        /// Tests the Top() method on an entity collection that has a $ref navigation.
        /// </summary>
        [TestMethod]
        public void Top()
        {
            var expectedRequestUrl = string.Format("{0}/groups/groupId/members", this.graphBaseUrl);

            var groupMembersRequest = this.graphServiceClient.Groups["groupId"].Members.Request().Top(1) as GroupMembersCollectionWithReferencesRequest;

            Assert.IsNotNull(groupMembersRequest, "Unexpected request.");
            Assert.AreEqual(new Uri(expectedRequestUrl), new Uri(groupMembersRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual(1, groupMembersRequest.QueryOptions.Count, "Unexpected number of query options.");
            Assert.AreEqual("$top", groupMembersRequest.QueryOptions[0].Name, "Unexpected query option name.");
            Assert.AreEqual("1", groupMembersRequest.QueryOptions[0].Value, "Unexpected query option value.");
        }

#if false // This test can no longer run at this time since the Graph does not have a $ref navigation that allows filter.
        /// <summary>
        /// Tests the Filter() method on an entity collection that has a $ref navigation.
        /// </summary>
        [TestMethod]
        public void Filter()
        {
            var expectedRequestUrl = string.Format("{0}/groups/groupId/members", this.graphBaseUrl);

            var groupMembersRequest = this.graphServiceClient.Groups["groupId"].Members.Request().Filter("value") as GroupMembersCollectionWithReferencesRequest;

            Assert.IsNotNull(groupMembersRequest, "Unexpected request.");
            Assert.AreEqual(new Uri(expectedRequestUrl), new Uri(groupMembersRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual(1, groupMembersRequest.QueryOptions.Count, "Unexpected number of query options.");
            Assert.AreEqual("$filter", groupMembersRequest.QueryOptions[0].Name, "Unexpected query option name.");
            Assert.AreEqual("value", groupMembersRequest.QueryOptions[0].Value, "Unexpected query option value.");
        }
#endif

#if false // This test can no longer run at this time since the Graph does not have a $ref navigation that allows skip.
        /// <summary>
        /// Tests the Skip() method on an entity collection that has a $ref navigation.
        /// </summary>
        [TestMethod]
        public void Skip()
        {
            var expectedRequestUrl = string.Format("{0}/groups/groupId/members", this.graphBaseUrl);

            var groupMembersRequest = this.graphServiceClient.Groups["groupId"].Members.Request().Skip(1) as GroupMembersCollectionWithReferencesRequest;

            Assert.IsNotNull(groupMembersRequest, "Unexpected request.");
            Assert.AreEqual(new Uri(expectedRequestUrl), new Uri(groupMembersRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual(1, groupMembersRequest.QueryOptions.Count, "Unexpected number of query options.");
            Assert.AreEqual("$skip", groupMembersRequest.QueryOptions[0].Name, "Unexpected query option name.");
            Assert.AreEqual("1", groupMembersRequest.QueryOptions[0].Value, "Unexpected query option value.");
        }
#endif

        /// <summary>
        /// Tests the OrderBy() method on an entity collection that has a $ref navigation.
        /// </summary>
        [TestMethod]
        public void OrderBy()
        {
            var expectedRequestUrl = string.Format("{0}/groups/groupId/members", this.graphBaseUrl);

            var groupMembersRequest = this.graphServiceClient.Groups["groupId"].Members.Request().OrderBy("value") as GroupMembersCollectionWithReferencesRequest;

            Assert.IsNotNull(groupMembersRequest, "Unexpected request.");
            Assert.AreEqual(new Uri(expectedRequestUrl), new Uri(groupMembersRequest.RequestUrl), "Unexpected request URL.");
            Assert.AreEqual(1, groupMembersRequest.QueryOptions.Count, "Unexpected number of query options.");
            Assert.AreEqual("$orderby", groupMembersRequest.QueryOptions[0].Name, "Unexpected query option name.");
            Assert.AreEqual("value", groupMembersRequest.QueryOptions[0].Value, "Unexpected query option value.");
        }
    }
}
