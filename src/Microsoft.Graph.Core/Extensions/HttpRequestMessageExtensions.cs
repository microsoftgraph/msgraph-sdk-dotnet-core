// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Kiota.Http.HttpClientLibrary.Extensions;

    /// <summary>
    /// Contains extension methods for <see cref="HttpRequestMessage"/>
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Get's feature request header value from the incoming <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> object</param>
        /// <returns></returns>
        internal static FeatureFlag GetFeatureFlags(this HttpRequestMessage httpRequestMessage)
        {
            httpRequestMessage.Headers.TryGetValues(CoreConstants.Headers.FeatureFlag, out IEnumerable<string> flags);

            if (!Enum.TryParse(flags?.FirstOrDefault(), out FeatureFlag featureFlag))
            {
                featureFlag = FeatureFlag.None;
            }

            return featureFlag;
        }

        /// <summary>
        /// Gets a <see cref="GraphRequestContext"/> from <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="httpRequestMessage">The <see cref="HttpRequestMessage"/> representation of the request.</param>
        /// <returns></returns>
        public static GraphRequestContext GetRequestContext(this HttpRequestMessage httpRequestMessage)
        {
            GraphRequestContext requestContext = new GraphRequestContext();
#pragma warning disable CS0618
            if (httpRequestMessage.Properties.TryGetValue(nameof(GraphRequestContext), out var requestContextObject))
#pragma warning restore CS0618
            {
                requestContext = (GraphRequestContext)requestContextObject;
            }
            return requestContext;
        }
    }
}
