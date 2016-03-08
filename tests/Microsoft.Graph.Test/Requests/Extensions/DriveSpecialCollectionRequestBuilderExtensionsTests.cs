// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Test.Requests.Extensions
{
    using System;

    using Microsoft.Graph;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DriveSpecialCollectionRequestBuilderExtensionsTests : RequestTestBase
    {
        [TestMethod]
        public void AppRoot()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/drive/special/approot");
            var driveItemRequestBuilder = this.graphServiceClient.Me.Drive.Special.AppRoot as DriveItemRequestBuilder;

            Assert.IsNotNull(driveItemRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(driveItemRequestBuilder.RequestUrl), "Unexpected request URL.");
        }
    }
}
