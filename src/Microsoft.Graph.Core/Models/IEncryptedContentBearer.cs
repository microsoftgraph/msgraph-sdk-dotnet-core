// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// The IEncryptedContentBearer interface
    /// </summary>
    public interface IEncryptedContentBearer<T> where T: IDecryptableContent
    {
        /// <summary>
        /// The encrypted content
        /// </summary>
        T EncryptedContent { get; set; }
    }
}
