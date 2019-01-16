// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Net;
    using System.Net.Http;

    /// <summary>
    /// The retry middleware option class
    /// </summary>
    public class RetryOption : IMiddlewareOption
    {
        /// <summary>
        /// Constructs a new <see cref="RetryOption"/>
        /// </summary>
        public RetryOption()
        {
            ShouldRetry = (response) => IsRetry(response);
        }

        /// <summary>
        /// MaxRetry property
        /// </summary>
        public int MaxRetry { set; get; } = 10;

        /// <summary>
        /// A ShouldRetry delegate
        /// </summary>
        public Func<HttpResponseMessage, bool> ShouldRetry { get; set; }

        /// <summary>
        /// Check the HTTP response's status to determine whether it should be retried or not.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>returned.</param>
        /// <returns></returns>
        private bool IsRetry(HttpResponseMessage response)
        {
            if ((response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == (HttpStatusCode)429))
            {
                return true;
            }
            return false;
        }
    }
}
