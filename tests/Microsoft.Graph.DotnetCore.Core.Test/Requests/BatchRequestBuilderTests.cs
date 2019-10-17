// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using Moq;
    using Xunit;
    using Microsoft.Graph.Core.Requests;

    public class BatchRequestBuilderTests
    {
        [Fact]
        public void BatchRequestBuilder()
        {
            // Arrange
            var requestUrl = "https://localhost";
            var client = new Mock<IBaseClient>().Object;

            // Act
            var batchRequestBuilder = new BatchRequestBuilder(requestUrl, client);

            // Assert
            Assert.Equal(requestUrl, batchRequestBuilder.RequestUrl);
            Assert.Equal(client, batchRequestBuilder.Client);
        }
    }
}