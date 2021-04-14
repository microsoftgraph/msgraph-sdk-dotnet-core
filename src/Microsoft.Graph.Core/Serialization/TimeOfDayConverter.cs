// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Handles serialization and deserialization for TimeOfDay.
    /// </summary>
    public class TimeOfDayConverter : JsonConverter<TimeOfDay>
    {
        /// <summary>
        /// Checks if the given type can be converted to a TimeOfDay.
        /// </summary>
        /// <param name="objectType">The object type.</param>
        /// <returns>True if the object is type match of TimeOfDay.</returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(TimeOfDay))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deserialize the JSON data into a TimeOfDay object.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
        /// <param name="typeToConvert">The object type.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> for conversion.</param>
        /// <returns>A TimeOfDay object.</returns>
        public override TimeOfDay Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                var dateTime = DateTime.Parse(reader.GetString());
                return new TimeOfDay(dateTime);
            }
            catch (FormatException formatException)
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.GeneralException,
                        Message = "Unable to deserialize time of day"
                    },
                    formatException);
            }
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
        /// <param name="value">The <see cref="TimeOfDay"/> value.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> of the calling serializer.</param>
        public override void Write(Utf8JsonWriter writer, TimeOfDay value, JsonSerializerOptions options)
        {
            if (value != null)
            {
                writer.WriteStringValue(value.ToString());
            }
            else
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.GeneralException,
                        Message = "Invalid type for time of day converter"
                    });
            }
        }
    }
}
