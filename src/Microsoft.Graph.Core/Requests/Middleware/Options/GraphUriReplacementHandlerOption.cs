// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;

    /// <summary>
    /// Replaces the reserved current-user path with the Microsoft Graph /me endpoint.
    /// </summary>
    public class GraphUriReplacementHandlerOption : IUriReplacementHandlerOption
    {
        private readonly bool enabled;

        /// <summary>
        /// Creates current-user URI replacement options.
        /// </summary>
        /// <param name="enabled">Whether current-user replacement is enabled.</param>
        public GraphUriReplacementHandlerOption(bool enabled = true)
        {
            this.enabled = enabled;
        }

        /// <inheritdoc/>
        public bool IsEnabled() => enabled;

        /// <inheritdoc/>
        public Uri Replace(Uri original)
        {
            if (!enabled || original == null || !original.IsAbsoluteUri)
            {
                return original;
            }

            var path = original.AbsolutePath;
            var token = CoreConstants.ReplacementConstants.UsersEndpointWithReplaceToken;
            var index = path.IndexOf(token, StringComparison.Ordinal);
            while (index >= 0)
            {
                var end = index + token.Length;
                if (end == path.Length || path[end] == '/')
                {
                    return new UriBuilder(original)
                    {
                        Path = path.Substring(0, index) + CoreConstants.ReplacementConstants.MeEndpoint + path.Substring(end)
                    }.Uri;
                }

                index = path.IndexOf(token, end, StringComparison.Ordinal);
            }

            return original;
        }
    }
}
