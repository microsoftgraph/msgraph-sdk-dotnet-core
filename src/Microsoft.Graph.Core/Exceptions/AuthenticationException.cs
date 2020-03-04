// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;

    /// <summary>
    /// Generic exception class to report unknown exceptions during authentication
    /// </summary>
    public class AuthenticationException : Exception
    {
        /// <summary>
        /// Creates a new authentication exception.
        /// </summary>
        /// <param name="error">The error that triggered the exception.</param>
        /// <param name="innerException">The possible inner exception.</param>
        public AuthenticationException(Error error, Exception innerException = null)
            : base(error?.ToString(), innerException)
        {
            this.Error = error;
        }

        /// <summary>
        /// The error from the authentication exception.
        /// </summary>
        public Error Error { get; private set; }
    }
}