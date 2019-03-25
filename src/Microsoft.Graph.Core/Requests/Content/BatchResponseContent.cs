// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    /// <summary>
    /// Handles batch request responses.
    /// </summary>
    public class BatchResponseContent
    {
        private JObject jBatchResponseObject;
        private HttpResponseMessage batchResponseMessage;

        /// <summary>
        /// Constructs a new <see cref="BatchResponseContent"/>
        /// </summary>
        /// <param name="httpResponseMessage">A <see cref="HttpResponseMessage"/> of a batch request execution.</param>
        public BatchResponseContent(HttpResponseMessage httpResponseMessage)
        {
            // TODO: Throw right exception
            this.batchResponseMessage = httpResponseMessage ?? throw new ArgumentNullException("httpResponseMessage", "httpResponseMessage cannot be null.");
        }

        /// <summary>
        /// Gets all batch responses <see cref="Dictionary{String, HttpResponseMessage}"/>.
        /// </summary>
        /// <returns>A Dictionary of id and <see cref="HttpResponseMessage"/> representing batch responses.</returns>
        public async Task<Dictionary<string, HttpResponseMessage>> GetResponsesAsync()
        {
            Dictionary<string, HttpResponseMessage> responseMessages = new Dictionary<string, HttpResponseMessage>();
            jBatchResponseObject = jBatchResponseObject ?? await GetBatchResponseContentAsync();
            if (jBatchResponseObject == null)
                return responseMessages;

            if(jBatchResponseObject.TryGetValue("responses", out JToken jResponses))
            {
                foreach (JObject jResponseItem in jResponses)
                    responseMessages.Add(jResponseItem.GetValue("id").ToString(), GetResponseMessageFromJObject(jResponseItem));
            }
            return responseMessages;
        }

        /// <summary>
        /// Gets a batch response as <see cref="HttpResponseMessage"/> for the specified batch request id.
        /// </summary>
        /// <param name="requestId">A batch request id.</param>
        /// <returns>A <see cref="HttpResponseMessage"/> response object for a batch request.</returns>
        public async Task<HttpResponseMessage> GetResponseByIdAsync(string requestId)
        {
            jBatchResponseObject = jBatchResponseObject ?? await GetBatchResponseContentAsync();
            if (jBatchResponseObject == null)
                return null;

            JObject jResponseItem = null;

            if (jBatchResponseObject.TryGetValue("responses", out JToken jResponses))
            {
                jResponseItem = jResponses.FirstOrDefault((jtoken) => jtoken.Value<string>("id").Equals(requestId)) as JObject;
            }

            return GetResponseMessageFromJObject(jResponseItem);
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

            return jBatchResponseObject.GetValue("@nextLink")?.ToString();
        }

        /// <summary>
        /// Gets a <see cref="HttpResponseMessage"/> from <see cref="JObject"/> representing a batch response item.
        /// </summary>
        /// <param name="jResponseItem">A single batch response item of type <see cref="JObject"/>.</param>
        /// <returns>A single batch response as a <see cref="HttpResponseMessage"/>.</returns>
        private HttpResponseMessage GetResponseMessageFromJObject(JObject jResponseItem)
        {
            if (jResponseItem == null)
                return null;

            HttpResponseMessage responseMessage = new HttpResponseMessage();

            if (jResponseItem.TryGetValue("status", out JToken status))
            {
                responseMessage.StatusCode = (HttpStatusCode)int.Parse(status.ToString());
            }

            if (jResponseItem.TryGetValue("body", out JToken body))
            {
                responseMessage.Content = new StringContent(body.ToString(), Encoding.UTF8, "application/json");
            }

            if (jResponseItem.TryGetValue("headers", out JToken headers))
            {
                foreach (KeyValuePair<string, string> headerKeyValue in headers.ToObject<Dictionary<string, string>>())
                {
                    responseMessage.Headers.TryAddWithoutValidation(headerKeyValue.Key, headerKeyValue.Value);
                }
            }
            return responseMessage;
        }

        /// <summary>
        /// Gets the <see cref="HttpContent"/> of a batch response as <see cref="JObject"/>.
        /// </summary>
        /// <returns>A batch response content as <see cref="JObject"/>.</returns>
        private async Task<JObject> GetBatchResponseContentAsync()
        {
            if (this.batchResponseMessage.Content == null)
                return null;

            try
            {
                string content = await this.batchResponseMessage.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<JObject>(content);
            }
            catch (JsonReaderException ex)
            {
                // TODO: Throw proper exception
                throw new ServiceException(new Error { Code = "Invalid json response", Message = "Invalid json response" }, ex);
            }
        }
    }
}
