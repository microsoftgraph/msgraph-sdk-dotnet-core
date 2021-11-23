// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// The GraphResponse Object
    /// </summary>
    public class GraphResponse<T> : GraphResponse
    {
        /// <summary>
        /// The GraphResponse Constructor
        /// </summary>
        /// <param name="requestInformation">The Request made for the response</param>
        /// <param name="httpResponseMessage">The response</param>
        public GraphResponse(RequestInformation requestInformation, HttpResponseMessage httpResponseMessage)
            : base(requestInformation, httpResponseMessage)
        {
        }

        /// <summary>
        /// Gets the deserialized object 
        /// </summary>
        public async Task<T> GetResponseObjectAsync(IResponseHandler responseHandler)
        {
            return await responseHandler.HandleResponseAsync<HttpResponseMessage,T>(this.ToHttpResponseMessage());
        }
    }
}