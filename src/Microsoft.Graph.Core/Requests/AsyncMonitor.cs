// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Serialization;

    /// <summary>
    /// Monitor for async operations to the Graph service on the client.
    /// </summary>
    /// <typeparam name="T">The object type to return.</typeparam>
    public class AsyncMonitor<T> : IAsyncMonitor<T> where T : IParsable, new()
    {
        private AsyncOperationStatus asyncOperationStatus;
        private IBaseClient client;

        internal string monitorUrl;

        private readonly IAsyncParseNodeFactory parseNodeFactory;
        /// <summary>
        /// Construct an Async Monitor.
        /// </summary>
        /// <param name="client">The client to monitor.</param>
        /// <param name="monitorUrl">The URL to monitor.</param>
        /// <param name="parseNodeFactory"> The <see cref="IParseNodeFactory"/> to use for response handling</param>
        public AsyncMonitor(IBaseClient client, string monitorUrl, IParseNodeFactory parseNodeFactory = null)
        {
            this.client = client;
            this.monitorUrl = monitorUrl;
            this.parseNodeFactory = parseNodeFactory as IAsyncParseNodeFactory ?? ParseNodeFactoryRegistry.DefaultInstance;
        }

        /// <summary>
        /// Poll to check for completion of an async call to the Graph service.
        /// </summary>
        /// <param name="progress">The progress status.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The operation task.</returns>
        public async Task<T> PollForOperationCompletionAsync(IProgress<AsyncOperationStatus> progress, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var requestInformation = new RequestInformation() { HttpMethod = Method.GET, UrlTemplate = this.monitorUrl };
                var nativeResponseHandler = new NativeResponseHandler();
                requestInformation.SetResponseHandler(nativeResponseHandler);
                await this.client.RequestAdapter.SendNoContentAsync(requestInformation, cancellationToken: cancellationToken).ConfigureAwait(false);
                using var responseMessage = nativeResponseHandler.Value as HttpResponseMessage;

                // The monitor service will return an Accepted status for any monitor operation that hasn't completed.
                // If we have a success code that isn't Accepted, the operation is complete. Return the resulting object.
                if (responseMessage.StatusCode != HttpStatusCode.Accepted && responseMessage.IsSuccessStatusCode)
                {
                    using var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    return responseStream.Length > 0 ? (await parseNodeFactory.GetRootParseNodeAsync(CoreConstants.MimeTypeNames.Application.Json, responseStream, cancellationToken)).GetObjectValue(_ => new T()) : default;
                }

                using (var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    this.asyncOperationStatus = responseStream.Length > 0 ? (await parseNodeFactory.GetRootParseNodeAsync(CoreConstants.MimeTypeNames.Application.Json, responseStream, cancellationToken).ConfigureAwait(false)).GetObjectValue(_ => new AsyncOperationStatus()) : null;

                    if (this.asyncOperationStatus == null)
                    {
                        throw new ServiceException("Error retrieving monitor status.");
                    }

                    if (string.Equals(this.asyncOperationStatus.Status, "cancelled", StringComparison.OrdinalIgnoreCase))
                    {
                        return default(T);
                    }

                    if (string.Equals(this.asyncOperationStatus.Status, "failed", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(this.asyncOperationStatus.Status, "deleteFailed", StringComparison.OrdinalIgnoreCase))
                    {
                        object message = null;
                        this.asyncOperationStatus.AdditionalData?.TryGetValue("message", out message);

                        throw new ServiceException(message?.ToString() ?? "delete operation failed");
                    }

                    if (progress != null)
                    {
                        progress.Report(this.asyncOperationStatus);
                    }
                }

                await Task.Delay(CoreConstants.PollingIntervalInMs, cancellationToken).ConfigureAwait(false);
            }

            return default(T);
        }
    }
}
