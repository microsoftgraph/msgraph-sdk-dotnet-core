// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// The type ThumbnailSetRequestBuilder.
    /// </summary>
    public partial class ThumbnailSetRequestBuilder
    {
        /// <summary>
        /// Gets the thumbnail request builder for the specified thumbnail size.
        /// </summary>
        /// <param name="size">The thumbnail size.</param>
        /// <returns>The thumbnail request builder.</returns>
        public IThumbnailRequestBuilder this[string size]
        {
            get
            {
                return new ThumbnailRequestBuilder(
                    this.AppendSegmentToRequestUrl(size),
                    this.Client);
            }
        }
    }
}
