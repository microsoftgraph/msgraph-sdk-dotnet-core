// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Graph.Core.Requests;
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Authentication;
    using Microsoft.Kiota.Abstractions.Serialization;
    using Moq;
    using Xunit;

    public class BatchRequestBuilderTests
    {
        [Fact]
        public async Task BatchRequestBuilderAsync()
        {
            // Arrange
            IBaseClient baseClient = new BaseClient("https://localhost", new AnonymousAuthenticationProvider());

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
#pragma warning disable CS0618 // Type or member is obsolete use the BatchRequestContentCollection for making batch requests
            BatchRequestContent batchRequestContent = new BatchRequestContent(baseClient, requestStep1, requestStep2);
#pragma warning restore CS0618 // Type or member is obsolete use the BatchRequestContentCollection for making batch requests
            var requestInformation = await batchRequestBuilder.ToPostRequestInformationAsync(batchRequestContent);

            // Assert
            Assert.Equal("{+baseurl}/$batch", requestInformation.UrlTemplate);
            Assert.Equal(baseClient.RequestAdapter, batchRequestBuilder.RequestAdapter);
        }


        [Fact]
        public async Task BatchRequestBuilderPostAsyncHandlesDoesNotThrowExceptionAsync()
        {
            // Arrange
            var requestAdapter = new Mock<IRequestAdapter>();
            IBaseClient baseClient = new BaseClient(requestAdapter.Object);

            var errorResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json"),//dummy content
            };
            requestAdapter
                .Setup(requestAdapter => requestAdapter.SendNoContentAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
                .Callback((RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>> errorMapping, CancellationToken cancellationToken) => ((NativeResponseHandler)requestInfo.GetRequestOption<ResponseHandlerOption>().ResponseHandler).Value = errorResponseMessage)
                .Returns(Task.FromResult(0));

            // Act
            var batchRequestBuilder = new BatchRequestBuilder(baseClient.RequestAdapter);

            // 4. Create batch request content to be sent out
            // 4.1 Create HttpRequestMessages for the content
            HttpRequestMessage httpRequestMessage1 = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/");

            // 4.2 Create batch request steps with request ids.
            BatchRequestStep requestStep1 = new BatchRequestStep("1", httpRequestMessage1);

            // 4.3 Add batch request steps to BatchRequestContent.
#pragma warning disable CS0618 // Type or member is obsolete use the BatchRequestContentCollection for making batch requests
            BatchRequestContent batchRequestContent = new BatchRequestContent(baseClient, requestStep1);
#pragma warning restore CS0618 // Type or member is obsolete use the BatchRequestContentCollection for making batch requests
            var responseContent = await batchRequestBuilder.PostAsync(batchRequestContent);

            // Assert
            Assert.NotNull(responseContent);
        }

        [Fact]
        public async Task BatchRequestBuilderPostAsyncHandlesNonSuccessStatusWithJsonResponseAsync()
        {
            // Arrange
            var requestAdapter = new Mock<IRequestAdapter>();
            IBaseClient baseClient = new BaseClient(requestAdapter.Object);

            var errorResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("{\"error\": {\"code\": \"20117\",\"message\": \"An item with this name already exists in this location.\",\"innerError\":{\"request-id\": \"nothing1b13-45cd-new-92be873c5781\",\"date\": \"2019-03-22T23:17:50\"}}}", Encoding.UTF8, "application/json"),
            };
            requestAdapter
                .Setup(requestAdapter => requestAdapter.SendNoContentAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
                .Callback((RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>> errorMapping, CancellationToken cancellationToken) => ((NativeResponseHandler)requestInfo.GetRequestOption<ResponseHandlerOption>().ResponseHandler).Value = errorResponseMessage)
                .Returns(Task.FromResult(0));

            // Act
            var batchRequestBuilder = new BatchRequestBuilder(baseClient.RequestAdapter);

            // 4. Create batch request content to be sent out
            // 4.1 Create HttpRequestMessages for the content
            HttpRequestMessage httpRequestMessage1 = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/");

            // 4.2 Create batch request steps with request ids.
            BatchRequestStep requestStep1 = new BatchRequestStep("1", httpRequestMessage1);

            // 4.3 Add batch request steps to BatchRequestContent.
#pragma warning disable CS0618 // Type or member is obsolete use the BatchRequestContentCollection for making batch requests
            BatchRequestContent batchRequestContent = new BatchRequestContent(baseClient, requestStep1);
#pragma warning restore CS0618 // Type or member is obsolete use the BatchRequestContentCollection for making batch requests
            var serviceException = await Assert.ThrowsAsync<ServiceException>(async () => await batchRequestBuilder.PostAsync(batchRequestContent));

            // Assert
            Assert.Equal(ErrorConstants.Messages.BatchRequestError, serviceException.Message);
            Assert.Equal(401, serviceException.ResponseStatusCode);
            Assert.NotNull(serviceException.InnerException);
            Assert.Equal("20117 : An item with this name already exists in this location.", serviceException.InnerException.Message);
        }

        [Fact]
        public async Task BatchRequestBuilderPostAsyncHandlesNonSuccessStatusWithNonJsonResponseAsync()
        {
            // Arrange
            var requestAdapter = new Mock<IRequestAdapter>();
            IBaseClient baseClient = new BaseClient(requestAdapter.Object);

            var errorResponseMessage = new HttpResponseMessage(System.Net.HttpStatusCode.Conflict)
            {
                Content = new StringContent("<html>This is random html</html>", Encoding.UTF8, "text/plain"),
            };
            requestAdapter
                .Setup(requestAdapter => requestAdapter.SendNoContentAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
                .Callback((RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>> errorMapping, CancellationToken cancellationToken) => ((NativeResponseHandler)requestInfo.GetRequestOption<ResponseHandlerOption>().ResponseHandler).Value = errorResponseMessage)
                .Returns(Task.FromResult(0));

            // Act
            var batchRequestBuilder = new BatchRequestBuilder(baseClient.RequestAdapter);

            // 4. Create batch request content to be sent out
            // 4.1 Create HttpRequestMessages for the content
            HttpRequestMessage httpRequestMessage1 = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/");

            // 4.2 Create batch request steps with request ids.
            BatchRequestStep requestStep1 = new BatchRequestStep("1", httpRequestMessage1);

            // 4.3 Add batch request steps to BatchRequestContent.
#pragma warning disable CS0618 // Type or member is obsolete use the BatchRequestContentCollection for making batch requests
            BatchRequestContent batchRequestContent = new BatchRequestContent(baseClient, requestStep1);
#pragma warning restore CS0618 // Type or member is obsolete use the BatchRequestContentCollection for making batch requests
            var serviceException = await Assert.ThrowsAsync<ServiceException>(async () => await batchRequestBuilder.PostAsync(batchRequestContent));

            // Assert
            Assert.Equal(ErrorConstants.Messages.BatchRequestError, serviceException.Message);
            Assert.Equal(409, serviceException.ResponseStatusCode);
            Assert.Equal("<html>This is random html</html>", serviceException.RawResponseBody);
        }
    }
}
