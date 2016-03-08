// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Test.Requests.Extensions
{
    using System;

    using Microsoft.Graph;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MailFolderMessagesCollectionRequestBuilderExtensionsTests : RequestTestBase
    {
        [TestMethod]
        public void DeletedItems()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/mailFolders/DeletedItems");
            var mailFolderRequestBuilder = this.graphServiceClient.Me.MailFolders.DeletedItems as MailFolderRequestBuilder;

            Assert.IsNotNull(mailFolderRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(mailFolderRequestBuilder.RequestUrl), "Unexpected request URL.");
        }

        [TestMethod]
        public void Drafts()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/mailFolders/Drafts");
            var mailFolderRequestBuilder = this.graphServiceClient.Me.MailFolders.Drafts as MailFolderRequestBuilder;

            Assert.IsNotNull(mailFolderRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(mailFolderRequestBuilder.RequestUrl), "Unexpected request URL.");
        }

        [TestMethod]
        public void Inbox()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/mailFolders/Inbox");
            var mailFolderRequestBuilder = this.graphServiceClient.Me.MailFolders.Inbox as MailFolderRequestBuilder;

            Assert.IsNotNull(mailFolderRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(mailFolderRequestBuilder.RequestUrl), "Unexpected request URL.");
        }

        [TestMethod]
        public void SentItems()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/mailFolders/SentItems");
            var mailFolderRequestBuilder = this.graphServiceClient.Me.MailFolders.SentItems as MailFolderRequestBuilder;

            Assert.IsNotNull(mailFolderRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(mailFolderRequestBuilder.RequestUrl), "Unexpected request URL.");
        }
    }
}
