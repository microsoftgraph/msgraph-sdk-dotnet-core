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
        /// MaxRetry property
        /// </summary>
        public int MaxRetry { set; get; } = 10;

        /// <summary>
        /// Construct a new <see cref="RetryHandler"/>
        /// </summary>
        public RetryHandler()
        {
        }

        /// <summary>
        /// Construct a new <see cref="RetryHandler"/>
        /// </summary>
        /// <param name="innerHandler">An HTTP message handler to pass to the <see cref="HttpMessageHandler"/> for sending requests.</param>
        public RetryHandler(HttpMessageHandler innerHandler)
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
          
            var response = await base.SendAsync(httpRequest, cancellationToken);

            if (IsRetry(response) && httpRequest.IsBuffered())
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

            while (retryCount < MaxRetry)
            {
                // Call Delay method to get delay time from response's Retry-After header or by exponential backoff 
                Task delay = Delay(response, retryCount, cancellationToken);

                // general clone request with internal CloneAsync (see CloneAsync for details) extension method 
                var request = await response.RequestMessage.CloneAsync();

                // Increase retryCount and then update Retry-Attempt in request header
                retryCount++;
                AddOrUpdateRetryAttempt(request, retryCount);

                // Delay time
                await delay;

                // Call base.SendAsync to send the request
                response = await base.SendAsync(request, cancellationToken);

                if (!IsRetry(response) || !request.IsBuffered())
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
        /// Check the HTTP response's status to determine whether it should be retried or not.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>returned.</param>
        /// <returns></returns>
        public bool IsRetry(HttpResponseMessage response)
        {
            if ((response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == (HttpStatusCode)429))
            {
                return true;
            }
            return false;
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
