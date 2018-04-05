// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;

    /// <summary>
    /// Graph service exception.
    /// </summary>
    public class ServiceException : Exception
    {
        /// <summary>
        /// Creates a new service exception.
        /// </summary>
        /// <param name="error">The error that triggered the exception.</param>
        /// <param name="innerException">The possible innerException.</param>
        public ServiceException(Error error, Exception innerException = null)
            : base(error?.ToString(), innerException)
        {
            this.Error = error;
        }

        /// <summary>
        /// The error from the service exception.
        /// </summary>
        public Error Error { get; private set; }

        /// ResponseHeaders and StatusCode exposed as pass-through.
        public System.Net.Http.Headers.HttpResponseHeaders ResponseHeaders { get; internal set; }

        /// <summary>
        /// The HTTP status code from the response.
        /// </summary>
        public System.Net.HttpStatusCode StatusCode { get; internal set; }

        /// <summary>
        /// Checks if a given error code has been returned in the response at any level in the error stack.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <returns>True if the error code is in the stack.</returns>
        public bool IsMatch(string errorCode)
        {
            if (string.IsNullOrEmpty(errorCode))
            {
                throw new ArgumentException("errorCode cannot be null or empty", "errorCode");
            }

            var currentError = this.Error;

            while (currentError != null)
            {
                if (string.Equals(currentError.Code, errorCode, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                currentError = currentError.InnerError;
            }

            return false;
        }
    }
}