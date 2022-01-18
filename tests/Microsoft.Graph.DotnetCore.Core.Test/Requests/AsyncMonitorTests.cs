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
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Graph;
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using Moq;
    using Microsoft.Graph.DotnetCore.Core.Test.TestModels;
    using Xunit;
    using Microsoft.Kiota.Abstractions;
    using System.Text.Json;

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
            using var stringContent = new StringContent(JsonSerializer.Serialize(new AsyncOperationStatus()));
            using var redirectedStringContent = new StringContent(JsonSerializer.Serialize(new DerivedTypeClass { Id = "id" }));
            this.httpResponseMessage.Content = stringContent;
            this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;
            redirectedResponseMessage.Content = redirectedStringContent;

            this.requestAdapter
                .Setup(requestAdapter => requestAdapter.SendNoContentAsync(It.IsAny<RequestInformation>(), It.IsAny<NativeResponseHandler>(), It.IsAny<CancellationToken>()))
                .Callback((RequestInformation requestInfo, IResponseHandler responseHandler, CancellationToken cancellationToken) =>
                {
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
                .Setup(requestAdapter => requestAdapter.SendNoContentAsync(It.IsAny<RequestInformation>(), It.IsAny<NativeResponseHandler>(), It.IsAny<CancellationToken>()))
                .Callback((RequestInformation requestInfo, IResponseHandler responseHandler, CancellationToken cancellationToken) => ((NativeResponseHandler)responseHandler).Value = this.httpResponseMessage)
                .Returns(Task.FromResult(0));

            using var stringContent = new StringContent(JsonSerializer.Serialize(new AsyncOperationStatus { Status = "cancelled" }));
            this.httpResponseMessage.Content = stringContent;
            this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;

            var item = await this.asyncMonitor.PollForOperationCompletionAsync(this.progress.Object, CancellationToken.None);
            Assert.Null(item);
        }

        [Fact]
        public async Task PollForOperationCompletionAsync_OperationDeleteFailed()
        {
            this.requestAdapter
                .Setup(requestAdapter => requestAdapter.SendNoContentAsync(It.IsAny<RequestInformation>(), It.IsAny<NativeResponseHandler>(), It.IsAny<CancellationToken>()))
                .Callback((RequestInformation requestInfo, IResponseHandler responseHandler, CancellationToken cancellationToken) => ((NativeResponseHandler)responseHandler).Value = this.httpResponseMessage)
                .Returns(Task.FromResult(0));

            using var stringContent = new StringContent(JsonSerializer.Serialize(new AsyncOperationStatus { Status = "deleteFailed" }));
            this.httpResponseMessage.Content = stringContent;
            this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;

            ServiceException exception = await Assert.ThrowsAsync<ServiceException>(() => this.asyncMonitor.PollForOperationCompletionAsync(this.progress.Object, CancellationToken.None));
            Assert.Equal(ErrorConstants.Codes.GeneralException, exception.Error.Code);
        }

        [Fact]
        public async Task PollForOperationCompletionAsync_OperationFailed()
        {
            this.requestAdapter
                .Setup(requestAdapter => requestAdapter.SendNoContentAsync(It.IsAny<RequestInformation>(), It.IsAny<NativeResponseHandler>(), It.IsAny<CancellationToken>()))
                .Callback((RequestInformation requestInfo, IResponseHandler responseHandler, CancellationToken cancellationToken) => ((NativeResponseHandler)responseHandler).Value = this.httpResponseMessage)
                .Returns(Task.FromResult(0));

            using var stringContent = new StringContent(JsonSerializer.Serialize(new AsyncOperationStatus
            {
                AdditionalData = new Dictionary<string, object> { { "message", "message" } },
                Status = "failed"
            }));
            this.httpResponseMessage.Content = stringContent;
            this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;

            ServiceException exception = await Assert.ThrowsAsync<ServiceException>(() => this.asyncMonitor.PollForOperationCompletionAsync(this.progress.Object, CancellationToken.None));
            Assert.Equal(ErrorConstants.Codes.GeneralException, exception.Error.Code);
            Assert.Equal("message", exception.Error.Message);
        }

        [Fact]
        public async Task PollForOperationCompletionAsync_OperationNull()
        {
            this.requestAdapter
                .Setup(requestAdapter => requestAdapter.SendNoContentAsync(It.IsAny<RequestInformation>(), It.IsAny<NativeResponseHandler>(), It.IsAny<CancellationToken>()))
                .Callback((RequestInformation requestInfo, IResponseHandler responseHandler, CancellationToken cancellationToken) => ((NativeResponseHandler)responseHandler).Value = this.httpResponseMessage)
                .Returns(Task.FromResult(0));

            using var stringContent = new StringContent("");
            this.httpResponseMessage.Content = stringContent;
            this.httpResponseMessage.StatusCode = HttpStatusCode.Accepted;

            ServiceException exception = await Assert.ThrowsAsync<ServiceException>(() => this.asyncMonitor.PollForOperationCompletionAsync(this.progress.Object, CancellationToken.None));
            Assert.Equal(ErrorConstants.Codes.GeneralException, exception.Error.Code);
        }

        private void ProgressCallback(AsyncOperationStatus asyncOperationStatus, out bool called)
        {
            this.httpResponseMessage.StatusCode = HttpStatusCode.OK;
            this.asyncMonitor.monitorUrl = AsyncMonitorTests.itemUrl;

            called = true;
        }
    }
}
