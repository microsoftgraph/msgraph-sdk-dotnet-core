// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// The auth middleware option class
    /// </summary>
    public class AuthenticationHandlerOption : IMiddlewareOption
    {
        /// <summary>
        /// An Authentication Provider
        /// </summary>
        internal IAuthenticationProvider AuthenticationProvider { get; set; }

        /// <summary>
        /// An auth provider option property
        /// </summary>
        public IAuthProviderOption AuthProviderOption { get; set; }
    }
}
