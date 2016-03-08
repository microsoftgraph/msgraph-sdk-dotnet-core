// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Test.Requests.Extensions
{
    using System;

    using Microsoft.Graph;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UserRequestBuilderExtensionsTests : RequestTestBase
    {
        [TestMethod]
        public void ItemWithPath()
        {
            var graphBaseUrl = string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0");
            var itemPath = "/drive/root:/path/to/item";
            var expectedRequestUri = new Uri(string.Format("{0}/me{1}:", graphBaseUrl, itemPath));

            var driveItemRequestBuilder = this.graphServiceClient.Me.ItemWithPath(itemPath) as DriveItemRequestBuilder;

            Assert.IsNotNull(driveItemRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(driveItemRequestBuilder.RequestUrl), "Unexpected request URL.");
        }
    }
}
