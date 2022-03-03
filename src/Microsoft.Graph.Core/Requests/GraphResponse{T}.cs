// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;
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
        /// <param name="responseHandler">The response handler to use for the reponse</param>
        /// <param name="errorMappings">The errorMappings to use in the event of a non sucess request</param>
        public async Task<T> GetResponseObjectAsync(IResponseHandler responseHandler, Dictionary<string, ParsableFactory<IParsable>> errorMappings = null)
        {
            return await responseHandler.HandleResponseAsync<HttpResponseMessage,T>(this.ToHttpResponseMessage(), errorMappings).ConfigureAwait(false);
        }
    }
}