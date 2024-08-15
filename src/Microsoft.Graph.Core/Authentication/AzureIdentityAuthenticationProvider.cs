// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;
using Azure.Core;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Microsoft.Graph.Authentication;
/// <summary>
/// An overload of the Azure Identity Authentication Provider that has the defaults for Microsoft Graph.
/// </summary>
public class AzureIdentityAuthenticationProvider : BaseBearerTokenAuthenticationProvider
{
    /// <summary>
    /// The <see cref="AzureIdentityAuthenticationProvider"/> constructor
    /// </summary>
    /// <param name="credential">The credential implementation to use to obtain the access token.</param>
    /// <param name="allowedHosts">The list of allowed hosts for which to request access tokens.</param>
    /// <param name="scopes">The scopes to request the access token for.</param>
    /// <param name="observabilityOptions">The observability options to use for the authentication provider.</param>
    /// <param name="isCaeEnabled">Determines if the Continuous Access Evaluation (CAE) is enabled.</param>
    public AzureIdentityAuthenticationProvider(TokenCredential credential, string[] allowedHosts = null, Microsoft.Kiota.Authentication.Azure.ObservabilityOptions observabilityOptions = null, bool isCaeEnabled = true, params string[] scopes) : base(new AzureIdentityAccessTokenProvider(credential, allowedHosts, observabilityOptions, isCaeEnabled, scopes))
    {
	}
    /// <summary>
    /// The <see cref="AzureIdentityAuthenticationProvider"/> constructor
    /// </summary>
    /// <param name="credential">The credential implementation to use to obtain the access token.</param>
    /// <param name="allowedHosts">The list of allowed hosts for which to request access tokens.</param>
    /// <param name="scopes">The scopes to request the access token for.</param>
    /// <param name="observabilityOptions">The observability options to use for the authentication provider.</param>
    [Obsolete("Use the constructor that takes an isCaeEnabled parameter instead.")]
    public AzureIdentityAuthenticationProvider(TokenCredential credential, string[] allowedHosts = null, Microsoft.Kiota.Authentication.Azure.ObservabilityOptions observabilityOptions = null, params string[] scopes) : this(credential, allowedHosts, observabilityOptions, true, scopes)
    {
	}
}