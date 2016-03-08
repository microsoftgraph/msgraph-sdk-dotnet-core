// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    public partial class DriveSpecialCollectionRequestBuilder
    {
        /// <summary>
        /// Gets app root special folder item request builder.
        /// </summary>
        public IDriveItemRequestBuilder AppRoot
        {
            get { return new DriveItemRequestBuilder(this.AppendSegmentToRequestUrl(Constants.Url.AppRoot), this.Client); }
        }
    }
}
