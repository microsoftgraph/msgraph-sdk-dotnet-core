// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    /// <summary>
    /// Handles batch request responses.
    /// </summary>
    public class BatchResponseContent
    {
        private JsonDocument jBatchResponseObject;
        private readonly HttpResponseMessage batchResponseMessage;
        private readonly Dictionary<string, ParsableFactory<IParsable>> apiErrorMappings;

        /// <summary>
        /// Constructs a new <see cref="BatchResponseContent"/>
        /// </summary>
        /// <param name="httpResponseMessage">A <see cref="HttpResponseMessage"/> of a batch request execution.</param>
        /// <param name="errorMappings">A dictionary of error mappings to handle failed responses.</param>
        public BatchResponseContent(HttpResponseMessage httpResponseMessage, Dictionary<string, ParsableFactory<IParsable>> errorMappings = null)
        {
            this.batchResponseMessage = httpResponseMessage ?? throw new ArgumentNullException(nameof(httpResponseMessage));
            this.apiErrorMappings = errorMappings ?? new();
        }

        /// <summary>
        /// Gets all batch responses <see cref="Dictionary{String, HttpResponseMessage}"/>.
        /// All <see cref="HttpResponseMessage"/> in the dictionary MUST be disposed since they implement <see cref="IDisposable"/>.
        /// </summary>
        /// <returns>A Dictionary of id and <see cref="HttpResponseMessage"/> representing batch responses.</returns>
        public async Task<Dictionary<string, HttpResponseMessage>> GetResponsesAsync()
        {
            Dictionary<string, HttpResponseMessage> responseMessages = new Dictionary<string, HttpResponseMessage>();
            jBatchResponseObject = jBatchResponseObject ?? await GetBatchResponseContentAsync().ConfigureAwait(false);
            if (jBatchResponseObject == null)
                return responseMessages;

            if(jBatchResponseObject.RootElement.TryGetProperty(CoreConstants.BatchRequest.Responses, out JsonElement jResponses) && jResponses.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement jResponseItem in jResponses.EnumerateArray())
                    responseMessages.Add(jResponseItem.GetProperty(CoreConstants.BatchRequest.Id).ToString(), GetResponseMessageFromJObject(jResponseItem));
            }
            return responseMessages;
        }

        /// <summary>
        /// Gets all batch responses statuscodes <see cref="Dictionary{String, HttpStatusCode}"/>.
        /// </summary>
        /// <returns>A Dictionary of id and <see cref="HttpStatusCode"/> representing batch responses.</returns>
        public async Task<Dictionary<string, HttpStatusCode>> GetResponsesStatusCodesAsync()
        {
            Dictionary<string, HttpStatusCode> statuscodes = new Dictionary<string, HttpStatusCode>();
            jBatchResponseObject = jBatchResponseObject ?? await GetBatchResponseContentAsync().ConfigureAwait(false);
            if (jBatchResponseObject == null)
                return statuscodes;

            if (jBatchResponseObject.RootElement.TryGetProperty(CoreConstants.BatchRequest.Responses, out JsonElement jResponses) && jResponses.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement jResponseItem in jResponses.EnumerateArray())
                    statuscodes.Add(jResponseItem.GetProperty(CoreConstants.BatchRequest.Id).ToString(), GetStatusCodeFromJObject(jResponseItem));
            }
            return statuscodes;
        }

        /// <summary>
        /// Gets a batch response as <see cref="HttpResponseMessage"/> for the specified batch request id.
        /// The returned <see cref="HttpResponseMessage"/> MUST be disposed since it implements an <see cref="IDisposable"/>.
        /// </summary>
        /// <param name="requestId">A batch request id.</param>
        /// <returns>A <see cref="HttpResponseMessage"/> response object for a batch request.</returns>
        public async Task<HttpResponseMessage> GetResponseByIdAsync(string requestId)
        {
            jBatchResponseObject = jBatchResponseObject ?? await GetBatchResponseContentAsync().ConfigureAwait(false);
            if (jBatchResponseObject == null)
                return null;

            if (jBatchResponseObject.RootElement.TryGetProperty(CoreConstants.BatchRequest.Responses, out JsonElement jResponses) && jResponses.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in jResponses.EnumerateArray())
                {
                    if (element.GetProperty(CoreConstants.BatchRequest.Id).GetString().Equals(requestId))
                    {
                        return GetResponseMessageFromJObject(element);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a batch response as a requested type for the specified batch request id.
        /// </summary>
        /// <param name="requestId">A batch request id.</param>
        /// <param name="responseHandler">ResponseHandler to use for the response</param>
        /// <returns>A deserialized object of type T<see cref="HttpResponseMessage"/>.</returns>
        public async Task<T> GetResponseByIdAsync<T>(string requestId, IResponseHandler responseHandler = null) where T : IParsable, new()
        {
            using var httpResponseMessage = await GetResponseByIdAsync(requestId).ConfigureAwait(false);
            if (httpResponseMessage == null)
                return default;

            // return the deserialized object
            responseHandler ??= new ResponseHandler<T>();
            return await responseHandler.HandleResponseAsync<HttpResponseMessage, T>(httpResponseMessage, apiErrorMappings).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets a batch response content as a stream
        /// </summary>
        /// <param name="requestId">A batch request id.</param>
        /// <returns>The response stream of the batch response object</returns>
        /// <remarks> Stream should be dispose once done with.</remarks>
        public async Task<Stream> GetResponseStreamByIdAsync(string requestId)
        {
            using var httpResponseMessage = await GetResponseByIdAsync(requestId).ConfigureAwait(false);
            if (httpResponseMessage == null)
                return default;

            using var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
            return memoryStream;
        }

        /// <summary>
        /// Gets the @NextLink of a batch response.
        /// </summary>
        /// <returns></returns>
        [Obsolete("This method is deprecated as a batch response does not contain a next link",true)]
        public async Task<string> GetNextLinkAsync()
        {
            jBatchResponseObject = jBatchResponseObject ?? await GetBatchResponseContentAsync().ConfigureAwait(false);
            if (jBatchResponseObject == null)
                return null;

            if (jBatchResponseObject.RootElement.TryGetProperty(CoreConstants.Serialization.ODataNextLink, out JsonElement nextLink))
            {
                return nextLink.GetString();
            }

            return null;
        }

        /// <summary>
        /// Checks is a <see cref="HttpStatusCode"/> can be marked as successful
        /// </summary>
        /// <param name="statusCode">A single <see cref="HttpStatusCode"/>.</param>
        /// <returns>Returns true if status code is between 200 and 300.</returns>
        public static bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return ((int)statusCode >= 200) && ((int)statusCode <= 299); 
        }

        /// <summary>
        /// Gets a <see cref="HttpResponseMessage"/> from <see cref="JsonElement"/> representing a batch response item.
        /// </summary>
        /// <param name="jResponseItem">A single batch response item of type <see cref="JsonElement"/>.</param>
        /// <returns>A single batch response as a <see cref="HttpResponseMessage"/>.</returns>
        private HttpResponseMessage GetResponseMessageFromJObject(JsonElement jResponseItem)
        {
            HttpResponseMessage responseMessage = new HttpResponseMessage();

            if (jResponseItem.TryGetProperty(CoreConstants.BatchRequest.Status, out JsonElement status))
            {
                responseMessage.StatusCode = (HttpStatusCode)int.Parse(status.ToString());
            }

            if (jResponseItem.TryGetProperty(CoreConstants.BatchRequest.Body, out JsonElement body))
            {
                responseMessage.Content = new StringContent(body.ToString(), Encoding.UTF8, CoreConstants.MimeTypeNames.Application.Json);
            }

            if (jResponseItem.TryGetProperty(CoreConstants.BatchRequest.Headers, out JsonElement headers))
            {
                foreach (var headerKeyValue in headers.EnumerateObject())
                {
                    // try to add the header to the request message otherwise add it to the content message if the content is set
                    if (!responseMessage.Headers.TryAddWithoutValidation(headerKeyValue.Name, headerKeyValue.Value.ToString()) && responseMessage.Content != null)
                    {
                        if(headerKeyValue.Name.Equals("Content-Type",StringComparison.OrdinalIgnoreCase))
                            responseMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(headerKeyValue.Value.ToString()); // we do this to override the default and to allow content types with parameters e.g. Content-Type: application/json; odata=verbose
                        else
                            responseMessage.Content.Headers.TryAddWithoutValidation(headerKeyValue.Name, headerKeyValue.Value.ToString());// Try to add the headers we couldn't add to the HttpResponseMessage to the HttpContent
                    }
                }
            }
            return responseMessage;
        }


        private HttpStatusCode GetStatusCodeFromJObject(JsonElement jResponseItem)
        {
            if (jResponseItem.TryGetProperty(CoreConstants.BatchRequest.Status, out JsonElement status))
            {
                return (HttpStatusCode)int.Parse(status.ToString());
            }
            throw new ArgumentException("Response does not contain statuscode");
        }
        /// <summary>
        /// Gets the <see cref="HttpContent"/> of a batch response as a <see cref="JsonDocument"/>.
        /// </summary>
        /// <returns>A batch response content as <see cref="JsonDocument"/>.</returns>
        private async Task<JsonDocument> GetBatchResponseContentAsync()
        {
            if (this.batchResponseMessage.Content == null || this.batchResponseMessage.Content.Headers.ContentLength == 0 )
                return null;

            try
            {
                using (Stream streamContent = await this.batchResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    return await JsonDocument.ParseAsync(streamContent).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw new ClientException(ErrorConstants.Messages.UnableToDeserializeContent, ex);
            }
        }
    }
}
