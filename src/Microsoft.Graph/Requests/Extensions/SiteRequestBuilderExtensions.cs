// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// The type SiteRequestBuilder.
    /// </summary>
    public partial class SiteRequestBuilder
    {
        /// <summary>
        /// Gets a site request for the requested path.
        /// <returns>The site request.</returns>
        /// </summary>
        public ISiteRequestBuilder SiteWithPath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (!path.StartsWith("/"))
                {
                    path = string.Format("/{0}", path);
                }
            }

            return new SiteRequestBuilder(
                string.Format("{0}:{1}:", this.RequestUrl, path),
                this.Client);
        }
    }
}
