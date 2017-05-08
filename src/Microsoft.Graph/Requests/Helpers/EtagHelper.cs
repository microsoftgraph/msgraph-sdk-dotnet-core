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
        /// Name of the OData etag property.
        /// </summary>
        public const string ODataEtagPropertyName = "@odata.etag";

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

            entity.AdditionalData.TryGetValue(ODataEtagPropertyName, out object etag);
            return etag as string;
        }
    }
}
