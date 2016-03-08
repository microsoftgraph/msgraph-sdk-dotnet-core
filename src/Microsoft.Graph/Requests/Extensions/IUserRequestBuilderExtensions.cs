// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    public partial interface IUserRequestBuilder
    {
        /// <summary>
        /// Gets drive item request builder for the specified drive item path.
        /// <returns>The drive item request builder.</returns>
        /// </summary>
        IDriveItemRequestBuilder ItemWithPath(string path);
    }
}
