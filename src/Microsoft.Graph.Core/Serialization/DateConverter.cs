// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// The Date Converter.
    /// </summary>
    public class DateConverter : JsonConverter<Date>
    {
        /// <summary>
        /// Check if the given object can be converted into a Date.
        /// </summary>
        /// <param name="objectType">The type of the object.</param>
        /// <returns>True if the object is a Date type.</returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(Date))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Converts the JSON object into a Date object
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
        /// <param name="typeToConvert">The object type.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> to use on deserialization.</param>
        /// <returns></returns>
        public override Date Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                var dateTime = DateTime.Parse(reader.GetString());
                return new Date(dateTime);
            }
            catch (Exception dateParseException)
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.GeneralException,
                        Message = ErrorConstants.Messages.UnableToDeserializeDate,
                    },
                    dateParseException);
            }
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to write to.</param>
        /// <param name="date">The date value.</param>
        /// <param name="options">The calling serializer options</param>
        public override void Write(Utf8JsonWriter writer, Date date, JsonSerializerOptions options)
        {
            if (date != null)
            {
                writer.WriteStringValue(date.ToString());
            }
            else
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.GeneralException,
                        Message = ErrorConstants.Messages.InvalidTypeForDateConverter,
                    });
            }
        }
    }
}
