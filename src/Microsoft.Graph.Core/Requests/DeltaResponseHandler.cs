// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
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
                // Gets the response string with response headers and status code
                // set on the response body object.
                var responseString = await GetResponseString(response);

                // Get the response body object with the change list 
                // set on each response item.
                JObject responseBody = await GetResponseBodyWithChangelist(responseString);

                return responseBody.ToObject<T>();
            }

            return default(T);
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
                // Add headers
                var responseHeaders = hrm.Headers;
                var statusCode = hrm.StatusCode;

                Dictionary<string, string[]> headerDictionary = responseHeaders.ToDictionary(x => x.Key, x => x.Value.ToArray());
                var responseHeaderString = serializer.SerializeObject(headerDictionary);

                responseContent = content.Substring(0, content.Length - 1) + ", ";
                responseContent += "\"responseHeaders\": " + responseHeaderString + ", ";
                responseContent += "\"statusCode\": \"" + statusCode + "\"}";
            }

            return responseContent;
        }

        private async Task<JObject> GetResponseBodyWithChangelist(string deltaResponseBody)
        {
            // This is the JObject that we will replace. We should probably
            // return a string instead.
            JObject responseJObject = JObject.Parse(deltaResponseBody);

            // An array of delta objects. We will need to process 
            // each one independently of each other.
            var pageOfDeltaObjects = responseJObject["value"] as JArray;
            // TODO: check for empty response. Return the original JObject.

            JArray updatedObjectsWithChangeList = new JArray();

            for (int i = 0; i < pageOfDeltaObjects.Count(); i++)
            {
                // Go inspect all of the properties in the responseItem
                var updatedObject = await DiscoverAllChangedProperties(pageOfDeltaObjects[i] as JObject);
                updatedObjectsWithChangeList.Add(updatedObject);
            }

            // Replace the original page of changed items with a page of items that
            // have a self describing change list.
            responseJObject["value"].Replace(updatedObjectsWithChangeList);

            return responseJObject;

            throw new NotImplementedException();
        }

        public async Task<JObject> DiscoverAllChangedProperties(JObject responseItem)
        {
            // List of changed properties.
            JArray changes = new JArray();

            // Get the list of changed properties on the item.
            await GetObjectProperties(responseItem, changes);

            // Add the changes object to the response item.
            responseItem.Add("changes", changes);

            return responseItem;
        }

        public async Task GetObjectProperties(JObject changedObject, JArray changes, string parentName = "")
        {
            if (parentName != string.Empty)
            {
                parentName += ".";
            }

            foreach (JProperty property in changedObject.Children())
            {
                if (property.Value is JObject)
                {
                    string parent = parentName + property.Name;
                    await GetObjectProperties(property.Value as JObject, changes, parent);
                }
                else if (property.Value is JArray)
                {
                    string parent = parentName + property.Name;

                    JArray collection = (property.Value as JArray);
                    for (int i = 0; i < collection.Count(); i++)
                    {
                        string parentWithIndex = $"{parent}[{i}]";

                        JObject collectionItem = collection[i] as JObject;

                        await GetObjectProperties(collectionItem, changes, parentWithIndex);
                    }
                }
                else if (property.Value is JValue)
                {
                    changes.Add(parentName + property.Name);
                }
                else
                {
                    throw new NotImplementedException("Case is not a JObject, JArray, or JProperty");
                }
            }
        }
    }
}
