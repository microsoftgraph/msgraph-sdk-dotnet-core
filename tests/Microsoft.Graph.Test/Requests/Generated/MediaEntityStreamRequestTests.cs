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
    public class MediaEntityStreamRequestTests : RequestTestBase
    {
        [TestMethod]
        public void RequestBuilder()
        {
            var expectedRequestUri = new Uri(string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/photo/$value");
            var profilePhotoContentRequestBuilder = this.graphServiceClient.Me.Photo.Content as ProfilePhotoContentRequestBuilder;

            Assert.IsNotNull(profilePhotoContentRequestBuilder, "Unexpected request builder.");
            Assert.AreEqual(expectedRequestUri, new Uri(profilePhotoContentRequestBuilder.RequestUrl), "Unexpected request URL.");
        }

        [TestMethod]
        public async System.Threading.Tasks.Task GetAsync()
        {
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                var requestUrl = string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/photo/$value";
                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().StartsWith(requestUrl)
                                && request.Method == HttpMethod.Get),
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None))
                    .Returns(System.Threading.Tasks.Task.FromResult(httpResponseMessage));

                using (var returnedResponseStream = await this.graphServiceClient.Me.Photo.Content.Request().GetAsync())
                {
                    Assert.AreEqual(await httpResponseMessage.Content.ReadAsStreamAsync(), returnedResponseStream, "Unexpected stream returned.");
                }
            }
        }

        [TestMethod]
        public async System.Threading.Tasks.Task PutAsync()
        {
            using (var requestStream = new MemoryStream())
            using (var httpResponseMessage = new HttpResponseMessage())
            using (var responseStream = new MemoryStream())
            using (var streamContent = new StreamContent(responseStream))
            {
                httpResponseMessage.Content = streamContent;

                var requestUrl = string.Format(Constants.Url.GraphBaseUrlFormatString, "v1.0") + "/me/photo/$value";
                this.httpProvider.Setup(
                    provider => provider.SendAsync(
                        It.Is<HttpRequestMessage>(
                            request => request.RequestUri.ToString().StartsWith(requestUrl)
                                && request.Method == HttpMethod.Put),
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.None))
                    .Returns(System.Threading.Tasks.Task.FromResult(httpResponseMessage));

                using (var returnedResponseStream = await this.graphServiceClient.Me.Photo.Content.Request().PutAsync(requestStream))
                {
                    Assert.AreEqual(await httpResponseMessage.Content.ReadAsStreamAsync(), returnedResponseStream, "Unexpected stream returned.");
                }
            }
        }
    }
}
