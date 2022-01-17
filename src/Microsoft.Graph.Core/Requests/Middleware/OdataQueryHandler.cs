// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;
    using System;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A <see cref="DelegatingHandler"/> implementation that handles compression.
    /// </summary>
    public class OdataQueryHandler : DelegatingHandler
    {
        private readonly ODataQueryHandlerOption odataQueryHandlerOption;
        private readonly Regex odataQueryRegex = new Regex("(?i)([^$])(count|deltatoken|expand|filter|format|orderby|search|select|skip|skiptoken|top)=",RegexOptions.Compiled);

        /// <summary>
        /// The <see cref="OdataQueryHandler"/> constructor
        /// </summary>
        /// <param name="handlerOption">The <see cref="ODataQueryHandlerOption"/> to use</param>
        public OdataQueryHandler(ODataQueryHandlerOption handlerOption = null) 
        {
            this.odataQueryHandlerOption = handlerOption ?? new ODataQueryHandlerOption();
        }

        /// <summary>
        /// Sends a HTTP request.
        /// </summary>
        /// <param name="httpRequest">The <see cref="HttpRequestMessage"/> to be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
        {
            if (httpRequest == null)
                throw new ArgumentNullException(nameof(httpRequest));

            // Check if the request has a preconfigured option otherwise use the one provided in this instance
            var odataQueryOption = httpRequest.GetRequestOption<ODataQueryHandlerOption>() ?? odataQueryHandlerOption;
            // If the request is replacable, just do it
            if (odataQueryOption.ShouldReplace(httpRequest))
            {
                var queryString = string.Empty;
                if (httpRequest.RequestUri.Query != null && httpRequest.RequestUri.Query.Length > 1)
                    // We insert and remove the ? sign so we can make no dollar mandatory and avoid adding a second dollar when already here
                    queryString = odataQueryRegex.Replace("?" + httpRequest.RequestUri.Query, "$1$$$2=")[1..];

                // replace the uri with the new query options
                httpRequest.RequestUri = new UriBuilder(httpRequest.RequestUri) { Query = queryString }.Uri;
            }

            HttpResponseMessage response = await base.SendAsync(httpRequest, cancellationToken);

            return response;
        }
    }
}