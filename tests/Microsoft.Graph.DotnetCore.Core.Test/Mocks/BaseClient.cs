// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Mocks
{
    using Microsoft.Graph.Core.Requests;
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Authentication;

    /// <summary>
    /// A default client implementation.
    /// </summary>
    internal class BaseClient : IBaseClient
    {
        /// <summary>
        /// Constructs a new <see cref="BaseClient"/>.
        /// </summary>
        /// <param name="requestAdapter">The custom <see cref="IRequestAdapter"/> to be used for making requests</param>
        internal BaseClient(IRequestAdapter requestAdapter)
        {
            this.RequestAdapter = requestAdapter;
        }

        /// <summary>
        /// Constructs a new <see cref="BaseClient"/>.
        /// </summary>
        /// <param name="baseUrl">The base service URL. For example, "https://graph.microsoft.com/v1.0."</param>
        /// <param name="authenticationProvider">The <see cref="IAuthenticationProvider"/> for authenticating request messages.</param>
        internal BaseClient(
            string baseUrl,
            IAuthenticationProvider authenticationProvider
            ) : this(new BaseGraphRequestAdapter(authenticationProvider) { BaseUrl = baseUrl })
        {
        }

        /// <summary>
        /// Gets the <see cref="IRequestAdapter"/> for sending requests.
        /// </summary>
        public IRequestAdapter RequestAdapter
        {
            get; set;
        }

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
