// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// The type ISiteRequestBuilder.
    /// </summary>
    public partial interface ISiteRequestBuilder
    {
        /// <summary>
        /// Gets a site request for the requested path.
        /// <returns>The site request.</returns>
        /// </summary>
        ISiteRequestBuilder SiteWithPath(string path);
    }
}
