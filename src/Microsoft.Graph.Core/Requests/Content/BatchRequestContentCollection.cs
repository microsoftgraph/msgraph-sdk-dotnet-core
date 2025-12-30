// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Kiota.Abstractions;

    /// <summary>
    /// A collection of batch requests that are automatically managed.
    /// </summary>
    public class BatchRequestContentCollection
    {
        private readonly IRequestAdapter requestAdapter;
        private readonly HashSet<BatchRequestContent> batchRequests;
        private readonly int batchRequestLimit;
        private BatchRequestContent currentRequest;
        private bool readOnly = false;

        /// <summary>
        /// Constructs a new <see cref="BatchRequestContentCollection"/>.
        /// </summary>
        /// <param name="baseClient">The <see cref="IBaseClient"/> for making requests</param>
        /// <param name="batchRequestLimit">Number of requests that may be placed in a single batch</param>
        public BatchRequestContentCollection(IBaseClient baseClient, int batchRequestLimit = CoreConstants.BatchRequest.MaxNumberOfRequests) : this(baseClient?.RequestAdapter, batchRequestLimit)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="BatchRequestContentCollection"/>.
        /// </summary>
        /// <param name="requestAdapter">The <see cref="IRequestAdapter"/> for making requests</param>
        /// <param name="batchRequestLimit">Number of requests that may be placed in a single batch</param>
        public BatchRequestContentCollection(IRequestAdapter requestAdapter, int batchRequestLimit = CoreConstants.BatchRequest.MaxNumberOfRequests)
        {
            if (batchRequestLimit < 2 || batchRequestLimit > CoreConstants.BatchRequest.MaxNumberOfRequests)
            {
                throw new ArgumentOutOfRangeException(nameof(batchRequestLimit));
            }
            this.requestAdapter = requestAdapter ?? throw new ArgumentNullException(nameof(requestAdapter));
            this.batchRequestLimit = batchRequestLimit;
            batchRequests = new HashSet<BatchRequestContent>();
#pragma warning disable CS0618
            currentRequest = new BatchRequestContent(this.requestAdapter);
#pragma warning restore CS0618
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
#pragma warning disable CS0618
                currentRequest = new BatchRequestContent(requestAdapter);
#pragma warning restore CS0618
            }
        }

        /// <summary>
        /// Adds a <see cref="BatchRequestStep"/> to batch request content.
        /// </summary>
        /// <param name="batchRequestStep">A <see cref="BatchRequestStep"/> to add.</param>
        /// <returns>True or false based on addition or not addition of the provided <see cref="BatchRequestStep"/>. </returns>
        public bool AddBatchRequestStep(BatchRequestStep batchRequestStep)
        {
            SetupCurrentRequest();
#pragma warning disable CS0618
            return currentRequest.AddBatchRequestStep(batchRequestStep);
#pragma warning restore CS0618
        }

        /// <summary>
        /// Adds a <see cref="HttpRequestMessage"/> to batch request content.
        /// </summary>
        /// <param name="httpRequestMessage">A <see cref="HttpRequestMessage"/> to use to build a <see cref="BatchRequestStep"/> to add.</param>
        /// <returns>The requestId of the newly created <see cref="BatchRequestStep"/></returns>
        public string AddBatchRequestStep(HttpRequestMessage httpRequestMessage)
        {
            SetupCurrentRequest();
#pragma warning disable CS0618
            return currentRequest.AddBatchRequestStep(httpRequestMessage);
#pragma warning restore CS0618
        }

        /// <summary>
        /// Adds a <see cref="RequestInformation"/> to batch request content
        /// </summary>
        /// <param name="requestInformation">A <see cref="RequestInformation"/> to use to build a <see cref="BatchRequestStep"/> to add.</param>
        /// <param name="requestId">An optional string that will be used as the requestId of the batch request</param>
        /// <returns>The requestId of the  newly created <see cref="BatchRequestStep"/></returns>
        public Task<string> AddBatchRequestStepAsync(RequestInformation requestInformation, string requestId = null)
        {
            SetupCurrentRequest();
#pragma warning disable CS0618
            return currentRequest.AddBatchRequestStepAsync(requestInformation, requestId);
#pragma warning restore CS0618
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
                foreach (var batchRequest in batchRequests)
                {
                    removed = batchRequest.RemoveBatchRequestStepWithId(requestId);
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
        public IReadOnlyDictionary<string, BatchRequestStep> BatchRequestSteps
        {
            get
            {
                if (batchRequests.Count > 0)
                {
                    IEnumerable<KeyValuePair<string, BatchRequestStep>> result = batchRequests.Contains(currentRequest) ?
                        new List<KeyValuePair<string, BatchRequestStep>>()
                        : currentRequest.BatchRequestSteps;

                    foreach (var request in batchRequests)
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
            var request = new BatchRequestContentCollection(this.requestAdapter, batchRequestLimit);
            var steps = this.BatchRequestSteps;
            foreach (var response in responseStatusCodes)
            {
                if (steps.ContainsKey(response.Key) && !BatchResponseContent.IsSuccessStatusCode(response.Value))
                {
                    var step = steps[response.Key];
                    var newStep = new BatchRequestStep(
                        requestId: step.RequestId,
                        httpRequestMessage: step.Request,
                        dependsOn: step.DependsOn?.ToList());

                    request.AddBatchRequestStep(newStep);
                }
            }
            return request;
        }

        /// <summary>
        /// Creates a new <see cref="BatchRequestContentCollection"/> with all <see cref="BatchRequestStep"/> that failed.
        /// </summary>
        /// <param name="responseStatusCodes">A dictionary with response codes, get by executing batchResponseContentCollection.GetResponsesStatusCodesAsync()</param>
        /// <param name="statusCodesToTreatAsSuccess">Optional additional HTTP status codes to also treat as successful. Success is determined by <see cref="BatchResponseContent.IsSuccessStatusCode(HttpStatusCode)"/> OR membership in this set</param>
        /// <returns>new <see cref="BatchRequestContentCollection"/> with all failed requests.</returns>
        public BatchRequestContentCollection NewBatchWithFailedRequests(Dictionary<string, HttpStatusCode> responseStatusCodes, IEnumerable<HttpStatusCode> statusCodesToTreatAsSuccess)
        {
            var request = new BatchRequestContentCollection(this.requestAdapter, batchRequestLimit);
            if (responseStatusCodes == null || responseStatusCodes.Count == 0)
            {
                return request;
            }

            HashSet<HttpStatusCode> successSet = statusCodesToTreatAsSuccess != null
                ? [.. statusCodesToTreatAsSuccess]
                : null;

            bool IsSuccess(HttpStatusCode code)
            {
                if (BatchResponseContent.IsSuccessStatusCode(code))
                {
                    return true;
                }

                if (successSet != null && successSet.Contains(code))
                {
                    return true;
                }

                return false;
            }

            var steps = this.BatchRequestSteps;
            foreach (var kvp in responseStatusCodes)
            {
                if (steps.TryGetValue(kvp.Key, out var step))
                {
                    if (!IsSuccess(kvp.Value))
                    {
                        var newStep = new BatchRequestStep(
                            requestId: step.RequestId,
                            httpRequestMessage: step.Request,
                            dependsOn: step.DependsOn?.ToList());

                        request.AddBatchRequestStep(newStep);
                    }
                }
            }

            return request;
        }
    }
}
