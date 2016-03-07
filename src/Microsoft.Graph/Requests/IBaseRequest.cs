// ------------------------------------------------------------------------------
//  Copyright (c) 2016 Microsoft Corporation
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Net.Http;

    public interface IBaseRequest
    {
        /// <summary>
        /// Gets or sets the content type for the request.
        /// </summary>
        string ContentType { get; set; }

        /// <summary>
        /// Gets the <see cref="HeaderOption"/> collection for the request.
        /// </summary>
        IList<HeaderOption> Headers { get; }

        /// <summary>
        /// Gets the <see cref="IGraphServiceClient"/> for handling requests.
        /// </summary>
        IBaseClient Client { get; }

        /// <summary>
        /// Gets or sets the HTTP method string for the request.
        /// </summary>
        string Method { get; }

        /// <summary>
        /// Gets the URL for the request, without query string.
        /// </summary>
        string RequestUrl { get; }

        /// <summary>
        /// Gets the <see cref="QueryOption"/> collection for the request.
        /// </summary>
        IList<QueryOption> QueryOptions { get; }

        /// <summary>
        /// Gets the <see cref="HttpRequestMessage"/> representation of the request.
        /// </summary>
        /// <returns>The <see cref="HttpRequestMessage"/> representation of the request.</returns>
        HttpRequestMessage GetHttpRequestMessage();
    }
}
