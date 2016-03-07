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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The base request class.
    /// </summary>
    public class BaseRequest : IBaseRequest
    {
        private readonly string sdkVersionHeaderValue;

        /// <summary>
        /// Constructs a new <see cref="BaseRequest"/>.
        /// </summary>
        /// <param name="requestUrl">The URL for the request.</param>
        /// <param name="client">The <see cref="IBaseClient"/> for handling requests.</param>
        /// <param name="options">The header and query options for the request.</param>
        public BaseRequest(
            string requestUrl,
            IBaseClient client,
            IEnumerable<Option> options = null)
        {
            this.Method = "GET";
            this.Client = client;
            this.Headers = new List<HeaderOption>();
            this.QueryOptions = new List<QueryOption>();

            this.RequestUrl = this.InitializeUrl(requestUrl);

            if (options != null)
            {
                var headerOptions = options.OfType<HeaderOption>();
                if (headerOptions != null)
                {
                    ((List<HeaderOption>)this.Headers).AddRange(headerOptions);
                }

                var queryOptions = options.OfType<QueryOption>();
                if (queryOptions != null)
                {
                    ((List<QueryOption>)this.QueryOptions).AddRange(queryOptions);
                }
            }

            this.sdkVersionHeaderValue = string.Format(
                Constants.Headers.SdkVersionHeaderValue,
                this.GetType().GetTypeInfo().Assembly.GetName().Version);
        }

        /// <summary>
        /// Gets or sets the content type for the request.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Gets the <see cref="HeaderOption"/> collection for the request.
        /// </summary>
        public IList<HeaderOption> Headers { get; private set; }

        /// <summary>
        /// Gets the <see cref="IGraphServiceClient"/> for handling requests.
        /// </summary>
        public IBaseClient Client { get; private set; }

        /// <summary>
        /// Gets or sets the HTTP method string for the request.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets the URL for the request, without query string.
        /// </summary>
        public string RequestUrl { get; internal set; }

        /// <summary>
        /// Gets the <see cref="QueryOption"/> collection for the request.
        /// </summary>
        public IList<QueryOption> QueryOptions { get; set; }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="serializableObject">The serializable object to send.</param>
        /// <param name="completionOption">The <see cref="HttpCompletionOption"/> to pass to the <see cref="IHttpProvider"/> on send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The task to await.</returns>
        public async Task SendAsync(
            object serializableObject,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            using (var response = await this.SendRequestAsync(serializableObject, completionOption, cancellationToken).ConfigureAwait(false))
            {
            }
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <typeparam name="T">The expected response object type for deserialization.</typeparam>
        /// <param name="serializableObject">The serializable object to send.</param>
        /// <param name="completionOption">The <see cref="HttpCompletionOption"/> to pass to the <see cref="IHttpProvider"/> on send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The deserialized response object.</returns>
        public async Task<T> SendAsync<T>(
            object serializableObject,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            using (var response = await this.SendRequestAsync(serializableObject, completionOption, cancellationToken).ConfigureAwait(false))
            {
                if (response.Content != null)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    return this.Client.HttpProvider.Serializer.DeserializeObject<T>(responseString);
                }

                return default(T);
            }
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <typeparam name="T">The expected response object type for deserialization.</typeparam>
        /// <param name="serializableObject">The serializable object to send.</param>
        /// <param name="completionOption">The <see cref="HttpCompletionOption"/> to pass to the <see cref="IHttpProvider"/> on send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The stream.</returns>
        public async Task<Stream> SendStreamRequestAsync(
            object serializableObject,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            var response = await this.SendRequestAsync(serializableObject, completionOption, cancellationToken).ConfigureAwait(false);
            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <typeparam name="T">The expected response object type for deserialization.</typeparam>
        /// <param name="serializableObject">The serializable object to send.</param>
        /// <param name="completionOption">The <see cref="HttpCompletionOption"/> to pass to the <see cref="IHttpProvider"/> on send.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns>The <see cref="HttpResponseMessage"/> object.</returns>
        public async Task<HttpResponseMessage> SendRequestAsync(
            object serializableObject,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(this.RequestUrl))
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = GraphErrorCode.InvalidRequest.ToString(),
                        Message = "Request URL is required to send a request.",
                    });
            }

            if (this.Client.AuthenticationProvider == null)
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = GraphErrorCode.InvalidRequest.ToString(),
                        Message = "Authentication provider is required before sending a request.",
                    });
            }

            using (var request = this.GetHttpRequestMessage())
            {
                await this.AuthenticateRequest(request).ConfigureAwait(false);

                if (serializableObject != null)
                {
                    var inputStream = serializableObject as Stream;

                    if (inputStream != null)
                    {
                        request.Content = new StreamContent(inputStream);
                    }
                    else
                    {
                        request.Content = new StringContent(this.Client.HttpProvider.Serializer.SerializeObject(serializableObject));
                    }

                    if (!string.IsNullOrEmpty(this.ContentType))
                    {
                        request.Content.Headers.ContentType = new MediaTypeHeaderValue(this.ContentType);
                    }
                }

                return await this.Client.HttpProvider.SendAsync(request).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets the <see cref="HttpRequestMessage"/> representation of the request.
        /// </summary>
        /// <returns>The <see cref="HttpRequestMessage"/> representation of the request.</returns>
        public HttpRequestMessage GetHttpRequestMessage()
        {
            var queryString = this.BuildQueryString();
            var request = new HttpRequestMessage(new HttpMethod(this.Method), this.RequestUrl + queryString);

            this.AddHeadersToRequest(request);

            return request;
        }

        /// <summary>
        /// Builds the query string for the request from the query option collection.
        /// </summary>
        /// <returns>The constructed query string.</returns>
        internal string BuildQueryString()
        {
            if (this.QueryOptions != null)
            {
                var stringBuilder = new StringBuilder();

                foreach (var queryOption in this.QueryOptions)
                {
                    if (stringBuilder.Length == 0)
                    {
                        stringBuilder.AppendFormat("?{0}={1}", queryOption.Name, queryOption.Value);
                    }
                    else
                    {
                        stringBuilder.AppendFormat("&{0}={1}", queryOption.Name, queryOption.Value);
                    }
                }

                return stringBuilder.ToString();
            }

            return null;
        }

        /// <summary>
        /// Adds all of the headers from the header collection to the request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> representation of the request.</param>
        private void AddHeadersToRequest(HttpRequestMessage request)
        {
            if (this.Headers != null)
            {
                foreach (var header in this.Headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Name, header.Value);
                }
            }

            // Append SDK version header for telemetry
            request.Headers.Add(
                Constants.Headers.SdkVersionHeaderName,
                this.sdkVersionHeaderValue);
        }

        /// <summary>
        /// Adds the authentication header to the request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> representation of the request.</param>
        /// <returns>The task to await.</returns>
        private Task AuthenticateRequest(HttpRequestMessage request)
        {
            return this.Client.AuthenticationProvider.AuthenticateRequestAsync(request);
        }

        /// <summary>
        /// Initializes the request URL for the request, breaking it into query options and base URL.
        /// </summary>
        /// <param name="requestUrl">The request URL.</param>
        /// <returns>The request URL minus query string.</returns>
        private string InitializeUrl(string requestUrl)
        {
            if (string.IsNullOrEmpty(requestUrl))
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = GraphErrorCode.InvalidRequest.ToString(),
                        Message = "Base URL is not initialized for the request.",
                    });
            }

            var uri = new Uri(requestUrl);
            
            if (!string.IsNullOrEmpty(uri.Query))
            {
                var queryString = uri.Query;
                if (queryString[0] == '?')
                {
                    queryString = queryString.Substring(1);
                }

                var queryOptions = queryString.Split('&').Select(
                        queryValue =>
                        {
                            var segments = queryValue.Split('=');
                            return new QueryOption(
                                segments[0],
                                segments.Length > 1 ? segments[1] : string.Empty);
                        });

                foreach(var queryOption in queryOptions)
                {
                    this.QueryOptions.Add(queryOption);
                }
            }

            return new UriBuilder(uri) { Query = string.Empty }.ToString();
        }
    }
}
