// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Handles resolving interfaces to the correct concrete class during serialization/deserialization.
    /// </summary>
    public class InterfaceConverter<T> : JsonConverter<T> where T : class
    {
        /// <summary>
        /// Checks if the given object can be converted. In this instance, all object can be converted.
        /// </summary>
        /// <param name="typeToConvert">The type of the object to convert.</param>
        /// <returns>True</returns>
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsInterface && typeToConvert.IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Deserializes the object to the correct type.
        /// </summary>
        /// <param name="reader">The <see cref="Utf8JsonReader"/> to read from.</param>
        /// <param name="typeToConvert">The interface type.</param>
        /// <param name="options">The <see cref="JsonSerializerOptions"/> for deserialization.</param>
        /// <returns></returns>
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            DerivedTypeConverter<T> derivedTypeConverter = new DerivedTypeConverter<T>();
            return (T)derivedTypeConverter.Read(ref reader, typeof(T), options);
        }

        /// <summary>
        /// Serializes object to writer
        /// </summary>
        /// <param name="writer">The <see cref="Utf8JsonWriter"/> to serialize to</param>
        /// <param name="value">The value to serialize</param>
        /// <param name="options">The serializer options to use.</param>
        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, typeof(T), options);
        }
    }
}
