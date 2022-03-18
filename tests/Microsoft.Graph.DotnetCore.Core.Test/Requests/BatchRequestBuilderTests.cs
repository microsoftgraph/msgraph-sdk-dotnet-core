// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using Moq;
    using Xunit;
    using Microsoft.Graph.Core.Requests;
    using Microsoft.Kiota.Abstractions;
    using System.Threading.Tasks;
    using System.Net.Http;
    using System.Collections.Generic;

    public class BatchRequestBuilderTests
    {
        [Fact]
        public async Task BatchRequestBuilderAsync()
        {
            // Arrange
            IBaseClient baseClient = new BaseClient("https://localhost", GraphClientFactory.Create());

            // Act
            var batchRequestBuilder = new BatchRequestBuilder(baseClient.RequestAdapter);

            // 4. Create batch request content to be sent out
            // 4.1 Create HttpRequestMessages for the content
            HttpRequestMessage httpRequestMessage1 = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/");
            HttpRequestMessage httpRequestMessage2 = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, "https://graph.microsoft.com/v1.0/me/onenote/notebooks");

            // 4.2 Create batch request steps with request ids.
            BatchRequestStep requestStep1 = new BatchRequestStep("1", httpRequestMessage1);
            BatchRequestStep requestStep2 = new BatchRequestStep("2", httpRequestMessage2, new List<string> { "1" });

            // 4.3 Add batch request steps to BatchRequestContent.
            BatchRequestContent batchRequestContent = new BatchRequestContent(baseClient, requestStep1, requestStep2);
            var requestInformation = await batchRequestBuilder.CreatePostRequestInformationAsync(batchRequestContent);

            // Assert
            Assert.Equal("{+baseurl}/$batch", requestInformation.UrlTemplate);
            Assert.Equal(baseClient.RequestAdapter, batchRequestBuilder.RequestAdapter);
        }
    }
}