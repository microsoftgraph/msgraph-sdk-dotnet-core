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
    public class EntityReferenceRequestTests : RequestTestBase
    {
        /// <summary>
        /// Tests building a request for an entity's $ref navigation.
        /// </summary>
        [TestMethod]
        public void BuildRequest()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/groups/groupId/members/memberId/$ref");
            var memberReferenceRequestBuilder = this.graphServiceClient.Groups["groupId"].Members["memberId"].Reference as DirectoryObjectReferenceRequestBuilder;

            Assert.IsNotNull(memberReferenceRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(memberReferenceRequestBuilder.RequestUrl), "Unexpected request URL.");

            var memberReferenceRequest = memberReferenceRequestBuilder.Request() as DirectoryObjectReferenceRequest;
            Assert.IsNotNull(memberReferenceRequest, "Unexpected request.");
            Assert.AreEqual(expectedRequestUri, new Uri(memberReferenceRequest.RequestUrl), "Unexpected request URL.");
        }

        /// <summary>
        /// Tests the DeleteAsync() method on an entity's $ref navigation.
        /// </summary>
        [TestMethod]
        public async System.Threading.Tasks.Task DeleteAsync()
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                var requestUrl = string.Format("{0}/groups/groupId/members/memberId/$ref", this.graphBaseUrl);

                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().Equals(requestUrl)
                                && request.Method == HttpMethod.Delete),
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None))
                    .Returns(System.Threading.Tasks.Task.FromResult(httpResponseMessage));

                await this.graphServiceClient.Groups["groupId"].Members["memberId"].Reference.Request().DeleteAsync();
            }
        }
    }
}
