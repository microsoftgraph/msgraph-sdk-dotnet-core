// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// The type  ItemRequestBuilder.
    /// </summary>
    public partial interface IDriveItemRequestBuilder
    {
        /// <summary>
        /// Gets item request builder for the specified item path.
        /// <returns>The item request builder.</returns>
        /// </summary>
        IDriveItemRequestBuilder ItemWithPath(string path);
    }
}
