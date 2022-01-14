// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

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
    /// A <see cref="HttpContent"/> implementation to handle json batch requests.
    /// </summary>
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
        /// <param name="baseClient">The <see cref="BaseClient"/> for making requests</param>
        public BatchRequestContent(BaseClient baseClient)
            :this(baseClient, new BatchRequestStep[] { })
        {
        }

        /// <summary>
        /// Constructs a new <see cref="BatchRequestContent"/>.
        /// </summary>
        /// <param name="baseClient">The <see cref="BaseClient"/> for making requests</param>
        /// <param name="batchRequestSteps">A list of <see cref="BatchRequestStep"/> to add to the batch request content.</param>
        public BatchRequestContent(BaseClient baseClient, params BatchRequestStep[] batchRequestSteps)
        {
            if (batchRequestSteps == null)
                throw new ClientException(new Error
                {
                    Code = ErrorConstants.Codes.InvalidArgument,
                    Message = string.Format(ErrorConstants.Messages.NullParameter, nameof(batchRequestSteps))
                });

            if (batchRequestSteps.Count() > CoreConstants.BatchRequest.MaxNumberOfRequests)
                throw new ClientException(new Error {
                    Code = ErrorConstants.Codes.MaximumValueExceeded,
                    Message = string.Format(ErrorConstants.Messages.MaximumValueExceeded, "Number of batch request steps", CoreConstants.BatchRequest.MaxNumberOfRequests)
                });

            this.Headers.ContentType = new MediaTypeHeaderValue(CoreConstants.MimeTypeNames.Application.Json);

            BatchRequestSteps = new Dictionary<string, BatchRequestStep>();

            foreach (BatchRequestStep requestStep in batchRequestSteps)
            {
                if(requestStep.DependsOn != null && !ContainsCorrespondingRequestId(requestStep.DependsOn))
                {
                    throw new ClientException(new Error
                    {
                        Code = ErrorConstants.Codes.InvalidArgument,
                        Message = ErrorConstants.Messages.InvalidDependsOnRequestId
                    });
                }
                AddBatchRequestStep(requestStep);
            }

            this.RequestAdapter = baseClient?.RequestAdapter ?? throw new ArgumentNullException(nameof(baseClient.RequestAdapter));
        }

        /// <summary>
        /// Adds a <see cref="BatchRequestStep"/> to batch request content if doesn't exists.
        /// </summary>
        /// <param name="batchRequestStep">A <see cref="BatchRequestStep"/> to add.</param>
        /// <returns>True or false based on addition or not addition of the provided <see cref="BatchRequestStep"/>. </returns>
        public bool AddBatchRequestStep(BatchRequestStep batchRequestStep)
        {
            if (batchRequestStep == null
                || BatchRequestSteps.ContainsKey(batchRequestStep.RequestId)
                || BatchRequestSteps.Count >= CoreConstants.BatchRequest.MaxNumberOfRequests //we should not add any more steps
                )
            {
                return false;
            }

            (BatchRequestSteps as IDictionary<string, BatchRequestStep>).Add(batchRequestStep.RequestId, batchRequestStep);
            return true;
        }

        /// <summary>
        /// Adds a <see cref="HttpRequestMessage"/> to batch request content.
        /// </summary>
        /// <param name="httpRequestMessage">A <see cref="HttpRequestMessage"/> to use to build a <see cref="BatchRequestStep"/> to add.</param>
        /// <returns>The requestId of the newly created <see cref="BatchRequestStep"/></returns>
        public string AddBatchRequestStep(HttpRequestMessage httpRequestMessage)
        {
            if (BatchRequestSteps.Count >= CoreConstants.BatchRequest.MaxNumberOfRequests)
                throw new ClientException(new Error
                {
                    Code = ErrorConstants.Codes.MaximumValueExceeded,
                    Message = string.Format(ErrorConstants.Messages.MaximumValueExceeded, "Number of batch request steps", CoreConstants.BatchRequest.MaxNumberOfRequests)
                });

            string requestId = Guid.NewGuid().ToString();
            BatchRequestStep batchRequestStep = new BatchRequestStep(requestId, httpRequestMessage);
            (BatchRequestSteps as IDictionary<string, BatchRequestStep>).Add(batchRequestStep.RequestId, batchRequestStep);
            return requestId;
        }

        /// <summary>
        /// Adds a <see cref="RequestInformation"/> to batch request content
        /// </summary>
        /// <param name="requestInformation">A <see cref="RequestInformation"/> to use to build a <see cref="BatchRequestStep"/> to add.</param>
        /// <returns>The requestId of the  newly created <see cref="BatchRequestStep"/></returns>
        public string AddBatchRequestStep(RequestInformation requestInformation)
        {
            if (BatchRequestSteps.Count >= CoreConstants.BatchRequest.MaxNumberOfRequests)
                throw new ClientException(new Error
                {
                    Code = ErrorConstants.Codes.MaximumValueExceeded,
                    Message = string.Format(ErrorConstants.Messages.MaximumValueExceeded, "Number of batch request steps", CoreConstants.BatchRequest.MaxNumberOfRequests)
                });
            string requestId = Guid.NewGuid().ToString();
            var requestMessage = ((HttpClientRequestAdapter)RequestAdapter).GetRequestMessageFromRequestInformation(requestInformation);
            BatchRequestStep batchRequestStep = new BatchRequestStep(requestId, requestMessage);
            (BatchRequestSteps as IDictionary<string, BatchRequestStep>).Add(batchRequestStep.RequestId, batchRequestStep);
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
                throw new ClientException(
                    new Error
                        {
                            Code = ErrorConstants.Codes.InvalidArgument,
                            Message = string.Format(ErrorConstants.Messages.NullParameter, nameof(requestId))
                        });

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
        /// Get the content of the batchRequest in the form of a stream.
        /// It is the responsibility of the caller to dispose of the stream returned.
        /// </summary>
        /// <returns>A stream object with the contents of the batch request</returns>
        internal async Task<Stream> GetBatchRequestContentAsync()
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
                    await WriteBatchRequestStepAsync(batchRequestStep.Value, writer);
                }
                writer.WriteEndArray();

                writer.WriteEndObject();//close the root object
                await writer.FlushAsync();

                //Reset the position since we want the caller to use this stream
                stream.Position = 0;

                return stream;
            }
        }

        private bool ContainsCorrespondingRequestId(IList<string> dependsOn)
        {
        	return dependsOn.All(requestId => BatchRequestSteps.ContainsKey(requestId));
        }

        private async Task WriteBatchRequestStepAsync(BatchRequestStep batchRequestStep, Utf8JsonWriter writer)
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
            if (batchRequestStep.Request != null && batchRequestStep.Request.Content != null)
            {
                writer.WritePropertyName(CoreConstants.BatchRequest.Body);
                using (JsonDocument content = await GetRequestContentAsync(batchRequestStep.Request))
                {
                    content.WriteTo(writer);
                }
            }
            writer.WriteEndObject();//close root object.
        }

        private async Task<JsonDocument> GetRequestContentAsync(HttpRequestMessage request)
        {
            try
            {
                HttpRequestMessage clonedRequest = await request.CloneAsync();
                using (Stream streamContent = await clonedRequest.Content.ReadAsStreamAsync())
                {
                    return JsonDocument.Parse(streamContent);
                }
            }
            catch (Exception ex)
            {
                throw new ClientException(new Error
                {
                    Code = ErrorConstants.Codes.InvalidRequest,
                    Message = ErrorConstants.Messages.UnableToDeserializeContent
                }, ex);
            }
        }

        private string GetHeaderValuesAsString(IEnumerable<string> headerValues)
        {
            if (headerValues == null || headerValues.Count() == 0)
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
            using (Stream batchContent = await GetBatchRequestContentAsync())
            {
                await batchContent.CopyToAsync(stream);
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
