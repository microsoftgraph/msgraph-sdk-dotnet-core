// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// The type  DriveItemRequestBuilder.
    /// </summary>
    public partial class DriveItemRequestBuilder
    {
        /// <summary>
        /// Gets children request.
        /// <returns>The children request.</returns>
        /// </summary>
        public IDriveItemRequestBuilder ItemWithPath(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (!path.StartsWith("/"))
                {
                    path = string.Format("/{0}", path);
                }
            }

            return new DriveItemRequestBuilder(
                string.Format("{0}:{1}:", this.RequestUrl, path),
                this.Client);
        }
    }
}
