// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net;
    /// <summary>
    /// A <see cref="DelegatingHandler"/> implementation using standard .NET libraries.
    /// </summary>
    public class AuthenticationHandler: DelegatingHandler
    {
        /// <summary>
        /// MaxRetry property for 401's
        /// </summary>
        private int MaxRetry { get; set; } = 1;

        /// <summary>
        /// AuthOption property
        /// </summary>
        internal AuthOption AuthOption { get; set; }

        /// <summary>
        /// AuthenticationProvider property
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider { get; set; }

        /// <summary>
        /// Construct a new <see cref="AuthenticationHandler"/>
        /// <param name="authenticationProvider">An authentication provider to pass to <see cref="AuthenticationHandler"/> for authenticating requests.</param>
        /// </summary>
        /// <param name="authOption">An OPTIONAL <see cref="Microsoft.Graph.AuthOption"/> to configure <see cref="AuthenticationHandler"/></param>
        public AuthenticationHandler(IAuthenticationProvider authenticationProvider, AuthOption authOption = null)
        {
            AuthenticationProvider = authenticationProvider;
            AuthOption = authOption ?? new AuthOption();
        }

        /// <summary>
        /// Construct a new <see cref="AuthenticationHandler"/>
        /// </summary>
        /// <param name="authenticationProvider">An authentication provider to pass to <see cref="AuthenticationHandler"/> for authenticating requests.</param>
        /// <param name="innerHandler">A HTTP message handler to pass to the <see cref="AuthenticationHandler"/> for sending requests.</param>
        /// <param name="authOption">An OPTIONAL <see cref="Microsoft.Graph.AuthOption"/> to configure <see cref="AuthenticationHandler"/></param>
        public AuthenticationHandler(IAuthenticationProvider authenticationProvider, HttpMessageHandler innerHandler, AuthOption authOption = null)
            :this(authenticationProvider, authOption)
        {
            InnerHandler = innerHandler;
            AuthenticationProvider = authenticationProvider;
        }

        /// <summary>
        /// Checks HTTP response message status code if it's unauthorized (401) or not
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

                if (!IsUnauthorized(httpResponseMessage) || !originalRequest.IsBuffered())
                {
                    // Re-issue the request to get a new access token
                    return httpResponseMessage;
                }
            }

            return httpResponseMessage;
        }

        /// <summary>
        /// Sends a HTTP request and retries the request when the response is unauthorized.
        /// This can happen when a token from the cache expires between graph getting the request and the backend receiving the request
        /// </summary>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
        {
            // If default auth provider is not set, use the option
            var authProvider = AuthenticationProvider ?? httpRequestMessage.GetMiddlewareOption<AuthOption>()?.AuthenticationProvider;

            // Authenticate request using AuthenticationProvider
            if (authProvider != null)
            {
                AuthOption = httpRequestMessage.GetMiddlewareOption<AuthOption>() ?? AuthOption;

                await authProvider.AuthenticateRequestAsync(httpRequestMessage);

                HttpResponseMessage response = await base.SendAsync(httpRequestMessage, cancellationToken);

                // Chcek if response is a 401 & is not a streamed body (is buffered)
                if (IsUnauthorized(response) && httpRequestMessage.IsBuffered())
                {
                    // re-issue the request to get a new access token
                    response = await SendRetryAsync(response, cancellationToken);
                }

                return response;
            }
            else
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.InvalidRequest,
                        Message = ErrorConstants.Messages.AuthenticationProviderMissing,
                    });
            }
        }
    }
}
