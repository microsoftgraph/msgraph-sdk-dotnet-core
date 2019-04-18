// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Net.Http;

    internal static class HttpClientExtensions
    {
        /// <summary>
        /// Adds featureflag to existing header values.
        /// </summary>
        /// <param name="httpClient">The http client to set FeatureUsage header.</param>
        /// <param name="featureFlag">The Feature usage flag to set.</param>
        internal static void SetFeatureFlag(this HttpClient httpClient, FeatureFlag featureFlag)
        {
            // If feature flag header exists, add incoming flag to existing bitfield values and replace existing header with the computed bitfield total.
            if (httpClient.DefaultRequestHeaders.TryGetValues(CoreConstants.Headers.FeatureFlag, out var flags))
            {
                // Add incoming flag to existing feature flag values.
                foreach (var flag in flags)
                    if (Enum.TryParse(flag, out FeatureFlag targetFeatureFlag))
                        featureFlag |= targetFeatureFlag;

                // Remove current header value.
                httpClient.DefaultRequestHeaders.Remove(CoreConstants.Headers.FeatureFlag);
            }

            // Add/Replace new computed bitfield.
            httpClient.DefaultRequestHeaders.Add(CoreConstants.Headers.FeatureFlag, Enum.Format(typeof(FeatureFlag), featureFlag, "x"));
        }
    }
}
