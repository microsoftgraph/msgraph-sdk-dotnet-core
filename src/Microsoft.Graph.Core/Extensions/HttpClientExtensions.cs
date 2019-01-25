// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Net.Http;

    internal static class HttpClientExtensions
    {
        internal static void SetFeatureFlags(this HttpClient httpClient, FeatureFlag featureFlag)
        {
            httpClient.DefaultRequestHeaders.Add(CoreConstants.Headers.FeatureFlag, featureFlag.ToString());
        }
    }
}
