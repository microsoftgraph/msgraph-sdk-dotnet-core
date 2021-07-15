// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
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
        /// <param name="iBaseRequest">The Request made for the response</param>
        /// <param name="httpResponseMessage">The response</param>
        public GraphResponse(IBaseRequest iBaseRequest, HttpResponseMessage httpResponseMessage)
            : base(iBaseRequest, httpResponseMessage)
        {
        }

        /// <summary>
        /// Gets the deserialized object 
        /// </summary>
        public async Task<T> GetResponseObjectAsync()
        {
            return await this.BaseRequest.ResponseHandler.HandleResponse<T>(this.ToHttpResponseMessage());
        }
    }
}