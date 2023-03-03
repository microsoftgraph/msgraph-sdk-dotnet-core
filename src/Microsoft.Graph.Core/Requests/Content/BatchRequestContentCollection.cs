namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Http.HttpClientLibrary;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    /// <summary>
    /// A collection of batch requests that are automatically managed.
    /// </summary>
    public class BatchRequestContentCollection
    {
        private readonly IBaseClient baseClient;
        private readonly List<BatchRequestContent> batchRequests;
        private BatchRequestContent currentRequest;
        private bool readOnly = false;

        /// <summary>
        /// Constructs a new <see cref="BatchRequestContentCollection"/>.
        /// </summary>
        /// <param name="baseClient">The <see cref="IBaseClient"/> for making requests</param>
        public BatchRequestContentCollection(IBaseClient baseClient)
        {
            this.baseClient = baseClient;
            batchRequests = new List<BatchRequestContent>();
            currentRequest = new BatchRequestContent(baseClient);
        }

        private void ValidateReadOnly()
        {
            if (readOnly)
            {
                throw new InvalidOperationException("Batch request collection is already executed");
            }
        }

        private void SetupCurrentRequest()
        {
            ValidateReadOnly();
            if (currentRequest.BatchRequestSteps.Count >= CoreConstants.BatchRequest.MaxNumberOfRequests)
            {
                batchRequests.Add(currentRequest);
                currentRequest = new BatchRequestContent(baseClient);
            }
        }

        /// <summary>
        /// Adds a <see cref="HttpRequestMessage"/> to batch request content.
        /// </summary>
        /// <param name="httpRequestMessage">A <see cref="HttpRequestMessage"/> to use to build a <see cref="BatchRequestStep"/> to add.</param>
        /// <returns>The requestId of the newly created <see cref="BatchRequestStep"/></returns>
        public string AddBatchRequestStep(HttpRequestMessage httpRequestMessage)
        {
            SetupCurrentRequest();
            return currentRequest.AddBatchRequestStep(httpRequestMessage);
        }

        /// <summary>
        /// Adds a <see cref="RequestInformation"/> to batch request content
        /// </summary>
        /// <param name="requestInformation">A <see cref="RequestInformation"/> to use to build a <see cref="BatchRequestStep"/> to add.</param>
        /// <returns>The requestId of the  newly created <see cref="BatchRequestStep"/></returns>
        public Task<string> AddBatchRequestStepAsync(RequestInformation requestInformation)
        {
            SetupCurrentRequest();
            return currentRequest.AddBatchRequestStepAsync(requestInformation);
        }

        /// <summary>
        /// Removes a <see cref="BatchRequestStep"/> from batch request content for the specified id.
        /// </summary>
        /// <param name="requestId">A unique batch request id to remove.</param>
        /// <returns>True or false based on removal or not removal of a <see cref="BatchRequestStep"/>.</returns>
        public bool RemoveBatchRequestStepWithId(string requestId)
        {
            ValidateReadOnly();
            var removed = currentRequest.RemoveBatchRequestStepWithId(requestId);
            if (!removed && batchRequests.Count > 0)
            {
                for (int i = 0; i < batchRequests.Count; i++)
                {
                    removed = batchRequests[i].RemoveBatchRequestStepWithId(requestId);
                    if (removed)
                    {
                        return true;
                    }
                }
            }
            return removed;
        }

        internal IEnumerable<BatchRequestContent> GetBatchRequestsForExecution()
        {
            readOnly = true;
            if (currentRequest.BatchRequestSteps.Count > 0)
            {
                batchRequests.Add(currentRequest);
            }

            return batchRequests;
        }
    }
}