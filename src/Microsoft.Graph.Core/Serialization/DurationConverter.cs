// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Converter for serializing and deserializing Duration objects.
    /// </summary>
    public class DurationConverter : JsonConverter<Duration>
    {
        /// <summary>
        /// Checks if the given object can be converted into a Duration object.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <returns>True if the object is of type Duration.</returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(Duration))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deserialize the edm.duration into an Microsoft.Graph.Duration object.
        /// </summary>
        /// <returns>A Microsoft.Graph.Duration object.</returns>
        public override Duration Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                if (reader.TokenType == JsonTokenType.Null)
                {
                    return null;
                }
                string value = reader.GetString();
                return new Duration(value);
            }
            catch (JsonException serializationException)
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.GeneralException,
                        Message = "Unable to deserialize duration"
                    },
                    serializationException);
            }
        }

        /// <summary>
        /// Serializes the edm.duration representation of the Microsoft.Graph.Duration object.
        /// </summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
        /// <param name="value">The <see cref="Duration"/> value.</param>
        /// <param name="options">The calling serializer <see cref="JsonSerializerOptions"/>.</param>
        public override void Write(Utf8JsonWriter writer, Duration value, JsonSerializerOptions options)
        {
            var duration = value as Duration;

            if (duration != null)
            {
                writer.WriteStringValue(duration.ToString());
            }
            else
            {
                // Test the service whether it will accept an empty string. No need to throw an exception then.
                throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.GeneralException,
                        Message = "Invalid type for Duration converter"
                    });
            }
        }
    }
}
