// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;

    /// <summary>
    /// Graph client exception.
    /// </summary>
    public class ClientException : Exception
    {
        /// <summary>
        /// Creates a new client exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The possible innerException.</param>
        public ClientException(string message, Exception innerException = null) : base(message, innerException)
        {
        }
    }
}
