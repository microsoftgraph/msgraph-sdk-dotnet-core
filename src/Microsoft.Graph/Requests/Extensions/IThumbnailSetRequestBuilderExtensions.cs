// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    public partial interface IThumbnailSetRequestBuilder
    {
        /// <summary>
        /// Gets the request builder for the specified thumbnail size.
        /// </summary>
        /// <param name="size">The thumbnail size.</param>
        /// <returns>The thumbnail request builder.</returns>
        IThumbnailRequestBuilder this[string size] { get; }
    }
}
