// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions;
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
        /// <typeparam name="NativeResponseType">The type of the response</typeparam>
        /// <typeparam name="ModelType">The type to return</typeparam>
        /// <param name="response">The HttpResponseMessage to handle</param>
        /// <returns></returns>
        public async Task<ModelType> HandleResponseAsync<NativeResponseType, ModelType>(NativeResponseType response)
        {
            if (response is HttpResponseMessage responseMessage && responseMessage.Content != null)
            {
                using var responseStream = await responseMessage.Content.ReadAsStreamAsync();
                return serializer.DeserializeObject<ModelType>(responseStream);
            }

            return default(ModelType);
        }
    }
}
