using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Microsoft.Graph
{
    public static class SerializerExtentions
    {
        public static HttpContent SerializeAsJsonContent(this ISerializer serializer, object source )
        {
            var stringContent = serializer.SerializeObject(source);
            return new StringContent(stringContent, Encoding.UTF8, "application/json");
        }
        
    }
}
