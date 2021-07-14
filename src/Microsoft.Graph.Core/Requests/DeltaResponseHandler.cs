// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Text.Json;
    using System.IO;
    using System.Text;

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
                var responseWithChangelist = await GetResponseBodyWithChangelist(responseString);

                return this.serializer.DeserializeObject<T>(responseWithChangelist);
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

        /// <summary>
        /// Gets the response with change lists set on each item.
        /// </summary>
        /// <param name="deltaResponseBody">The entire response body as a string.</param>
        /// <returns>A task with a string representation of the response body. The changes are set on each response item.</returns>
        private async Task<string> GetResponseBodyWithChangelist(string deltaResponseBody)
        {
            // This is the JsonDocument that we will replace. We should probably
            // return a string instead.
            using (var responseJsonDocument = JsonDocument.Parse(deltaResponseBody))
            {
                // An array of delta objects. We will need to process 
                // each one independently of each other.
                if (!responseJsonDocument.RootElement.TryGetProperty("value", out var pageOfDeltaObjects))
                {
                    return deltaResponseBody;
                }

                var updatedObjectsWithChangeList = new List<JsonElement>();

                foreach (var deltaObject in pageOfDeltaObjects.EnumerateArray())
                {
                    var updatedObjectJsonDocument = await DiscoverChangedProperties(deltaObject);
                    updatedObjectsWithChangeList.Add(updatedObjectJsonDocument.RootElement);
                }

                // Replace the original page of changed items with a page of items that
                // have a self describing change list.
                var response = AddOrReplacePropertyToObject(responseJsonDocument.RootElement, "value", updatedObjectsWithChangeList);

                return response;
            }
        }

        /// <summary>
        /// Inspects the response item and captures the list of properties on a new property
        /// named 'changes'.
        /// </summary>
        /// <param name="responseItem">The item to inspect for properties.</param>
        /// <returns>The item with the 'changes' property set on it.</returns>
        private async Task<JsonDocument> DiscoverChangedProperties(JsonElement responseItem)
        {
            // List of changed properties.
            var changes = new List<string>();

            // Get the list of changed properties on the item.
            await GetObjectProperties(responseItem, changes);

            // Add the changes object to the response item.
            var response = AddOrReplacePropertyToObject(responseItem, "changes", changes);

            return JsonDocument.Parse(response);
        }

        /// <summary>
        /// Gets all changes on the object.
        /// </summary>
        /// <param name="changedObject">The responseItem to inspect for changes.</param>
        /// <param name="changes">The list of properties returned in the response.</param>
        /// <param name="parentName">The parent object of this changed object.</param>
        /// <returns></returns>
        private async Task GetObjectProperties(JsonElement changedObject, List<string> changes, string parentName = "")
        {
            if (!string.IsNullOrEmpty(parentName))
            {
                parentName += ".";
            }

            foreach (var property in changedObject.EnumerateObject())
            {
                switch (property.Value.ValueKind)
                {
                    case JsonValueKind.Object:
                    {
                        string parent = parentName + property.Name;
                        await GetObjectProperties(property.Value, changes, parent);
                        break;
                    }
                    case JsonValueKind.Array:
                    {
                        string parent = parentName + property.Name;

                        int index = 0;
                        foreach ( var arrayItem in property.Value.EnumerateArray())
                        {
                            string parentWithIndex = $"{parent}[{index}]";

                            if (arrayItem.ValueKind == JsonValueKind.Object)
                            {
                                await GetObjectProperties(arrayItem, changes, parentWithIndex);
                            }
                            else // Assuming that this is a Value item.
                            {
                                changes.Add(parentWithIndex);
                            }
                            index++;
                        }

                        break;
                    }
                    default:
                    {
                        var name = parentName + property.Name;
                        changes.Add(name);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a property with the given property name to the JsonElement object. This function is currently necessary as
        /// <see cref="JsonElement"/> is currently readonly.
        /// </summary>
        /// <param name="jsonElement">The Original JsonElement to add/replace a property</param>
        /// <param name="propertyName">The property name to use</param>
        /// <param name="newItem">The object to be added</param>
        /// <returns></returns>
        private string AddOrReplacePropertyToObject(JsonElement jsonElement, string propertyName, object newItem)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (Utf8JsonWriter utf8JsonWriter = new Utf8JsonWriter(memoryStream))
                {
                    utf8JsonWriter.WriteStartObject(); //write start of object
                    bool isReplacement = false;
                    foreach (var element in jsonElement.EnumerateObject())
                    {
                        if (element.Name.Equals(propertyName))
                        {
                            isReplacement = true; // we are replacing an existing property
                            utf8JsonWriter.WritePropertyName(element.Name); //write the property name
                            // Try to get a JsonElement so that we can write it to the stream
                            string newJsonElement = this.serializer.SerializeObject(newItem);
                            using (var newJsonDocument = JsonDocument.Parse(newJsonElement))
                            {
                                newJsonDocument.RootElement.WriteTo(utf8JsonWriter); // write the object
                            }
                        }
                        else
                        {
                            element.WriteTo(utf8JsonWriter); // write out as is
                        }
                    }

                    // The property name did not exist so we a are writing something new
                    if (!isReplacement)
                    {
                        utf8JsonWriter.WritePropertyName(propertyName); //write the property name
                        // Try to get a JsonElement so that we can write it to the stream
                        string newJsonElement = this.serializer.SerializeObject(newItem);
                        using (var newJsonDocument = JsonDocument.Parse(newJsonElement))
                        {
                            newJsonDocument.RootElement.WriteTo(utf8JsonWriter); // write the object
                        }
                    }

                    utf8JsonWriter.WriteEndObject(); //write end of object
                }

                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }
    }
}
