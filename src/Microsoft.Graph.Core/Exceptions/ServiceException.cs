// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;

    public class ServiceException : Exception
    {
        public ServiceException(Error error, Exception innerException = null)
            : base(null, innerException)
        {
            this.Error = error;
        }

        public Error Error { get; private set; }

        // ResponseHeaders and StatusCode exposed as pass-through.
        public System.Net.Http.Headers.HttpResponseHeaders ResponseHeaders { get; internal set; }

        public System.Net.HttpStatusCode StatusCode { get; internal set; } 

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

        public override string ToString()
        {
            if (this.Error != null)
            {
                return this.Error.ToString();
            }

            return null;
        }
    }
}
