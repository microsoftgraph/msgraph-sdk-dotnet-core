// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
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

        /// <summary>
        /// Gets a serializer for serializing and deserializing JSON objects.
        /// </summary>
        public ISerializer Serializer { get; private set; }

        /// <summary>
        /// Constructs a new <see cref="BatchResponseContent"/>
        /// </summary>
        /// <param name="httpResponseMessage">A <see cref="HttpResponseMessage"/> of a batch request execution.</param>
        /// <param name="serializer">A serializer for serializing and deserializing JSON objects.</param>
        public BatchResponseContent(HttpResponseMessage httpResponseMessage, ISerializer serializer = null)
        {
            this.batchResponseMessage = httpResponseMessage ?? throw new ClientException(new Error
            {
                Code = ErrorConstants.Codes.InvalidArgument,
                Message = string.Format(ErrorConstants.Messages.NullParameter, nameof(httpResponseMessage))
            });

            this.Serializer = serializer ?? new Serializer();
        }

        /// <summary>
        /// Gets all batch responses <see cref="Dictionary{String, HttpResponseMessage}"/>.
        /// All <see cref="HttpResponseMessage"/> in the dictionary MUST be disposed since they implement <see cref="IDisposable"/>.
        /// </summary>
        /// <returns>A Dictionary of id and <see cref="HttpResponseMessage"/> representing batch responses.</returns>
        public async Task<Dictionary<string, HttpResponseMessage>> GetResponsesAsync()
        {
            Dictionary<string, HttpResponseMessage> responseMessages = new Dictionary<string, HttpResponseMessage>();
            jBatchResponseObject = jBatchResponseObject ?? await GetBatchResponseContentAsync();
            if (jBatchResponseObject == null)
                return responseMessages;

            if(jBatchResponseObject.RootElement.TryGetProperty(CoreConstants.BatchRequest.Responses, out JsonElement jResponses))
            {
                foreach (JsonElement jResponseItem in jResponses.EnumerateArray())
                    responseMessages.Add(jResponseItem.GetProperty(CoreConstants.BatchRequest.Id).ToString(), GetResponseMessageFromJObject(jResponseItem));
            }
            return responseMessages;
        }

        /// <summary>
        /// Gets a batch response as <see cref="HttpResponseMessage"/> for the specified batch request id.
        /// The returned <see cref="HttpResponseMessage"/> MUST be disposed since it implements an <see cref="IDisposable"/>.
        /// </summary>
        /// <param name="requestId">A batch request id.</param>
        /// <returns>A <see cref="HttpResponseMessage"/> response object for a batch request.</returns>
        public async Task<HttpResponseMessage> GetResponseByIdAsync(string requestId)
        {
            jBatchResponseObject = jBatchResponseObject ?? await GetBatchResponseContentAsync();
            if (jBatchResponseObject == null)
                return null;

            if (jBatchResponseObject.RootElement.TryGetProperty(CoreConstants.BatchRequest.Responses, out JsonElement jResponses))
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
        /// <returns>A deserialized object of type T<see cref="HttpResponseMessage"/>.</returns>
        public async Task<T> GetResponseByIdAsync<T>(string requestId)
        {
            using (var httpResponseMessage = await GetResponseByIdAsync(requestId))
            {
                var responseHandler = new ResponseHandler(new Serializer());
                
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    Error error;
                    string rawResponseBody = null;

                    //deserialize into an ErrorResponse as the result is not a success.
                    ErrorResponse errorResponse = await responseHandler.HandleResponse<ErrorResponse>(httpResponseMessage);

                    if (errorResponse?.Error == null)
                    {
                        if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                        {
                            error = new Error { Code = ErrorConstants.Codes.ItemNotFound };
                        }
                        else
                        {
                            error = new Error
                            {
                                Code = ErrorConstants.Codes.GeneralException,
                                Message = ErrorConstants.Messages.UnexpectedExceptionResponse
                            };
                        }
                    }
                    else
                    {
                        error = errorResponse.Error;
                    }

                    if (httpResponseMessage.Content?.Headers.ContentType.MediaType == "application/json")
                    {
                        rawResponseBody = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }

                    throw new ServiceException(error, httpResponseMessage.Headers, httpResponseMessage.StatusCode, rawResponseBody);
                }

                // return the deserialized object
                return await responseHandler.HandleResponse<T>(httpResponseMessage);
            }
        }

        /// <summary>
        /// Gets the @NextLink of a batch response.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetNextLinkAsync()
        {
            jBatchResponseObject = jBatchResponseObject ?? await GetBatchResponseContentAsync();
            if (jBatchResponseObject == null)
                return null;

            if (jBatchResponseObject.RootElement.TryGetProperty(CoreConstants.Serialization.ODataNextLink, out JsonElement nexlink))
            {
                return nexlink.ToString();
            }

            return null;
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
                    responseMessage.Headers.TryAddWithoutValidation(headerKeyValue.Name, headerKeyValue.Value.ToString());
                }
            }
            return responseMessage;
        }

        /// <summary>
        /// Gets the <see cref="HttpContent"/> of a batch response as a <see cref="JsonDocument"/>.
        /// </summary>
        /// <returns>A batch response content as <see cref="JsonDocument"/>.</returns>
        private async Task<JsonDocument> GetBatchResponseContentAsync()
        {
            if (this.batchResponseMessage.Content == null)
                return null;

            try
            {
                using (Stream streamContent = await this.batchResponseMessage.Content.ReadAsStreamAsync())
                {
                    return JsonDocument.Parse(streamContent);
                }
            }
            catch (Exception ex)
            {
                throw new ClientException(new Error
                {
                    Code = ErrorConstants.Codes.InvalidRequest,
                    Message = ErrorConstants.Messages.UnableToDeserializexContent
                }, ex);
            }
        }
    }
}
