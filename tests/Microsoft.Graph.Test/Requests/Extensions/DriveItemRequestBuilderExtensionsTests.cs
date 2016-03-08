// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Test.Requests.Extensions
{
    using System;

    using Microsoft.Graph;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DriveItemRequestBuilderExtensionsTests : RequestTestBase
    {
        [TestMethod]
        public void ItemById_BuildRequest()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/drive/items/id");
            var itemRequestBuilder = this.graphServiceClient.Me.Drive.Items["id"] as DriveItemRequestBuilder;

            Assert.IsNotNull(itemRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(itemRequestBuilder.RequestUrl), "Unexpected request URL.");

            var itemRequest = itemRequestBuilder.Request() as DriveItemRequest;
            Assert.IsNotNull(itemRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(itemRequest.RequestUrl), "Unexpected request URL.");
        }

        [TestMethod]
        public void ItemByPath_BuildRequest()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/drive/root:/item/with/path:");
            var itemRequestBuilder = this.graphServiceClient.Me.Drive.Root.ItemWithPath("item/with/path") as DriveItemRequestBuilder;

            Assert.IsNotNull(itemRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(itemRequestBuilder.RequestUrl), "Unexpected request URL.");

            var itemRequest = itemRequestBuilder.Request() as DriveItemRequest;
            Assert.IsNotNull(itemRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(itemRequest.RequestUrl), "Unexpected request URL.");
        }

        [TestMethod]
        public void ItemByPath_BuildRequestWithLeadingSlash()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/drive/root:/item/with/path:");
            var itemRequestBuilder = this.graphServiceClient.Me.Drive.Root.ItemWithPath("/item/with/path") as DriveItemRequestBuilder;

            Assert.IsNotNull(itemRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(itemRequestBuilder.RequestUrl), "Unexpected request URL.");

            var itemRequest = itemRequestBuilder.Request() as DriveItemRequest;
            Assert.IsNotNull(itemRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(itemRequest.RequestUrl), "Unexpected request URL.");
        }
    }
}
