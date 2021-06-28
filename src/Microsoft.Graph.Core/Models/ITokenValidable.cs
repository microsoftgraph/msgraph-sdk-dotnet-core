// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;

    /// <summary>
    /// The ITokenValidable interface
    /// </summary>
    public interface ITokenValidable
    {
        /// <summary>
        /// The collection of validation tokens
        /// </summary>
        IEnumerable<string> ValidationTokens { get; set; }

        /// <summary>
        /// The collection of encrypted token bearers
        /// </summary>
        IEnumerable<IEncryptedContentBearer> Value { get; set; }
    }
}