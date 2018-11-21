using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Microsoft.Graph.Core.Helpers
{
    internal static class ContentHelper
    {
        /// <summary>
        /// Check the HTTP request's content to determine if it's buffered content or not.
        /// </summary>
        /// <param name="httpRequest">The <see cref="HttpRequestMessage"/>needs to be sent.</param>
        /// <returns></returns>
        internal static bool IsBuffered(HttpRequestMessage httpRequest)
        {
            HttpContent requestContent = httpRequest.Content;

            if ((httpRequest.Method == HttpMethod.Put || httpRequest.Method == HttpMethod.Post || httpRequest.Method.Method.Equals("PATCH"))
                && requestContent != null && (requestContent.Headers.ContentLength == null || (int)requestContent.Headers.ContentLength == -1))
            {
                return false;
            }
            return true;
        }
    }
}
