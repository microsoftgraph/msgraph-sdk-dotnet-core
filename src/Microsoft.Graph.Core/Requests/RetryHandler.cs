// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

namespace Microsoft.Graph
{ 
    /// <summary>
    /// An <see cref="DelegatingHandler"/> implementation using standard .NET libraries.
    /// </summary>
    public class RetryHandler : DelegatingHandler
    {

        // property RetryPolicy
        //public RetryPolicy retryPolicy { get; private set; }

        // All value for fields are temporary
        private const int MAX_RETRY = 3;
        private const string RETRY_AFTER = "Retry-After";
        private const string RETRY_ATTEMPT = "Retry-Attempt";
        private const int DELAY_MILLISECONDES = 6000;
        private const int MAX_DELAY_MILLISECONDS = 10000;
        private int m_pow = 1;

        //public RetryHandler(int max_retry, int delay_milliseconds, int max_delay_milliseconds, HttpMessageHandler innerHandler)
        //{
        //    
        //}
        /// <summary>
        /// Constructs a new <see cref="RetryHandler"/>
        /// </summary>
        /// <param name="innerHandler">An HTTP message handler to pass to the <see cref="HttpMessageHandler"/> for sending requests.</param>
        public RetryHandler(HttpMessageHandler innerHandler)
        {
            InnerHandler = innerHandler;
        }

        // public RetryHandler(HttpMessageHandler innerHandler, RetryPolicy retryPolicy)
        //{
        //}

        /// <summary>
        /// Send a HTTP request 
        /// </summary>
        /// <param name="httpRequest">The HTTP request <see cref="HttpRequestMessage"/>need to be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
        {
            // Sends request first time
            var response = await base.SendAsync(httpRequest, cancellationToken);

            // Check whether needs to retry 
            if (IsRetry(response) && IsBuffed(httpRequest))
            {
                response = await SendRetryAsync(response, cancellationToken);
            }

            return response;
        }

        /// <summary>
        /// Retry sending the HTTP request 
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/> need to be retried.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the retry.</param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> SendRetryAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {


            int retryCount = 0;

            //Check if retry times less than maxRetry
            while (retryCount < MAX_RETRY)
            {

                // Call Delay method to get delay time from response's Retry-After header or from exponential backoff 
                // Start Task.Delay task
                Task delay = Delay(response, retryCount);

                // Get the original request
                var request = response.RequestMessage;

                // Increament retryCount and then update Retry-Attempt in request header
                retryCount++;
                AddOrUpdateRetryAttempt(request, retryCount);

                // Delay time
                await delay;

                // Call base.SendAsync to send the request
                response = await base.SendAsync(request, cancellationToken);

                if (!IsRetry(response) || !IsBuffed(request))
                {
                    return response;
                }

            }
            throw new ServiceException(
                         new Error
                         {
                             Code = ErrorConstants.Codes.TooManyRetries,
                             Message = string.Format(ErrorConstants.Messages.TooManyRedirectsFormatString, retryCount)
                         });

        }



        /// <summary>
        /// Check the HTTP response's status to determine whether it should be retried or not.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/></param>
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
        /// Check the HTTP request's payloads to determine whether it can be retried or not.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/></param>
        /// <returns></returns>
        private bool IsBuffed(HttpRequestMessage request)
        {
            return true;
        }

        /// <summary>
        /// Update Retry-Attempt header in the HTTP request
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/></param>
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
        /// <param name="response">The <see cref="HttpResponseMessage"/>returned</param>
        /// <param name="retry_count">The retry times</param>
        /// <returns></returns>
        public Task Delay(HttpResponseMessage response, int retry_count)
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
                if (retry_count < 31)
                {
                    m_pow = m_pow << 1; // m_pow = Pow(2, m_retries - 1)
                }
                int delay_time = Math.Min(DELAY_MILLISECONDES * (m_pow - 1) / 2,
                    MAX_DELAY_MILLISECONDS);
               
                delay = TimeSpan.FromMilliseconds(delay_time);
            }
            return Task.Delay(delay);

        }

    }
}
