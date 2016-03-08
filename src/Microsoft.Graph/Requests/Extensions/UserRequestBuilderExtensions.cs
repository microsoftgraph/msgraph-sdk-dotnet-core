// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    public partial class UserRequestBuilder
    {

        /// <summary>
        /// Gets drive item request builder for the specified drive item path.
        /// <returns>The drive item request builder.</returns>
        /// </summary>
        public IDriveItemRequestBuilder ItemWithPath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                path = path.TrimStart('/');
            }

            return new DriveItemRequestBuilder(
                string.Format("{0}/{1}:", this.RequestUrl, path),
                this.Client);
        }
    }
}
