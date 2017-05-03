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

    internal static class ComplexTypeHelper
    {
        public static void ConvertComplexTypeProperties<TObject>(this IDictionary<string, object> additionalData, string odataType)
        {
            Lazy<JsonSerializer> serializer = new Lazy<JsonSerializer>(JsonSerializer.Create);
            foreach (var item in additionalData.ToList())
            {
                JObject complexValue = item.Value as JObject;
                if (complexValue != null)
                {
                    if (complexValue.TryGetValue("@odata.type", out JToken value) && value.Value<string>() == odataType)
                    {
                        additionalData[item.Key] = serializer.Value.Deserialize<TObject>(complexValue.CreateReader());
                    }
                }
            }
        }
    }
}
