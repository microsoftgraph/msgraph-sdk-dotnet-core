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

    using Microsoft.Graph.Core;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ActionRequestTests : RequestTestBase
    {
        /// <summary>
        /// Tests that an exception is thrown when the first of required parameters passed to an action request is null (assignLicence).
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void MultipleRequiredParameters_FirstParameterNull()
        {
            var removeLicenses = new List<Guid> { new Guid() };

            try
            {
                var assignLicenseRequestBuilder = this.graphServiceClient.Me.AssignLicense(null, removeLicenses).Request();
            }
            catch (ServiceException serviceException)
            {
                Assert.IsTrue(serviceException.IsMatch(GraphErrorCode.InvalidRequest.ToString()), "Unexpected error code thrown.");
                Assert.AreEqual(
                    "addLicenses is a required parameter for this method request.",
                    serviceException.Error.Message,
                    "Unexpected error code thrown.");

                throw;
            }
        }

        /// <summary>
        /// Tests that an exception is thrown when the second of required parameters passed to an action request is null (assignLicence).
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void MultipleRequiredParameters_LastParameterNull()
        {
            var addLicenses = new List<AssignedLicense> { new AssignedLicense() };

            try
            {
                var assignLicenseRequestBuilder = this.graphServiceClient.Me.AssignLicense(addLicenses, null).Request();
            }
            catch (ServiceException serviceException)
            {
                Assert.IsTrue(serviceException.IsMatch(GraphErrorCode.InvalidRequest.ToString()), "Unexpected error code thrown.");
                Assert.AreEqual(
                    "removeLicenses is a required parameter for this method request.",
                    serviceException.Error.Message,
                    "Unexpected error code thrown.");

                throw;
            }
        }

        /// <summary>
        /// Tests the PostAsync() method for an action that returns a collection of primitives (checkMemberGroups).
        /// The action is also inherited from the base class.
        /// </summary>
        [TestMethod]
        public async Task PostAsync_CollectionOfPrimitivesReturnType()
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;
                
                var nextQueryKey = "key";
                var nextQueryValue = "value";

                var requestUrl = string.Format("{0}/me/microsoft.graph.checkMemberGroups", this.graphBaseUrl);
                var nextPageRequestUrl = string.Format("{0}?{1}={2}", requestUrl, nextQueryKey, nextQueryValue);

                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().StartsWith(requestUrl)
                                && request.Method == HttpMethod.Post),
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None))
                    .Returns(Task.FromResult(httpResponseMessage));

                var checkMemberGroupsCollectionPage = new DirectoryObjectCheckMemberGroupsCollectionPage
                {
                    "value 1",
                    "value 2",
                };

                var checkMemberGroupsCollectionResponse = new DirectoryObjectCheckMemberGroupsCollectionResponse
                {
                    Value = checkMemberGroupsCollectionPage,
                    AdditionalData = new Dictionary<string, object> { { "@odata.nextLink", nextPageRequestUrl } },
                };
                
                this.serializer.Setup(
                    serializer => serializer.SerializeObject(It.IsAny<DirectoryObjectCheckMemberGroupsRequestBody>()))
                    .Returns("request body string");

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<DirectoryObjectCheckMemberGroupsCollectionResponse>(It.IsAny<string>()))
                    .Returns(checkMemberGroupsCollectionResponse);

                var returnedCollectionPage = await this.graphServiceClient.Me.CheckMemberGroups(new List<string>()).Request().PostAsync();

                Assert.IsNotNull(returnedCollectionPage, "Collection page not returned.");
                Assert.AreEqual(checkMemberGroupsCollectionPage, returnedCollectionPage, "Unexpected collection page returned.");
                Assert.AreEqual(
                    checkMemberGroupsCollectionPage.AdditionalData,
                    returnedCollectionPage.AdditionalData,
                    "Additional data not initialized on collection page.");

                var nextPageRequest = returnedCollectionPage.NextPageRequest as DirectoryObjectCheckMemberGroupsRequest;

                Assert.IsNotNull(nextPageRequest, "Next page request not returned.");
                Assert.AreEqual(new Uri(requestUrl), new Uri(nextPageRequest.RequestUrl), "Unexpected URL initialized for next page request.");
                Assert.AreEqual(1, nextPageRequest.QueryOptions.Count, "Unexpected query options initialized.");
                Assert.AreEqual(nextQueryKey, nextPageRequest.QueryOptions[0].Name, "Unexpected query option name initialized.");
                Assert.AreEqual(nextQueryValue, nextPageRequest.QueryOptions[0].Value, "Unexpected query option value initialized.");
            }
        }

        /// <summary>
        /// Tests the PostAsync() method for an action that returns a single entity (createLink).
        /// </summary>
        [TestMethod]
        public async Task PostAsync_NonCollectionReturnType()
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
                            request => request.RequestUri.ToString().StartsWith(requestUrl)
                                && request.Method == HttpMethod.Post),
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None))
                    .Returns(Task.FromResult(httpResponseMessage));

                var expectedPermission = new Permission { Id = "id", Link = new SharingLink { Type = "edit" } };

                this.serializer.Setup(
                    serializer => serializer.SerializeObject(It.IsAny<DriveItemCreateLinkRequestBody>()))
                    .Returns("request body value");

                this.serializer.Setup(
                    serializer => serializer.DeserializeObject<Permission>(It.IsAny<string>()))
                    .Returns(expectedPermission);

                var permission = await this.graphServiceClient.Me.Drive.Items["id"].CreateLink("edit", "scope").Request().PostAsync();

                Assert.IsNotNull(permission, "Permission not returned.");
                Assert.AreEqual(expectedPermission, permission, "Unexpected permission returned.");
            }
        }

        /// <summary>
        /// Tests the PostAsync() method for an action that returns nothing (send).
        /// </summary>
        [TestMethod]
        public async Task PostAsync_NoReturnValue()
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                var requestUrl = string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/mailFolders/Drafts/messages/messageId/microsoft.graph.send";
                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().StartsWith(requestUrl)),
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None))
                    .Returns(Task.FromResult(httpResponseMessage));

                await this.graphServiceClient.Me.MailFolders.Drafts.Messages["messageId"].Send().Request().PostAsync();
            }
        }
    }
}