// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net;
    using Microsoft.Graph.Core.Helpers;
    /// <summary>
    /// An <see cref="DelegatingHandler"/> implementation using standard .NET libraries.
    /// </summary>
    public class AuthenticationHandler: DelegatingHandler
    {
        /// <summary>
        /// MaxRetry property for 401's
        /// </summary>
        public int MaxRetry { get; set; } = 1;

        /// <summary>
        /// AuthenticationProvider property
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider { get; set; }

        /// <summary>
        /// Construct a new <see cref="AuthenticationHandler"/>
        /// </summary>
        public AuthenticationHandler()
        {

        }

        /// <summary>
        /// Construct a new <see cref="AuthenticationHandler"/>
        /// <param name="authenticationProvider">An authentication provider to pass to <see cref="AuthenticationHandler"/> for authenticating requests.</param>
        /// </summary>
        public AuthenticationHandler(IAuthenticationProvider authenticationProvider)
        {
            AuthenticationProvider = authenticationProvider;
        }

        /// <summary>
        /// Construct a new <see cref="AuthenticationHandler"/>
        /// </summary>
        /// <param name="authenticationProvider">An authentication provider to pass to <see cref="AuthenticationHandler"/> for authenticating requests.</param>
        /// <param name="innerHandler">A HTTP message handler to pass to the <see cref="AuthenticationHandler"/> for sending requests.</param>
        public AuthenticationHandler(IAuthenticationProvider authenticationProvider, HttpMessageHandler innerHandler)
        {
            InnerHandler = innerHandler;
            AuthenticationProvider = authenticationProvider;
        }

        /// <summary>
        /// Check HTTP response message status code if it's unauthorized (401) or not
        /// </summary>
        /// <param name="httpResponseMessage">The <see cref="HttpResponseMessage"/>to send.</param>
        /// <returns></returns>
        private bool IsUnauthorized(HttpResponseMessage httpResponseMessage)
        {
            return httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized;
        }

        /// <summary>
        /// Retry sending HTTP request
        /// </summary>
        /// <param name="httpResponseMessage">The <see cref="HttpResponseMessage"/>to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>to send.</param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> SendRetryAsync(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken)
        {
            int retryAttempt = 0;
            while (retryAttempt < MaxRetry)
            {
                var originalRequest = httpResponseMessage.RequestMessage;

                // Authenticate request using AuthenticationProvider
                await AuthenticationProvider.AuthenticateRequestAsync(originalRequest);
                httpResponseMessage = await base.SendAsync(originalRequest, cancellationToken);

                retryAttempt++;

                if (!IsUnauthorized(httpResponseMessage) || !ContentHelper.IsBuffered(originalRequest))
                {
                    // Re-issue the request to get a new access token
                    return httpResponseMessage;
                }
            }

            return httpResponseMessage;
        }

        /// <summary>
        /// Send a HTTP request and retries the request when the response if unauthorized
        /// </summary>
        /// <param name="httpRequest">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
        {
            // Authenticate request using AuthenticationProvider
            if (AuthenticationProvider != null)
            {
                await AuthenticationProvider.AuthenticateRequestAsync(httpRequest);
            }

            HttpResponseMessage response = await base.SendAsync(httpRequest, cancellationToken);

            // Chcek if response is a 401 & is not a streamed body (is buffered)
            if (IsUnauthorized(response) && ContentHelper.IsBuffered(httpRequest) && (AuthenticationProvider != null))
            {
                // re-issue the request to get a new access token
                response = await SendRetryAsync(response, cancellationToken);
            }
            
            return response;
        }
    }
}
