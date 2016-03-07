// ------------------------------------------------------------------------------
//  Copyright (c) 2016 Microsoft Corporation
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Linq;
    using System.Globalization;

    /// <summary>
    /// Handles resolving interfaces to the correct derived class during serialization/deserialization.
    /// </summary>
    public class DerivedTypeConverter : JsonConverter
    {
        private static string assemblyName;

        private TextInfo textInfo;

        static DerivedTypeConverter()
        {
            assemblyName = typeof(DerivedTypeConverter).GetTypeInfo().Assembly.FullName;
        }

        public DerivedTypeConverter()
            : base()
        {
            this.textInfo = CultureInfo.CurrentCulture.TextInfo;
        }

        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Deserializes the object to the correct type.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">The interface type.</param>
        /// <param name="existingValue">The existing value of the object being read.</param>
        /// <param name="serializer">The <see cref="JsonSerializer"/> for deserialization.</param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);

            var type = jObject.GetValue(Constants.Serialization.ODataType);

            var instance = this.Create(type == null ? objectType.FullName : type.ToString());

            using (var objectReader = this.GetObjectReader(reader, jObject))
            {
                serializer.Populate(objectReader, instance);
                return instance;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the type string to title case.
        /// </summary>
        /// <param name="typeString">The type string.</param>
        /// <returns>The converted string.</returns>
        private string ConvertTypeToTitleCase(string typeString)
        {
            var stringSegments = typeString.Split('.').Select(
                segment => string.Concat(segment.Substring(0, 1).ToUpperInvariant(), segment.Substring(1)));
            return string.Join(".", stringSegments);
        }

        private object Create(string typeString)
        {
            typeString = typeString.TrimStart('#');
            typeString = this.ConvertTypeToTitleCase(typeString);

            try
            {
                return Activator.CreateInstance(Type.GetType(typeString));
            }
            catch(Exception exception)
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = GraphErrorCode.GeneralException.ToString(),
                        Message = "An unexpected error occurred during deserialization."
                    },
                    exception);
            }
        }

        private JsonReader GetObjectReader(JsonReader originalReader, JObject jObject)
        {
            var objectReader = jObject.CreateReader();
            
            objectReader.Culture = originalReader.Culture;
            objectReader.DateFormatString = originalReader.DateFormatString;
            objectReader.DateParseHandling = originalReader.DateParseHandling;
            objectReader.DateTimeZoneHandling = originalReader.DateTimeZoneHandling;
            objectReader.FloatParseHandling = originalReader.FloatParseHandling;

            // This reader is only for a single token. Don't dispose the stream after.
            objectReader.CloseInput = false;

            return objectReader;
        }
    }
}
