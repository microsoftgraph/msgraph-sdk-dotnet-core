// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// The interface IUploadChunkRequest.
    /// </summary>
    public partial interface IUploadChunkRequest : IBaseRequest
    {
        /// <summary>
        /// Puts the specified Chunk.
        /// </summary>
        /// <returns>The task to await.</returns>
        Task<UploadChunkResult> PutAsync(Stream stream);
    }
}