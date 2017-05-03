// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;

    public static class HeaderHelper
    {
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

        public static TRequest ReturnRepresentation<TRequest>(this TRequest request) where TRequest : IBaseRequest
        {
            return request.Header("Prefer","return=representation");
        }
    }
}
