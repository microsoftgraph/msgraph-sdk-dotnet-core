// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides method(s) to deserialize raw HTTP responses into strong types.
    /// </summary>
    public class ResponseHandler : IResponseHandler
    {
        private readonly ISerializer serializer;
        /// <summary>
        /// Constructs a new <see cref="ResponseHandler"/>.
        /// </summary>
        /// <param name="serializer"></param>
        public ResponseHandler(ISerializer serializer)
        {
            this.serializer = serializer;
        }

        /// <summary>
        /// Process raw HTTP response into requested domain type.
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="response">The HttpResponseMessage to handle</param>
        /// <returns></returns>
        public async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.Content != null)
            {
                using (var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    return serializer.DeserializeObject<T>(responseStream);
                }
            }

            return default(T);
        }
    }
}
