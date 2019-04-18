// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Net.Http;

    internal static class HttpClientExtensions
    {
        internal static void SetFeatureFlag(this HttpClient httpClient, FeatureFlag featureFlag)
        {
            if (httpClient.DefaultRequestHeaders.TryGetValues(CoreConstants.Headers.FeatureFlag, out var flags))
            {
                foreach (var flag in flags)
                    if (Enum.TryParse(flag, out FeatureFlag targetFeatureFlag))
                        featureFlag |= targetFeatureFlag;
            }

            httpClient.DefaultRequestHeaders.Add(CoreConstants.Headers.FeatureFlag, Enum.Format(typeof(FeatureFlag), featureFlag, "x"));
        }
    }
}
