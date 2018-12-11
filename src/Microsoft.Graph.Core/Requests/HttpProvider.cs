// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An <see cref="IHttpProvider"/> implementation using standard .NET libraries.
    /// </summary>
    public class HttpProvider : IHttpProvider
    {
        internal bool disposeHandler;

        internal HttpClient httpClient;

        internal HttpMessageHandler httpMessageHandler;

        /// <summary>
        /// Constructs a new <see cref="HttpProvider"/>.
        /// </summary>
        /// <param name="authenticationProvider">The <see cref="IAuthenticationProvider"/> for authenticating request messages.</param>
        /// <param name="serializer">A serializer for serializing and deserializing JSON objects.</param>
        public HttpProvider(IAuthenticationProvider authenticationProvider, ISerializer serializer = null)
            : this((HttpMessageHandler)null, true, authenticationProvider, serializer)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="HttpProvider"/>.
        /// </summary>
        /// <param name="httpClientHandler">An HTTP client handler to pass to the <see cref="HttpClient"/> for sending requests.</param>
        /// <param name="disposeHandler">Whether or not to dispose the client handler on Dispose().</param>
        /// <param name="authenticationProvider">The <see cref="IAuthenticationProvider"/> for authenticating request messages.</param>
        /// <param name="serializer">A serializer for serializing and deserializing JSON objects.</param>
        /// <remarks>
        ///     By default, HttpProvider disables automatic redirects and handles redirects to preserve authentication headers. If providing
        ///     an <see cref="HttpClientHandler"/> to the constructor and enabling automatic redirects this could cause issues with authentication
        ///     over the redirect.
        /// </remarks>
        public HttpProvider(HttpClientHandler httpClientHandler, bool disposeHandler, IAuthenticationProvider authenticationProvider, ISerializer serializer = null)
            : this((HttpMessageHandler)httpClientHandler, disposeHandler, authenticationProvider, serializer)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="HttpProvider"/>.
        /// </summary>
        /// <param name="httpMessageHandler">An HTTP message handler to pass to the <see cref="HttpClient"/> for sending requests.</param>
        /// <param name="disposeHandler">Whether or not to dispose the client handler on Dispose().</param>
        /// <param name="authenticationProvider">The <see cref="IAuthenticationProvider"/> for authenticating request messages.</param>
        /// <param name="serializer">A serializer for serializing and deserializing JSON objects.</param>
        public HttpProvider(HttpMessageHandler httpMessageHandler, bool disposeHandler, IAuthenticationProvider authenticationProvider, ISerializer serializer)
        {
            this.disposeHandler = disposeHandler;
            this.httpMessageHandler = httpMessageHandler ?? new HttpClientHandler { AllowAutoRedirect = false };
            this.Serializer = serializer ?? new Serializer();

            DelegatingHandler[] handlers = new DelegatingHandler[]
            {
                new RedirectHandler(),
                new RetryHandler(),
                new AuthenticationHandler(authenticationProvider)
            };

            this.httpClient = GraphClientFactory.CreateClient(this.httpMessageHandler, handlers);
        }

        /// <summary>
        /// Gets or sets the overall request timeout.
        /// </summary>
        public TimeSpan OverallTimeout
        {
            get
            {
                return this.httpClient.Timeout;
            }

            set
            {
                try
                {
                    this.httpClient.Timeout = value;
                }
                catch (InvalidOperationException exception)
                {
                    throw new ServiceException(
                        new Error
                        {
                            Code = ErrorConstants.Codes.NotAllowed,
                            Message = ErrorConstants.Messages.OverallTimeoutCannotBeSet,
                        },
                        exception);
                }
            }
        }

        /// <summary>
        /// Gets a serializer for serializing and deserializing JSON objects.
        /// </summary>
        public ISerializer Serializer { get; private set; }

        /// <summary>
        /// Disposes the HttpClient and HttpClientHandler instances.
        /// </summary>
        public void Dispose()
        {
            if (this.httpClient != null)
            {
                this.httpClient.Dispose();
            }
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return this.SendAsync(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="completionOption">The <see cref="HttpCompletionOption"/> to pass to the <see cref="IHttpProvider"/> on send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        public async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            try
            {
                return await this.httpClient.SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException exception)
            {
                throw new ServiceException(
                        new Error
                        {
                            Code = ErrorConstants.Codes.Timeout,
                            Message = ErrorConstants.Messages.RequestTimedOut,
                        },
                        exception);
            }
            catch (Exception exception)
            {
                throw new ServiceException(
                        new Error
                        {
                            Code = ErrorConstants.Codes.GeneralException,
                            Message = ErrorConstants.Messages.UnexpectedExceptionOnSend,
                        },
                        exception);
            }
        }
    }
}
