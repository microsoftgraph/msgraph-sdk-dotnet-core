// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------
namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Threading;
    using System.Net.Http;
    using System.Net;
    using System.Net.Http.Headers;

    /// <summary>
    /// An <see cref="DelegatingHandler"/> implementation using standard .NET libraries.
    /// </summary>
    public class RetryHandler : DelegatingHandler
    {

        private const string RETRY_AFTER = "Retry-After";
        private const string RETRY_ATTEMPT = "Retry-Attempt";
        private const int DELAY_MILLISECONDS = 10000;
        private double m_pow = 1;

        /// <summary>
        /// RetryOption property
        /// </summary>
        internal RetryHandlerOption RetryOption { get; set; }

        /// <summary>
        /// Construct a new <see cref="RetryHandler"/>
        /// </summary>
        /// <param name="retryOption">An OPTIONAL <see cref="Microsoft.Graph.RetryHandlerOption"/> to configure <see cref="RetryHandler"/></param>
        public RetryHandler(RetryHandlerOption retryOption = null)
        {
            RetryOption = retryOption ?? new RetryHandlerOption();
        }

        /// <summary>
        /// Construct a new <see cref="RetryHandler"/>
        /// </summary>
        /// <param name="innerHandler">An HTTP message handler to pass to the <see cref="HttpMessageHandler"/> for sending requests.</param>
        /// <param name="retryOption">An OPTIONAL <see cref="Microsoft.Graph.RetryHandlerOption"/> to configure <see cref="RetryHandler"/></param>
        public RetryHandler(HttpMessageHandler innerHandler, RetryHandlerOption retryOption = null)
            :this(retryOption)
        {
            InnerHandler = innerHandler;
        }

        /// <summary>
        /// Send a HTTP request 
        /// </summary>
        /// <param name="httpRequest">The HTTP request<see cref="HttpRequestMessage"/>needs to be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
        {
            RetryOption = httpRequest.GetMiddlewareOption<RetryHandlerOption>() ?? RetryOption;

            var response = await base.SendAsync(httpRequest, cancellationToken);

            if (RetryOption.ShouldRetry(response) && httpRequest.IsBuffered())
            {
                response = await SendRetryAsync(response, cancellationToken);
            }

            return response;
        }

        /// <summary>
        /// Retry sending the HTTP request 
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> which is returned and includes the HTTP request needs to be retried.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the retry.</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendRetryAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {


            int retryCount = 0;

          
            while (retryCount < RetryOption.MaxRetry)
            {

                // Call Delay method to get delay time from response's Retry-After header or by exponential backoff 
                Task delay = Delay(response, retryCount, cancellationToken);

                // Get the original request
                var request = response.RequestMessage;

                // Increase retryCount and then update Retry-Attempt in request header
                retryCount++;
                AddOrUpdateRetryAttempt(request, retryCount);

                // Delay time
                await delay;

                // Call base.SendAsync to send the request
                response = await base.SendAsync(request, cancellationToken);

                if (!RetryOption.ShouldRetry(response) || !request.IsBuffered())
                {
                    return response;
                }

            }
            throw new ServiceException(
                         new Error
                         {
                             Code = ErrorConstants.Codes.TooManyRetries,
                             Message = string.Format(ErrorConstants.Messages.TooManyRetriesFormatString, retryCount)
                         });

        }

        /// <summary>
        /// Update Retry-Attempt header in the HTTP request
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>needs to be sent.</param>
        /// <param name="retry_count">Retry times</param>
        private void AddOrUpdateRetryAttempt(HttpRequestMessage request, int retry_count)
        {
            if (request.Headers.Contains(RETRY_ATTEMPT))
            {
                request.Headers.Remove(RETRY_ATTEMPT);
            }
            request.Headers.Add(RETRY_ATTEMPT, retry_count.ToString());
        }

        /// <summary>
        /// Delay task operation based on Retry-After header in the response or exponential backoff
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>returned.</param>
        /// <param name="retry_count">The retry counts</param>
        /// <param name="cancellationToken">The cancellationToken for the Http request</param>
        /// <returns>The <see cref="Task"/> for delay operation.</returns>
        public Task Delay(HttpResponseMessage response, int retry_count, CancellationToken cancellationToken)
        {
            
            TimeSpan delay = TimeSpan.FromMilliseconds(0);
            HttpHeaders headers = response.Headers;
            if (headers.TryGetValues(RETRY_AFTER, out IEnumerable<string> values))
            {
                string retry_after = values.First();    
                if (Int32.TryParse(retry_after, out int delay_seconds))
                {
                    delay = TimeSpan.FromSeconds(delay_seconds);
                }
            }
            else
            {

                m_pow = Math.Pow(2, retry_count); // m_pow = Pow(2, retry_count)

                double delay_time = m_pow * DELAY_MILLISECONDS;
              
                delay = TimeSpan.FromMilliseconds(delay_time);
            }
            return Task.Delay(delay, cancellationToken);

        }


    }
}
