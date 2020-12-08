// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Newtonsoft.Json;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// An <see cref="ISerializer"/> implementation using the JSON.NET serializer.
    /// </summary>
    public class Serializer : ISerializer
    {
        readonly JsonSerializerOptions jsonSerializerOptions;

        /// <summary>
        /// Constructor for the serializer with defaults for the JsonSerializer settings.
        /// </summary>
        public Serializer()
            : this(
                  new JsonSerializerOptions
                  {
                      IgnoreNullValues = true,
                      PropertyNameCaseInsensitive = true
                  })
        {
        }

        /// <summary>
        /// Constructor for the serializer.
        /// </summary>
        /// <param name="jsonSerializerSettings">The serializer settings to apply to the serializer.</param>
        public Serializer(JsonSerializerOptions jsonSerializerSettings)
        {
            this.jsonSerializerOptions = jsonSerializerSettings;
            this.jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            this.jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            this.jsonSerializerOptions.Converters.Add(new DateTimeOffsetConverter());
        }

        /// <summary>
        /// Deserializes the stream to an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The deserialization type.</typeparam>
        /// <param name="stream">The stream to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public T DeserializeObject<T>(Stream stream)
        {
            if (stream == null)
            {
                return default(T);
            }

            return JsonSerializer.DeserializeAsync<T>(stream, this.jsonSerializerOptions).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Deserializes the JSON string to an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The deserialization type.</typeparam>
        /// <param name="inputString">The JSON string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public T DeserializeObject<T>(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                return default(T);
            }

            return JsonSerializer.Deserialize<T>(inputString, this.jsonSerializerOptions);
        }

        /// <summary>
        /// Serializes the specified object into a JSON string.
        /// </summary>
        /// <param name="serializeableObject">The object to serialize.</param>
        /// <returns>The JSON string.</returns>
        public string SerializeObject(object serializeableObject)
        {
            if (serializeableObject == null)
            {
                return null;
            }

            var stream = serializeableObject as Stream;
            if (stream != null)
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }

            var stringValue = serializeableObject as string;
            if (stringValue != null)
            {
                return stringValue;
            }

            return JsonSerializer.Serialize(serializeableObject, this.jsonSerializerOptions);
        }
    }
}
