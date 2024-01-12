// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;

    /// <summary>
    /// The ITokenValidable interface
    /// </summary>
    public interface ITokenValidable<T1,T2> where T1 : IEncryptedContentBearer<T2> where T2 : IDecryptableContent
    {
        /// <summary>
        /// The collection of validation tokens
        /// </summary>
        List<string> ValidationTokens { get; set; }

        /// <summary>
        /// The collection of encrypted token bearers
        /// </summary>
        List<T1> Value { get; set; }
    }
}