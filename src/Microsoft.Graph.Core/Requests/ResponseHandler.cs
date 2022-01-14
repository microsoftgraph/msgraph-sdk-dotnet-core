// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Serialization;
    using Microsoft.Kiota.Serialization.Json;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides method(s) to deserialize raw HTTP responses into strong types.
    /// </summary>
    public class ResponseHandler<T> : IResponseHandler where T : IParsable
    {
        private readonly JsonParseNodeFactory _jsonParseNodeFactory;

        /// <summary>
        /// Constructs a new <see cref="ResponseHandler{T}"/>.
        /// </summary>
        public ResponseHandler()
        {
            _jsonParseNodeFactory = new JsonParseNodeFactory();
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
                var jsonParseNode = _jsonParseNodeFactory.GetRootParseNode(responseMessage.Content.Headers?.ContentType?.MediaType?.ToLowerInvariant(), responseStream);
                return (ModelType)(object)jsonParseNode.GetObjectValue<T>();
            }

            return default;
        }
    }
}
