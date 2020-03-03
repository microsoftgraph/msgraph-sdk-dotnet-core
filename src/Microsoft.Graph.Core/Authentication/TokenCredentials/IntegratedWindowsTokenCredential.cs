// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Azure.Core;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Microsoft.Identity.Client;
    using System;

    /// <summary>
    /// A <see cref="TokenCredential"/> implementation using MSAL.Net to acquire token by integrated windows authentication.
    /// </summary>
    public class IntegratedWindowsTokenCredential : TokenCredential
    {

        private static readonly List<string> WellKnownTenants = new List<string>
        {   AuthConstants.Tenants.Common,
            AuthConstants.Tenants.Consumers,
            AuthConstants.Tenants.Organizations
        };

        private readonly IPublicClientApplication _clientApplication;

        /// <summary>
        /// Creates a new IntegratedWindowsTokenCredential which will authenticate users with the specified application.
        /// </summary>
        /// <param name="publicClientApplication">A <see cref="IPublicClientApplication"/> to pass to <see cref="IntegratedWindowsTokenCredential"/> for authentication.</param>
        public IntegratedWindowsTokenCredential(IPublicClientApplication publicClientApplication)
        {
            _clientApplication = publicClientApplication ?? throw new AuthenticationException(
                new Error
                {
                    Code = ErrorConstants.Codes.InvalidRequest,
                    Message = string.Format(ErrorConstants.Messages.NullValue, nameof(publicClientApplication))
                });
        }

        /// <summary>
        /// Gets an <see cref="AccessToken"/> using the provided <see cref="TokenRequestContext"/> in a synchronous fashion
        /// </summary>
        /// <param name="requestContext">The <see cref="TokenRequestContext"/> for the request</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request</param>
        /// <returns>An <see cref="AccessToken"/> to make requests with</returns>
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return GetTokenAsync(requestContext,cancellationToken).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets an <see cref="AccessToken"/> using the provided <see cref="TokenRequestContext"/> in an asynchronous fashion
        /// </summary>
        /// <param name="requestContext">The <see cref="TokenRequestContext"/> for the request</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request</param>
        /// <returns>An <see cref="AccessToken"/> to make requests with</returns>
        public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            AuthenticationResult result;
            try
            {
                // Try to get the account from the cache
                IEnumerable<IAccount> accounts = await _clientApplication.GetAccountsAsync();
                IAccount account = accounts.FirstOrDefault();

                // Try to get the token silently
                AcquireTokenSilentParameterBuilder tokenSilentBuilder =
                    _clientApplication.AcquireTokenSilent(requestContext.Scopes, account);

                if (!ContainsWellKnownTenantName(_clientApplication.Authority))
                    tokenSilentBuilder.WithAuthority(_clientApplication.Authority);

                result = await tokenSilentBuilder.ExecuteAsync(cancellationToken);
            }
            catch (MsalException)
            {
                // We can't get the token silently so get a new one
                result = await GetNewAccessTokenAsync(requestContext);
            }

            return new AccessToken(result.AccessToken, result.ExpiresOn);
        }

        /// <summary>
        /// Gets an new <see cref="AccessToken"/> after checking the cache has failed
        /// </summary>
        /// <param name="requestContext">The <see cref="TokenRequestContext"/> for the request</param>
        /// <returns></returns>
        private async Task<AuthenticationResult> GetNewAccessTokenAsync(TokenRequestContext requestContext)
        {
            AuthenticationResult authenticationResult = null;
            int retryCount = 0;
            do
            {
                try
                {
                    authenticationResult = await _clientApplication.AcquireTokenByIntegratedWindowsAuth(requestContext.Scopes)
                                .ExecuteAsync();
                }
                catch (MsalServiceException serviceException)
                {
                    // Service not available so wait 
                    if (serviceException.ErrorCode == ErrorConstants.Codes.TemporarilyUnavailable)
                    {
                        TimeSpan delay = this.GetRetryAfter(serviceException);
                        retryCount++;
                        // pause execution
                        await Task.Delay(delay);
                    }
                    else
                    {
                        throw new AuthenticationException(
                            new Error
                            {
                                Code = ErrorConstants.Codes.GeneralException,
                                Message = ErrorConstants.Messages.UnexpectedMsalException
                            },
                            serviceException);
                    }
                }
                catch (Exception exception)
                {
                    throw new AuthenticationException(
                            new Error
                            {
                                Code = ErrorConstants.Codes.GeneralException,
                                Message = ErrorConstants.Messages.UnexpectedException
                            },
                            exception);
                }

            } while (retryCount < 3);

            return authenticationResult;
        }

        /// <summary>
        /// Determines if an authority url has a `well known` tenant name (common, consumers, organizations) in its tenant segment.
        /// </summary>
        /// <param name="currentAuthority">The authority url to check.</param>
        /// <returns>True is pre-defined tenant names are present, and false if not. </returns>
        private bool ContainsWellKnownTenantName(string currentAuthority)
        {
            if (!Uri.IsWellFormedUriString(currentAuthority, UriKind.Absolute))
                throw new ArgumentException("Invalid authority URI set.");

            Uri authorityUri = new Uri(currentAuthority);

            return WellKnownTenants.Contains(authorityUri.Segments[1].Replace(@"/", string.Empty));
        }
    }
}