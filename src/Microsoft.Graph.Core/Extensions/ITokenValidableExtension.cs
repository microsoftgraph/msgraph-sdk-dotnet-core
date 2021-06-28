// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.IdentityModel.Protocols;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.IdentityModel.Tokens.Jwt;

    /// <summary>
    /// Contains extension methods for <see cref="ITokenValidableExtension"/>
    /// </summary>
    public static class ITokenValidableExtension
    {
        /// <summary>
        /// Validates tokens attached with the notification collection. If the result is false, the notification collection should be discarded.
        /// </summary>
        /// <param name="collection">Collection instance of <see cref="ITokenValidable"/></param>
        /// <param name="tenantIds">List of tenant ids that notifications might be originating from.</param>
        /// <param name="appIds">List of application id (clientid) that subscriptions have been created from.</param>
        /// <param name="wellKnownUri">Well known URL to get the signing certificates for the tokens. If you are not using the public cloud you need to pass the value corresponding to your national deployment.</param>
        /// <param name="issuerPrefix">Issuer prefix for the "aud" claim in the tokens. If you are not using the public cloud you need to pass the value corresponding to your national deployment.</param>
        /// <returns>Are tokens valid or not.</returns>
        public static async Task<bool> AreTokensValid(this ITokenValidable collection, IEnumerable<Guid> tenantIds, IEnumerable<Guid> appIds,
            string wellKnownUri = "https://login.microsoftonline.com/common/.well-known/openid-configuration",
            string issuerPrefix = "https://sts.windows.net/")
        {
            if ((collection.ValidationTokens == null || !collection.ValidationTokens.Any()) && collection.Value.All(x => x.EncryptedContent == null))
                return true;
            
            if (tenantIds == null || !tenantIds.Any())
                throw new ArgumentNullException(nameof(tenantIds));
            if (appIds == null || !appIds.Any())
                throw new ArgumentNullException(nameof(appIds));

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(wellKnownUri, new OpenIdConnectConfigurationRetriever());
            var openIdConfig = await configurationManager.GetConfigurationAsync();
            var handler = new JwtSecurityTokenHandler();
            var issuersToValidate = tenantIds.Select(x => $"{issuerPrefix}{x}/");
            var appIdsToValidate = appIds.Select(x => x.ToString());
            return collection.ValidationTokens.Select(x => IsTokenValid(x, handler, openIdConfig, issuersToValidate, appIdsToValidate))
                        .Aggregate((z, y) => z && y);
        }

        private static bool IsTokenValid(string token, JwtSecurityTokenHandler handler, OpenIdConnectConfiguration openIdConfig, IEnumerable<string> issuersToValidate, IEnumerable<string> appIds)
        {
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuers = issuersToValidate,
                ValidAudiences = appIds,
                IssuerSigningKeys = openIdConfig.SigningKeys
            }, out _);
            return true;
        }
    }
}
