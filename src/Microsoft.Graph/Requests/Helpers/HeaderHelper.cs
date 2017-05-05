// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;

    /// <summary>
    /// Helper class to add HTTP headers for requests.
    /// </summary>
    public static class HeaderHelper
    {
        /// <summary>
        /// Adds a header with the given name and value to the request.
        /// </summary>
        /// <typeparam name="TRequest">Type of the request.</typeparam>
        /// <param name="request">Request to which the header should be added.</param>
        /// <param name="name">Name of the header.</param>
        /// <param name="value">Value of the header.</param>
        /// <returns>Modified request with the added header.</returns>
        public static TRequest Header<TRequest>(this TRequest request, string name, string value) where TRequest : IBaseRequest
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!string.IsNullOrEmpty(value))
            {
                request.Headers.Add(new HeaderOption(name, value));
            }

            return request;
        }
    }
}
