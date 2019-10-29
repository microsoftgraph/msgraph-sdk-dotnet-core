// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// PREVIEW 
    /// A response handler that exposes the list of changes returned in a response.
    /// This supports scenarios where the service expresses changes to 'null'. The
    /// deserializer can't express changes to null so you can now discover if a property
    /// has been set to null. This is intended for use with a Delta query scenario.
    /// </summary>
    public class DeltaResponseHandler : IResponseHandler
    {
        private readonly ISerializer serializer;
        /// <summary>
        /// Constructs a new <see cref="ResponseHandler"/>.
        /// </summary>
        /// <param name="serializer"></param>
        
        public DeltaResponseHandler()
        {
            this.serializer = new Serializer();
        }

        /// <summary>
        /// Process raw HTTP response into requested domain type.
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="response">The HttpResponseMessage to handle</param>
        /// <returns></returns>
        public async Task<T> HandleResponse<T>(HttpResponseMessage response) 
        {
            if (response.Content != null)
            {
                var responseString = await GetResponseString(response);

                return serializer.DeserializeObject<T>(responseString);
            }

            return default(T);
        }

        /// <summary>
        /// Get the change properties.
        /// </summary>
        /// <param name="responseString">The response from the service.</param>
        /// <returns>A list of changes.</returns>
        private List<string> GetChangedProperties(string responseString)
        {
            var changedPropertiesList = new List<string>();

            using (JsonTextReader reader = new JsonTextReader(new StringReader(responseString)))
            {
                // Forward the reader to the list of changes.
                while (reader.TokenType != JsonToken.StartArray) // This is a weak assumption. 
                {
                    reader.Read();
                }
                
                // Read all of the changed properties in the page and
                // add them to the changed property list.
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        changedPropertiesList.Add(reader.Path);
                    }
                }
            }
            return changedPropertiesList;
        }

        /// <summary>
        /// Get the response content string
        /// </summary>
        /// <param name="hrm">The response object</param>
        /// <returns>The full response string to return</returns>
        private async Task<string> GetResponseString(HttpResponseMessage hrm)
        {
            var responseContent = "";

            var content = await hrm.Content.ReadAsStringAsync().ConfigureAwait(false);

            //Only add headers and the changed list if we are going to return a response body
            if (content.Length > 0)
            {
                // Get the list of changes in the delta response. 
                List<string> changedProperties = GetChangedProperties(content);
                var changes = serializer.SerializeObject(changedProperties);

                // Add headers
                var responseHeaders = hrm.Headers;
                var statusCode = hrm.StatusCode;

                Dictionary<string, string[]> headerDictionary = responseHeaders.ToDictionary(x => x.Key, x => x.Value.ToArray());
                var responseHeaderString = serializer.SerializeObject(headerDictionary);

                responseContent = content.Substring(0, content.Length - 1) + ", ";
                responseContent += "\"responseHeaders\": " + responseHeaderString + ", ";
                responseContent += "\"statusCode\": \"" + statusCode + "\", ";
                responseContent += "\"changes\": " + changes + "}";
            }

            return responseContent;
        }
    }
}
