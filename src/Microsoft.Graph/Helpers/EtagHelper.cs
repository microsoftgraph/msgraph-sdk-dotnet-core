// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;

    /// <summary>
    /// Helper class to extract @odata.etag property and to specify If-Match headers for requests.
    /// </summary>
    public static class EtagHelper
    {
        /// <summary>
        /// Returns the etag of an entity.
        /// </summary>
        /// <param name="entity">The entity that contains an etag.</param>
        /// <returns>Etag value if present, null otherwise.</returns>
        public static string GetEtag(this Entity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            entity.AdditionalData.TryGetValue("@odata.etag", out object etag);
            return etag as string;
        }

        /// <summary>
        /// Adds If-Match header to a request with the given etag.
        /// </summary>
        /// <typeparam name="TRequest">Type of the request.</typeparam>
        /// <param name="request">The request.</param>
        /// <param name="etag">The etag value.</param>
        /// <returns>The request with the If-Match header.</returns>
        public static TRequest IfMatch<TRequest>(this TRequest request, string etag) where TRequest : IBaseRequest
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!string.IsNullOrEmpty(etag))
            {
                request.Headers.Add(new HeaderOption("If-Match", etag));
            }

            return request;
        }
    }
}
