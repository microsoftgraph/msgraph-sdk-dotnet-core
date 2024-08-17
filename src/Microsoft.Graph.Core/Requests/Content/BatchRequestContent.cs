// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Threading;

    /// <summary>
    /// A <see cref="HttpContent"/> implementation to handle json batch requests.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BatchRequestContent: HttpContent
    {
        /// <summary>
        /// A BatchRequestSteps property.
        /// </summary>
        public IReadOnlyDictionary<string, BatchRequestStep> BatchRequestSteps { get; private set; }

        /// <summary>
        /// The request adapter for sending the batch request
        /// </summary>
        public IRequestAdapter RequestAdapter { get; set; }

        /// <summary>
        /// Constructs a new <see cref="BatchRequestContent"/>.
        /// </summary>
        /// <param name="baseClient">The <see cref="IBaseClient"/> for making requests</param>
        [Obsolete("Please use the BatchRequestContentCollection for making batch requests as it supports handling more than 20 requests and provides a similar API experience.")]
        public BatchRequestContent(IBaseClient baseClient)
            :this(baseClient, new BatchRequestStep[] { })
        {
        }

        /// <summary>
        /// Constructs a new <see cref="BatchRequestContent"/>.
        /// </summary>
        /// <param name="baseClient">The <see cref="IBaseClient"/> for making requests</param>
        /// <param name="batchRequestSteps">A list of <see cref="BatchRequestStep"/> to add to the batch request content.</param>
        [Obsolete("Please use the BatchRequestContentCollection for making batch requests as it supports handling more than 20 requests and provides a similar API experience.")]
        public BatchRequestContent(IBaseClient baseClient, params BatchRequestStep[] batchRequestSteps): this(baseClient?.RequestAdapter ?? throw new ArgumentNullException(nameof(baseClient)), batchRequestSteps)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="BatchRequestContent"/>.
        /// </summary>
        /// <param name="requestAdapter">The <see cref="IRequestAdapter"/> for making requests</param>
        /// <param name="batchRequestSteps">A list of <see cref="BatchRequestStep"/> to add to the batch request content.</param>
        [Obsolete("Please use the BatchRequestContentCollection for making batch requests as it supports handling more than 20 requests and provides a similar API experience.")]
        public BatchRequestContent(IRequestAdapter requestAdapter, params BatchRequestStep[] batchRequestSteps)
        {
            if (batchRequestSteps == null)
                throw new ArgumentNullException(nameof(batchRequestSteps));

            if (batchRequestSteps.Count() > CoreConstants.BatchRequest.MaxNumberOfRequests)
                throw new ArgumentException(string.Format(ErrorConstants.Messages.MaximumValueExceeded, "Number of batch request steps", CoreConstants.BatchRequest.MaxNumberOfRequests));

            this.Headers.ContentType = new MediaTypeHeaderValue(CoreConstants.MimeTypeNames.Application.Json);

            BatchRequestSteps = new Dictionary<string, BatchRequestStep>();

            foreach (BatchRequestStep requestStep in batchRequestSteps)
            {
                if(requestStep.DependsOn != null && !ContainsCorrespondingRequestId(requestStep.DependsOn))
                {
                    throw new ArgumentException(ErrorConstants.Messages.InvalidDependsOnRequestId);
                }
                AddBatchRequestStep(requestStep);
            }

            this.RequestAdapter = requestAdapter ?? throw new ArgumentNullException(nameof(requestAdapter));
        }

        /// <summary>
        /// Adds a <see cref="BatchRequestStep"/> to batch request content if doesn't exists.
        /// </summary>
        /// <param name="batchRequestStep">A <see cref="BatchRequestStep"/> to add.</param>
        /// <returns>True or false based on addition or not addition of the provided <see cref="BatchRequestStep"/>. </returns>
        /// <exception cref="ArgumentException"> When the the request step contains a depends on to a request id that is not present.</exception>
        [Obsolete("Please use the BatchRequestContentCollection for making batch requests as it supports handling more than 20 requests and provides a similar API experience.")]
        public bool AddBatchRequestStep(BatchRequestStep batchRequestStep)
        {
            if (batchRequestStep == null
                || BatchRequestSteps.ContainsKey(batchRequestStep.RequestId)
                || BatchRequestSteps.Count >= CoreConstants.BatchRequest.MaxNumberOfRequests //we should not add any more steps
                )
            {
                return false;
            }
            // validate the depends on exists before adding it
            if(batchRequestStep.DependsOn != null && !ContainsCorrespondingRequestId(batchRequestStep.DependsOn))
            {
                throw new ArgumentException(ErrorConstants.Messages.InvalidDependsOnRequestId);
            }
            (BatchRequestSteps as IDictionary<string, BatchRequestStep>).Add(batchRequestStep.RequestId, batchRequestStep);
            return true;
        }

        /// <summary>
        /// Adds a <see cref="HttpRequestMessage"/> to batch request content.
        /// </summary>
        /// <param name="httpRequestMessage">A <see cref="HttpRequestMessage"/> to use to build a <see cref="BatchRequestStep"/> to add.</param>
        /// <returns>The requestId of the newly created <see cref="BatchRequestStep"/></returns>
        [Obsolete("Please use the BatchRequestContentCollection for making batch requests as it supports handling more than 20 requests and provides a similar API experience.")]
        public string AddBatchRequestStep(HttpRequestMessage httpRequestMessage)
        {
            if (BatchRequestSteps.Count >= CoreConstants.BatchRequest.MaxNumberOfRequests)
                throw new ArgumentException(string.Format(ErrorConstants.Messages.MaximumValueExceeded, "Number of batch request steps", CoreConstants.BatchRequest.MaxNumberOfRequests));

            string requestId = Guid.NewGuid().ToString();
            BatchRequestStep batchRequestStep = new BatchRequestStep(requestId, httpRequestMessage);
            (BatchRequestSteps as IDictionary<string, BatchRequestStep>).Add(batchRequestStep.RequestId, batchRequestStep);
            return requestId;
        }

        /// <summary>
        /// Adds a <see cref="RequestInformation"/> to batch request content
        /// </summary>
        /// <param name="requestInformation">A <see cref="RequestInformation"/> to use to build a <see cref="BatchRequestStep"/> to add.</param>
        /// <param name="requestId">An optional string that will be used as the requestId of the batch request</param>
        /// <returns>The requestId of the  newly created <see cref="BatchRequestStep"/></returns>
        [Obsolete("Please use the BatchRequestContentCollection for making batch requests as it supports handling more than 20 requests and provides a similar API experience.")]
        public async Task<string> AddBatchRequestStepAsync(RequestInformation requestInformation, string requestId = null)
        {
            if (BatchRequestSteps.Count >= CoreConstants.BatchRequest.MaxNumberOfRequests)
                throw new ArgumentException(string.Format(ErrorConstants.Messages.MaximumValueExceeded, "Number of batch request steps", CoreConstants.BatchRequest.MaxNumberOfRequests));
            if (requestId == null)
            {
                requestId = Guid.NewGuid().ToString();
            }
            var requestMessage = await RequestAdapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInformation);
            BatchRequestStep batchRequestStep = new BatchRequestStep(requestId, requestMessage);
            (BatchRequestSteps as IDictionary<string, BatchRequestStep>)!.Add(batchRequestStep.RequestId, batchRequestStep);
            return requestId;
        }

        /// <summary>
        /// Removes a <see cref="BatchRequestStep"/> from batch request content for the specified id.
        /// </summary>
        /// <param name="requestId">A unique batch request id to remove.</param>
        /// <returns>True or false based on removal or not removal of a <see cref="BatchRequestStep"/>.</returns>
        public bool RemoveBatchRequestStepWithId(string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
                throw new ArgumentNullException(nameof(requestId));

            bool isRemoved = false;
            if (BatchRequestSteps.ContainsKey(requestId)) {
                (BatchRequestSteps as IDictionary<string, BatchRequestStep>).Remove(requestId);
                isRemoved = true;
                foreach (KeyValuePair<string, BatchRequestStep> batchRequestStep in BatchRequestSteps)
                {
                    if (batchRequestStep.Value != null && batchRequestStep.Value.DependsOn != null)
                        while (batchRequestStep.Value.DependsOn.Remove(requestId)) ;
                }
            }
            return isRemoved;
        }

        /// <summary>
        /// Creates a new <see cref="BatchRequestContent"/> with all <see cref="BatchRequestStep"/> that failed.
        /// </summary>
        /// <param name="responseStatusCodes">A dictionary with response codes, get with batchResponseContent.GetResponsesStatusCodesAsync()</param>
        /// <returns>new <see cref="BatchRequestContent"/> with all failed requests.</returns>
        public BatchRequestContent NewBatchWithFailedRequests(Dictionary<string, HttpStatusCode> responseStatusCodes)
        {
#pragma warning disable CS0618
            var request = new BatchRequestContent(this.RequestAdapter);
#pragma warning restore CS0618
            foreach(var response in responseStatusCodes)
            {
                if (BatchRequestSteps.ContainsKey(response.Key) && !BatchResponseContent.IsSuccessStatusCode(response.Value)) {
#pragma warning disable CS0618
                    request.AddBatchRequestStep(BatchRequestSteps[response.Key]);
#pragma warning restore CS0618
                }
            }
            return request;
        }

        

        /// <summary>
        /// Get the content of the batchRequest in the form of a stream.
        /// It is the responsibility of the caller to dispose of the stream returned.
        /// </summary>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> to use for cancelling requests</param>
        /// <returns>A stream object with the contents of the batch request</returns>
        internal async Task<Stream> GetBatchRequestContentAsync(CancellationToken cancellationToken = default)
        {
            var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();//open the root object
                writer.WritePropertyName(CoreConstants.BatchRequest.Requests);// requests property name

                //write the elements of the requests array
                writer.WriteStartArray();
                foreach (KeyValuePair<string, BatchRequestStep> batchRequestStep in BatchRequestSteps)
                {
                    await WriteBatchRequestStepAsync(batchRequestStep.Value, writer,cancellationToken).ConfigureAwait(false);
                }
                writer.WriteEndArray();

                writer.WriteEndObject();//close the root object
                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

                //Reset the position since we want the caller to use this stream
                stream.Position = 0;

                return stream;
            }
        }

        private bool ContainsCorrespondingRequestId(IList<string> dependsOn)
        {
        	return dependsOn.All(requestId => BatchRequestSteps.ContainsKey(requestId));
        }

        private async Task WriteBatchRequestStepAsync(BatchRequestStep batchRequestStep, Utf8JsonWriter writer, CancellationToken cancellationToken)
        {
            writer.WriteStartObject();// open root object
            writer.WriteString(CoreConstants.BatchRequest.Id, batchRequestStep.RequestId);//write the id property
            writer.WriteString(CoreConstants.BatchRequest.Url, GetRelativeUrl(batchRequestStep.Request.RequestUri));//write the url property
            writer.WriteString(CoreConstants.BatchRequest.Method, batchRequestStep.Request.Method.Method);// write the method property

            // if the step depends on another step, write it
            if (batchRequestStep.DependsOn != null && batchRequestStep.DependsOn.Any())
            {
                writer.WritePropertyName(CoreConstants.BatchRequest.DependsOn);
                writer.WriteStartArray();
                foreach (var value in batchRequestStep.DependsOn)
                {
                    writer.WriteStringValue(value);//write the id it depends on
                }
                writer.WriteEndArray();
            }

            // write any headers if the step contains any request headers or content headers
            if ((batchRequestStep.Request.Headers?.Any() ?? false) 
                || (batchRequestStep.Request.Content?.Headers?.Any() ?? false))
            {
                // write the Headers property name for the batch object
                writer.WritePropertyName(CoreConstants.BatchRequest.Headers);
                writer.WriteStartObject();

                // write any request headers
                if (batchRequestStep.Request.Headers != null)
                {
                    foreach (var header in batchRequestStep.Request.Headers)
                    {
                        writer.WriteString(header.Key, GetHeaderValuesAsString(header.Value));
                    }
                }

                // write any content headers
                if (batchRequestStep.Request.Content?.Headers != null)
                {
                    foreach (var header in batchRequestStep.Request.Content.Headers)
                    {
                        writer.WriteString(header.Key, GetHeaderValuesAsString(header.Value));
                    }
                }

                writer.WriteEndObject();
            }

            // write the content of the step if it has any
            if (batchRequestStep.Request?.Content != null)
            {
                writer.WritePropertyName(CoreConstants.BatchRequest.Body);
                // allow for non json content by checking the header value
                var vendorSpecificContentType = batchRequestStep.Request.Content?.Headers?.ContentType?.MediaType?.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                using var contentStream = await GetRequestContentAsync(batchRequestStep.Request,cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrEmpty(vendorSpecificContentType) || vendorSpecificContentType.Equals(CoreConstants.MimeTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase))
                {
                    using var jsonDocument = await JsonDocument.ParseAsync(contentStream,cancellationToken:cancellationToken).ConfigureAwait(false);
                    jsonDocument.WriteTo(writer);
                }
                else
                {
                    writer.WriteStringValue(Convert.ToBase64String(contentStream.ToArray()));
                }
            }
            writer.WriteEndObject();//close root object.
        }

        private static async Task<MemoryStream> GetRequestContentAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var memoryStream = new MemoryStream();
#if NET5_0_OR_GREATER
            await request.Content.CopyToAsync(memoryStream,cancellationToken);
#else
            await request.Content.CopyToAsync(memoryStream);
#endif
            memoryStream.Position = 0; //reset the stream to start
            return memoryStream;
        }

        private string GetHeaderValuesAsString(IEnumerable<string> headerValues)
        {
            if (headerValues == null || !headerValues.Any())
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            foreach (string headerValue in headerValues)
            {
                builder.Append(headerValue);
            }

            return builder.ToString();
        }

        private string GetRelativeUrl(Uri requestUri)
        {
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));

            return requestUri.PathAndQuery.Substring(5); // `v1.0/` and `beta/` are both 5 characters
        }

        /// <summary>
        /// Serialize the HTTP content to a stream as an asynchronous operation.
        /// </summary>
        /// <param name="stream">The target stream.</param>
        /// <param name="context">Information about the transport (channel binding token, for example). This parameter may be null.</param>
        /// <returns></returns>
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            using (Stream batchContent = await GetBatchRequestContentAsync().ConfigureAwait(false))
            {
                await batchContent.CopyToAsync(stream).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Determines whether the HTTP content has a valid length in bytes.
        /// </summary>
        /// <param name="length">The length in bytes of the HTTP content.</param>
        /// <returns></returns>
        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
