// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{

    /// <summary>
    /// The interface IGraphServiceSitesCollectionRequestBuilder.
    /// </summary>
    public partial interface IGraphServiceSitesCollectionRequestBuilder
    {
        /// <summary>
        /// Gets a request builder for accessing a site by relative path.
        /// </summary>
        /// <returns>The <see cref="ISiteRequestBuilder"/>.</returns>
        ISiteRequestBuilder GetByPath(string siteRelativePath, string hostname);

        /// <summary>
        /// Gets a request builder for accessing a site's root. This is how we can provide a request builder for structural properties.
        /// </summary>
        ISiteRequestBuilder Root { get; }
    }
}
