// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// The redirect middleware option class
    /// </summary>
    public class RedirectOption : IMiddlewareOption
    {
        /// <summary>
        /// Constructs a new <see cref="RedirectOption"/>
        /// </summary>
        public RedirectOption()
        {

        }
        /// <summary>
        /// A MaxRedirects property
        /// </summary>
        public int MaxRedirects { get; set; } = 5;
    }
}
