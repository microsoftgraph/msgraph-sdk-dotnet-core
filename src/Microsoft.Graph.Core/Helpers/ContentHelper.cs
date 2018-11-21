// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.Core.Helpers
{
    using System.Net.Http;
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
