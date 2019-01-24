// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Net.Http;

    internal static class HttpClientExtensions
    {
        internal static void SetFeatureFlags(this HttpClient httpClient, IEnumerable<string> flags)
        {
            httpClient.DefaultRequestHeaders.Add(CoreConstants.Headers.FeatureFlag, flags);
        }

        internal static IEnumerable<string> GetFeatureFlags(this HttpClient httpClient)
        {
            IEnumerable<string> flags = new List<string>();
            httpClient.DefaultRequestHeaders.TryGetValues(CoreConstants.Headers.FeatureFlag, out flags);

            return flags;
        }
    }
}
