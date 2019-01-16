// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// The auth middleware option class
    /// </summary>
    public class AuthOption : IMiddlewareOption
    {
        /// <summary>
        /// Constructs a new <see cref="AuthOption"/>
        /// </summary>
        public AuthOption()
        {

        }
        /// <summary>
        /// A Scopes property
        /// </summary>
        public string[] Scopes { get; set; }

        /// <summary>
        /// A ForceRefresh property
        /// </summary>
        public bool ForceRefresh { get; set; }
    }
}
