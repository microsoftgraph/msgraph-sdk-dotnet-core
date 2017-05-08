// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// This class provides internal functionality that allows converting complex typed values in dynamic properties of open types to their native types. 
    /// </summary>
    internal static class ComplexTypeHelper
    {
        /// <summary>
        /// For the given dynamic property dictionary, converts the entries with the speicifed odata type value to thegiven native type.
        /// </summary>
        /// <typeparam name="TObject">The native type of the objects to create</typeparam>
        /// <param name="additionalData">Dynamic properties dictionary</param>
        /// <param name="odataType">OData type name of the object.</param>
        /// <remarks>
        /// This method will inplace replace JObject anonymous types in the dynamic properties dictionary with the deserialized native type given as the 
        /// type argument if the anonymous type has the correct OData type value. Other dynamic type properties will not be altered.
        /// </remarks>
        public static void ConvertComplexTypeProperties<TObject>(this IDictionary<string, object> additionalData, string odataType)
        {
            Lazy<JsonSerializer> serializer = new Lazy<JsonSerializer>(JsonSerializer.Create);

            // Create a list of entries so we can modify the dictionary while enumerating.
            foreach (var item in additionalData.ToList())
            {
                // JSON serializer creates JObject type when the type of the structure is not known.
                JObject complexValue = item.Value as JObject;
                if (complexValue != null)
                {
                    if (complexValue.TryGetValue(CoreConstants.Serialization.ODataType, out JToken value) && value.Value<string>() == odataType)
                    {
                        additionalData[item.Key] = serializer.Value.Deserialize<TObject>(complexValue.CreateReader());
                    }
                }
            }
        }
    }
}
