// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels;
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Serialization;
    using Microsoft.Kiota.Serialization.Json;
    using Moq;
    using Xunit;

    public class AsyncMonitorTests : IDisposable
    {
        private const string itemUrl = "https://localhost/item";
        private const string monitorUrl = "https://localhost/monitor";

        private AsyncMonitor<DerivedTypeClass> asyncMonitor;
        private HttpResponseMessage httpResponseMessage;
        private MockProgress progress;
        private BaseClient client;
        private Mock<IRequestAdapter> requestAdapter;

        public AsyncMonitorTests()
        {

            this.httpResponseMessage = new HttpResponseMessage();

            this.progress = new MockProgress();
            this.requestAdapter = new Mock<IRequestAdapter>(MockBehavior.Strict);
            this.client = new BaseClient(this.requestAdapter.Object);
            this.asyncMonitor = new AsyncMonitor<DerivedTypeClass>(this.client, AsyncMonitorTests.monitorUrl);
            ParseNodeFactoryRegistry.DefaultInstance.ContentTypeAssociatedFactories.TryAdd(CoreConstants.MimeTypeNames.Application.Json, new JsonParseNodeFactory());
        }

        public void Dispose()
        {
            this.httpResponseMessage.Dispose();
        }

        [Fact]
        public async Task PollForOperationCompletionAsync_IsCancelled()
        {
            var item = await this.asyncMonitor.PollForOperationCompletionAsync(this.progress.Object, new CancellationToken(true));
            Assert.Null(item);
        }

        [Fact]
        public async Task PollForOperationCompletionAsync_OperationCompleted()
        {
            bool called = false;
            this.progress.Setup(
                mockProgress => mockProgress.Report(
                    It.IsAny<AsyncOperationStatus>()))
                .Callback<AsyncOperationStatus>(status => this.ProgressCallback(status, out called));

            using var redirectedResponseMessage = new HttpResponseMessage();
            using var stringContent = new StringContent(JsonSerializer.Serialize(new AsyncOperationStatus(), new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            using var redirectedStringContent = new StringContent(JsonSerializer.Serialize(new DerivedTypeClass { Id = "id" }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            this.httpResponseMessage.Content = stringContent;
            this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;
            redirectedResponseMessage.Content = redirectedStringContent;

            this.requestAdapter
                .Setup(requestAdapter => requestAdapter.SendNoContentAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
                .Callback((RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>> errorMapping, CancellationToken cancellationToken) =>
                {
                    var responseHandler = requestInfo.GetRequestOption<ResponseHandlerOption>().ResponseHandler;
                    if (!called)
                    {
                        called = true;
                        ((NativeResponseHandler)responseHandler).Value = this.httpResponseMessage;
                    }
                    else
                        ((NativeResponseHandler)responseHandler).Value = redirectedResponseMessage;
                })
                .Returns(Task.FromResult(0));


            var item = await this.asyncMonitor.PollForOperationCompletionAsync(this.progress.Object, CancellationToken.None);

            Assert.True(called);
            Assert.NotNull(item);
            Assert.Equal("id", item.Id);
        }

        [Fact]
        public async Task PollForOperationCompletionAsync_OperationCancelled()
        {
            this.requestAdapter
                .Setup(requestAdapter => requestAdapter.SendNoContentAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
                .Callback((RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>> errorMapping, CancellationToken cancellationToken) => ((NativeResponseHandler)requestInfo.GetRequestOption<ResponseHandlerOption>().ResponseHandler).Value = this.httpResponseMessage)
                .Returns(Task.FromResult(0));

            using var stringContent = new StringContent(JsonSerializer.Serialize(new AsyncOperationStatus { Status = "cancelled" }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            this.httpResponseMessage.Content = stringContent;
            this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;

            var item = await this.asyncMonitor.PollForOperationCompletionAsync(this.progress.Object, CancellationToken.None);
            Assert.Null(item);
        }

        [Fact]
        public async Task PollForOperationCompletionAsync_OperationDeleteFailed()
        {
            this.requestAdapter
                .Setup(requestAdapter => requestAdapter.SendNoContentAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
                .Callback((RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>> errorMapping, CancellationToken cancellationToken) => ((NativeResponseHandler)requestInfo.GetRequestOption<ResponseHandlerOption>().ResponseHandler).Value = this.httpResponseMessage)
                .Returns(Task.FromResult(0));

            using var stringContent = new StringContent(JsonSerializer.Serialize(new AsyncOperationStatus { Status = "deleteFailed" }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
            this.httpResponseMessage.Content = stringContent;
            this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;

            ServiceException exception = await Assert.ThrowsAsync<ServiceException>(() => this.asyncMonitor.PollForOperationCompletionAsync(this.progress.Object, CancellationToken.None));
            Assert.Equal("delete operation failed", exception.Message);
        }

        [Fact]
        public async Task PollForOperationCompletionAsync_OperationFailed()
        {
            this.requestAdapter
                .Setup(requestAdapter => requestAdapter.SendNoContentAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
                .Callback((RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>> errorMapping, CancellationToken cancellationToken) => ((NativeResponseHandler)requestInfo.GetRequestOption<ResponseHandlerOption>().ResponseHandler).Value = this.httpResponseMessage)
                .Returns(Task.FromResult(0));

            using var stringContent = new StringContent("{\"message\": \"message\",\"status\": \"failed\"}");
            this.httpResponseMessage.Content = stringContent;
            this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;

            ServiceException exception = await Assert.ThrowsAsync<ServiceException>(() => this.asyncMonitor.PollForOperationCompletionAsync(this.progress.Object, CancellationToken.None));
            Assert.Equal("message", exception.Message);
        }

        [Fact]
        public async Task PollForOperationCompletionAsync_OperationNull()
        {
            this.requestAdapter
                .Setup(requestAdapter => requestAdapter.SendNoContentAsync(It.IsAny<RequestInformation>(), It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(), It.IsAny<CancellationToken>()))
                .Callback((RequestInformation requestInfo, Dictionary<string, ParsableFactory<IParsable>> errorMapping, CancellationToken cancellationToken) => ((NativeResponseHandler)requestInfo.GetRequestOption<ResponseHandlerOption>().ResponseHandler).Value = this.httpResponseMessage)
                .Returns(Task.FromResult(0));

            using var stringContent = new StringContent("");
            this.httpResponseMessage.Content = stringContent;
            this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;

            ServiceException exception = await Assert.ThrowsAsync<ServiceException>(() => this.asyncMonitor.PollForOperationCompletionAsync(this.progress.Object, CancellationToken.None));
            Assert.Equal("Error retrieving monitor status.", exception.Message);
        }

        private void ProgressCallback(AsyncOperationStatus asyncOperationStatus, out bool called)
        {
            this.httpResponseMessage.StatusCode = HttpStatusCode.OK;
            this.asyncMonitor.monitorUrl = AsyncMonitorTests.itemUrl;

            called = true;
        }
    }
}
