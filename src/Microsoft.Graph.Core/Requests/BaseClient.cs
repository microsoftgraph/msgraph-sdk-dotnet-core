// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Net.Http;
    using Microsoft.Graph.Core.Requests;
    using System.Collections.Generic;
    using Azure.Core;
    using Microsoft.Kiota.Abstractions.Authentication;
    using Microsoft.Kiota.Authentication.Azure;
    using System.Linq;
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Http.HttpClientLibrary;

    /// <summary>
    /// A default client implementation.
    /// </summary>
    public class BaseClient
    {
        /// <summary>
        /// Constructs a new <see cref="BaseClient"/> for use by derived instances.
        /// </summary>
        protected BaseClient()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="BaseClient"/>.
        /// </summary>
        /// <param name="requestAdapter">The custom <see cref="IRequestAdapter"/> to be used for making requests</param>
        public BaseClient(
            IRequestAdapter requestAdapter)
        {
            this.RequestAdapter = requestAdapter;
        }

        /// <summary>
        /// Constructs a new <see cref="BaseClient"/>.
        /// </summary>
        /// <param name="baseUrl">The base service URL. For example, "https://graph.microsoft.com/v1.0."</param>
        /// <param name="authenticationProvider">The <see cref="IAuthenticationProvider"/> for authenticating request messages.</param>
        public BaseClient(
            string baseUrl,
            IAuthenticationProvider authenticationProvider
            ): this(new HttpClientRequestAdapter(authenticationProvider , httpClient: GraphClientFactory.Create()){ BaseUrl = baseUrl })
        {
        }

        /// <summary>
        /// Constructs a new <see cref="BaseClient"/>.
        /// </summary>
        /// <param name="baseUrl">The base service URL. For example, "https://graph.microsoft.com/v1.0."</param>
        /// <param name="tokenCredential">The <see cref="TokenCredential"/> for authenticating request messages.</param>
        /// <param name="scopes">List of scopes for the authentication context.</param>
        public BaseClient(
            string baseUrl,
            TokenCredential tokenCredential,
            IEnumerable<string> scopes = null
            ):this(baseUrl, new AzureIdentityAuthenticationProvider(tokenCredential, scopes?.ToArray() ?? Array.Empty<string>()))
        {
        }

        /// <summary>
        /// Constructs a new <see cref="BaseClient"/>.
        /// </summary>
        /// <param name="baseUrl">The base service URL. For example, "https://graph.microsoft.com/v1.0."</param>
        /// <param name="httpClient">The customized <see cref="HttpClient"/> to be used for making requests</param>
        public BaseClient(
            string baseUrl,
            HttpClient httpClient):this(new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient) { BaseUrl = baseUrl })
        {
        }

        /// <summary>
        /// Gets the <see cref="IRequestAdapter"/> for sending requests.
        /// </summary>
        protected internal IRequestAdapter RequestAdapter { get; set; }

        /// <summary>
        /// Gets the <see cref="BatchRequestBuilder"/> for building batch Requests
        /// </summary>
        public BatchRequestBuilder Batch
        {
            get
            {
                return new BatchRequestBuilder(this.RequestAdapter);
            }
        }
    }
}
