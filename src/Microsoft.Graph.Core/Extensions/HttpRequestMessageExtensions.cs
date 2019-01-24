// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Net.Http;
    /// <summary>
    /// Contains extension methods for <see cref="HttpRequestMessage"/>
    /// </summary>
    internal static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Checks the HTTP request's content to determine if it's buffered or streamed content.
        /// </summary>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/>needs to be sent.</param>
        /// <returns></returns>
        internal static bool IsBuffered(this HttpRequestMessage httpRequestMessage)
        {
            HttpContent requestContent = httpRequestMessage.Content;

            if ((httpRequestMessage.Method == HttpMethod.Put || httpRequestMessage.Method == HttpMethod.Post || httpRequestMessage.Method.Method.Equals("PATCH"))
                && requestContent != null && (requestContent.Headers.ContentLength == null || (int)requestContent.Headers.ContentLength == -1))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Gets a <see cref="GraphRequestContext"/> from <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> representation of the request.</param>
        /// <returns></returns>
        public static GraphRequestContext GetRequestContext(this HttpRequestMessage httpRequestMessage)
        {
            GraphRequestContext requestContext = new GraphRequestContext();
            if (httpRequestMessage.Properties.TryGetValue(typeof(GraphRequestContext).ToString(), out var requestContextObject))
            {
                requestContext = (GraphRequestContext)requestContextObject;
            }
            return requestContext;
        }

        /// <summary>
        /// Gets a <see cref="IMiddlewareOption"/> from <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> representation of the request.</param>
        /// <returns>A middleware option</returns>
        public static T GetMiddlewareOption<T>(this HttpRequestMessage httpRequestMessage) where T : IMiddlewareOption
        {
            IMiddlewareOption option = null;
            GraphRequestContext requestContext = httpRequestMessage.GetRequestContext();
            if (requestContext.MiddlewareOptions != null)
            {
                requestContext.MiddlewareOptions.TryGetValue(typeof(T).ToString(), out option);
            }
            return (T)option;
        }
    }
}
