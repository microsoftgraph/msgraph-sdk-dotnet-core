// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------


namespace Microsoft.Graph
{
    using Microsoft.Identity.Client;
    using System;
    using System.Net.Http.Headers;
    using Azure.Core;

    internal static class TokenCredentialExtensions
    {
        /// <summary>
        /// Gets retry after timespan from <see cref="RetryConditionHeaderValue"/>.
        /// </summary>
        /// <param name="tokenCredential">n <see cref="TokenCredential"/> object.</param>
        /// <param name="serviceException">A <see cref="MsalServiceException"/> with RetryAfter header</param>
        internal static TimeSpan GetRetryAfter(this TokenCredential tokenCredential, MsalServiceException serviceException)
        {
            RetryConditionHeaderValue retryAfter = serviceException.Headers?.RetryAfter;
            TimeSpan? delay = null;

            if (retryAfter != null && retryAfter.Delta.HasValue)
            {
                delay = retryAfter.Delta;
            }
            else if (retryAfter != null && retryAfter.Date.HasValue)
            {
                delay = retryAfter.Date.Value.Offset;
            }

            if (delay == null)
                throw new MsalServiceException(serviceException.ErrorCode, ErrorConstants.Messages.MissingRetryAfterHeader);

            return delay.Value;
        }
    }
}