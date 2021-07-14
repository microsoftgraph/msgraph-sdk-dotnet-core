// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Azure.Core;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Linq;
    using System;

    /// <summary>
    /// An AuthProvider to handle instances of <see cref="TokenCredential"/> from Azure.Core and Azure.Identity
    /// </summary>
    public class TokenCredentialAuthProvider : IAuthenticationProvider
    {
        private readonly TokenCredential _credential;

        private readonly IEnumerable<string> _scopes;

        /// <summary>
        /// An AuthProvider to handle instances of <see cref="TokenCredential"/> from Azure.Core and Azure.Identity
        /// </summary>
        /// <param name="tokenCredential">The <see cref="TokenCredential"/> to use for authentication</param>
        /// <param name="scopes">Scopes required to access Microsoft Graph. This defaults to https://graph.microsoft.com/.default when none is set.</param>
        /// <exception cref="ArgumentException"> When a null <see cref="TokenCredential"/> is passed</exception>
        public TokenCredentialAuthProvider(TokenCredential tokenCredential, IEnumerable<string> scopes = null)
        {
            _credential = tokenCredential ?? throw new ArgumentException(
                              string.Format(ErrorConstants.Messages.NullParameter, nameof(tokenCredential)),
                              nameof(tokenCredential));
            _scopes = scopes ?? new List<string> { AuthConstants.DefaultScopeUrl };
        }

        /// <summary>
        /// Adds an authentication header to the incoming request by checking using the <see cref="TokenCredential"/> provided
        /// during the creation of this class
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to authenticate</param>
        /// <returns></returns>
        public async Task AuthenticateRequestAsync(HttpRequestMessage request)
        {
            //First try to read the scopes off the requestContext.
            ScopedAuthenticationProviderOptions scopedAuthenticationProviderOptions = request.GetScopedAuthenticationProviderOption();
            AccessToken token = await _credential.GetTokenAsync(new TokenRequestContext(scopedAuthenticationProviderOptions.Scopes ?? _scopes.ToArray()), CancellationToken.None).ConfigureAwait(false);
            request.Headers.Authorization = new AuthenticationHeaderValue(CoreConstants.Headers.Bearer, token.Token);
        }
    }
}
