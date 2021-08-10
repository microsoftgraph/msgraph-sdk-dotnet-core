// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Net;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class NextLinkConverter : JsonConverter<string>
    {
        /// <summary>
        /// Checks if the given object can be converted into a next link url.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <returns>True if the object is of type Duration.</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        /// <summary>
        /// Deserialize the JSON data into a decoded nextLink url string.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
        /// <param name="typeToConvert">The object type.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> for conversion.</param>
        /// <returns>A TimeOfDay object.</returns>
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert == null)
                throw new ArgumentNullException(nameof(typeToConvert));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return WebUtility.UrlDecode(reader.GetString());
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
        /// <param name="value">The nextLink url value.</param>
        /// <param name="options">The calling serializer options</param>
        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            writer.WriteStringValue(value);
        }
    }
}
