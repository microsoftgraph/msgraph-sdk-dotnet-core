namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// A collection of batch requests that are automatically managed.
    /// </summary>
    public class BatchRequestContentCollection
    {
        private readonly IBaseClient baseClient;
        private readonly List<BatchRequestContent> batchRequests;
        private readonly int batchRequestLimit;
        private BatchRequestContent currentRequest;
        private bool readOnly = false;

        /// <summary>
        /// Constructs a new <see cref="BatchRequestContentCollection"/>.
        /// </summary>
        /// <param name="baseClient">The <see cref="IBaseClient"/> for making requests</param>
        public BatchRequestContentCollection(IBaseClient baseClient) : this (baseClient, CoreConstants.BatchRequest.MaxNumberOfRequests)
        {
            
        }

        /// <summary>
        /// Constructs a new <see cref="BatchRequestContentCollection"/>.
        /// </summary>
        /// <param name="baseClient">The <see cref="IBaseClient"/> for making requests</param>
        /// <param name="batchRequestLimit">Number of requests that may be placed in a single batch</param>
        public BatchRequestContentCollection(IBaseClient baseClient, int batchRequestLimit)
        {
            if(baseClient == null)
            {
                throw new ArgumentNullException(nameof(baseClient));
            }
            if (batchRequestLimit < 2 || batchRequestLimit > CoreConstants.BatchRequest.MaxNumberOfRequests)
            {
                throw new ArgumentOutOfRangeException(nameof(batchRequestLimit));
            }
            this.baseClient = baseClient;
            this.batchRequestLimit = batchRequestLimit;
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
            if (currentRequest.BatchRequestSteps.Count >= batchRequestLimit)
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

        /// <summary>
        /// A BatchRequestSteps property.
        /// </summary>
        public IReadOnlyDictionary<string, BatchRequestStep> BatchRequestSteps { get
            {
                if (batchRequests.Count > 0)
                {
                    IEnumerable<KeyValuePair<string, BatchRequestStep>> result = currentRequest.BatchRequestSteps;
                    foreach ( var request in batchRequests)
                    {
                        result = result.Concat(request.BatchRequestSteps);
                    }

                    return result.ToDictionary(x => x.Key, x => x.Value);
                }

                return currentRequest.BatchRequestSteps;
            }
        }

        /// <summary>
        /// Creates a new <see cref="BatchRequestContentCollection"/> with all <see cref="BatchRequestStep"/> that failed.
        /// </summary>
        /// <param name="responseStatusCodes">A dictionary with response codes, get by executing batchResponseContentCollection.GetResponsesStatusCodesAsync()</param>
        /// <returns>new <see cref="BatchRequestContentCollection"/> with all failed requests.</returns>
        public BatchRequestContentCollection NewBatchWithFailedRequests(Dictionary<string, HttpStatusCode> responseStatusCodes)
        {
            var request = new BatchRequestContentCollection(this.baseClient, batchRequestLimit);
            var steps = this.BatchRequestSteps;
            foreach(var response in responseStatusCodes)
            {
                if (steps.ContainsKey(response.Key) && !BatchResponseContent.IsSuccessStatusCode(response.Value)) {
                    request.AddBatchRequestStep(steps[response.Key].Request);
                }
            }
            return request;
        }
    }
}