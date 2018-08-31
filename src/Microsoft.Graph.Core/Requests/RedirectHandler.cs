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
    using System.IO;

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

                var redirectCount = 0;

                while (redirectCount < maxRedirects)
                {
                    // general copy request with internal CopyRequest(see copyRequest for details) method 
                    var newRequest = await CopyRequest(response.RequestMessage);

                    // status code == 303: change request method from post to get and content to be null
                    if (response.StatusCode == HttpStatusCode.SeeOther)
                    {
                        newRequest.Content = null;
                        newRequest.Method = HttpMethod.Get;
                    }

                    // Set newRequestUri from response
                    newRequest.RequestUri = response.Headers.Location;
                    
                    // Remove Auth if http request's scheme or host changes
                    if (String.Compare(newRequest.RequestUri.Host, request.RequestUri.Host, StringComparison.OrdinalIgnoreCase) != 0 || 
                        !newRequest.RequestUri.Scheme.Equals(request.RequestUri.Scheme))
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
                    redirectCount++;
                }
                throw new ServiceException(
                        new Error
                        {
                            Code = ErrorConstants.Codes.TooManyRedirects,
                            Message = string.Format(ErrorConstants.Messages.TooManyRedirectsFormatString, redirectCount)
                        });

            }
            return response;

        }

        /// <summary>
        /// Create a new HTTP request by copying previous HTTP request's headers and properties from response's request message.
        /// </summary>
        /// <param name="originalRequest">The previous <see cref="HttpRequestMessage"/> needs to be copy.</param>
        /// <param name="content">The <see cref="StreamContent"/>may need to be passed to the new request.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        /// <remarks>
        /// Re-issue a new HTTP request with the previous request's headers and properities
        /// </remarks>
        internal async Task<HttpRequestMessage> CopyRequest(HttpRequestMessage originalRequest)
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

            // Set Content if previous request contains
            if (originalRequest.Content != null && originalRequest.Content.Headers.ContentLength != 0)
            {
                newRequest.Content = new StreamContent(await originalRequest.Content.ReadAsStreamAsync());
            }

            return newRequest;
        }


        /// <summary>
        /// Checks whether <see cref="HttpStatusCode"/> is redirected
        /// </summary>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <returns>Bool value for redirection or not</returns>
        private bool IsRedirect(HttpStatusCode statusCode)
        {
            if (statusCode == HttpStatusCode.MovedPermanently ||
                statusCode == HttpStatusCode.Found ||
                statusCode == HttpStatusCode.SeeOther ||
                statusCode == HttpStatusCode.TemporaryRedirect ||
                statusCode == (HttpStatusCode)308
                )
            {
                return true;
            }
            return false;
        }


    }

}
