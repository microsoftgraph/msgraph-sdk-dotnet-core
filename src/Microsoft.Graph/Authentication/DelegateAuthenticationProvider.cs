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
    using System.Net.Http;
    using System.Threading.Tasks;

    public delegate Task AuthenticateRequestAsyncDelegate(HttpRequestMessage request);

    /// <summary>
    /// A default <see cref="IAuthenticationProvider"/> implementation.
    /// </summary>
    public class DelegateAuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// Constructs an <see cref="DelegateAuthenticationProvider"/>.
        /// </summary>
        public DelegateAuthenticationProvider(AuthenticateRequestAsyncDelegate authenticateRequestAsyncDelegate)
        {
            this.AuthenticateRequestAsyncDelegate = authenticateRequestAsyncDelegate;
        }

        /// <summary>
        /// Gets or sets the delegate for authenticating requests.
        /// </summary>
        public AuthenticateRequestAsyncDelegate AuthenticateRequestAsyncDelegate { get; set; }

        /// <summary>
        /// Authenticates the specified request message.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate.</param>
        public Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            if (this.AuthenticateRequestAsyncDelegate != null)
            {
                return this.AuthenticateRequestAsyncDelegate(request);
            }

            return Task.FromResult(0);
        }
    }
}
