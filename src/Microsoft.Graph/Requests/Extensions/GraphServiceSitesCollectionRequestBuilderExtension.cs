// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// The type GraphServiceSitesCollectionRequestBuilder.
    /// </summary>
    public partial class GraphServiceSitesCollectionRequestBuilder : BaseRequestBuilder, IGraphServiceSitesCollectionRequestBuilder
    {
        /// <summary>
        /// Gets a request builder for accessing a site by relative path.
        /// </summary>
        /// <returns>The <see cref="ISiteRequestBuilder"/>.</returns>
        public ISiteRequestBuilder GetByPath(string siteRelativePath, string hostname)
        {
            if (!string.IsNullOrEmpty(siteRelativePath))
            {
                if (!siteRelativePath.StartsWith("/"))
                {
                    siteRelativePath = string.Format("/{0}:", siteRelativePath);
                }
            }

            return new SiteRequestBuilder(
                string.Format("{0}/{1}:{2}", this.RequestUrl, hostname, siteRelativePath),
                this.Client);
        }
    }
}
