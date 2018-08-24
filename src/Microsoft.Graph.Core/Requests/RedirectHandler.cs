// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Net;

    /// <summary>
    /// An <see cref="DelegatingHandler"/> implementation using standard .NET libraries.
    /// </summary>
    public class RedirectHandler : DelegatingHandler
    {

        private const int maxRedirects = 5;

        /// <summary>
        /// Constructs a new <see cref="RedirectHandler"/> 
        /// </summary>
        /// <param name="innerHandler">An HTTP message handler to pass to the <see cref="HttpMessageHandler"/> for sending requests.</param>
        public RedirectHandler(HttpMessageHandler innerHandler)
        {
            InnerHandler = innerHandler;
        }

        /// <summary>
        /// Sends the Request 
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>for the request.</param>
        /// <returns>The <see cref="HttpResponseMessage"/>.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            
            // send request first time to get response
            var response = await base.SendAsync(request, cancellationToken);

            // check response status code 
            if (IsRedirect(response.StatusCode))
            {

                // general copy request with internal copyRequest(see copyRequest for details) Method 
                var newRequest = CopyRequest(request);
                StreamContent content = null;

                if (request.Content != null && request.Content.Headers.ContentLength != 0)
                {
                    content = new StreamContent(request.Content.ReadAsStreamAsync().Result);
                }

                var redirectCount = 0;

                // check whether redirect count over maxRedirects
                while (redirectCount++ < maxRedirects)
                {
                   
                    // status code == 303: change request method from post to get and content to be null
                    if (response.StatusCode == HttpStatusCode.SeeOther)
                    {
                        newRequest.Content = null;
                        newRequest.Method = HttpMethod.Get;
                    }
                    else
                    {
                        newRequest.Content = content;
                        newRequest.Method = request.Method;
                    }

                    // Set newRequestUri from response
                    newRequest.RequestUri = response.Headers.Location;

                    // Remove Auth if unneccessary
                    if (String.Compare(newRequest.RequestUri.Host, request.RequestUri.Host, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        newRequest.Headers.Authorization = null;

                    }

                    // Send redirect request to get reponse      
                    response = await base.SendAsync(newRequest, cancellationToken);

                    // Check response status code
                    if (!IsRedirect(response.StatusCode))
                    {
                        return response;
                    }
                }
                throw new ServiceException(
                        new Error
                        {
                            Code = ErrorConstants.Codes.TooManyRedirects,
                            Message = string.Format(ErrorConstants.Messages.TooManyRedirectsFormatString, maxRedirects)
                        });

            }
            return response;

        }

        /// <summary>
        /// Copy original HTTP request's headers and porperties.
        /// </summary>
        /// <param name="originalRequest">The original <see cref="HttpRequestMessage"/> needs to be copy.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        /// <remarks>
        /// Re-issue a new HTTP request with the original request's headers and properities
        /// </remarks>
        internal HttpRequestMessage CopyRequest(HttpRequestMessage originalRequest)
        {
            var newRequest = new HttpRequestMessage(originalRequest.Method, originalRequest.RequestUri);

            foreach (var header in originalRequest.Headers)
            {
                newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            foreach (var property in originalRequest.Properties)
            {
                newRequest.Properties.Add(property);
            }

            return newRequest;
        }


        /// <summary>
        /// Checks whether <see cref="HttpStatusCode"/> needs redirected
        /// </summary>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <returns>Bool value for redirection or not</returns>
        private bool IsRedirect(HttpStatusCode statusCode)
        {
            return (int)statusCode >= 300 && (int)statusCode < 400 && statusCode != HttpStatusCode.NotModified && statusCode != HttpStatusCode.UseProxy;
        }


    }

}
