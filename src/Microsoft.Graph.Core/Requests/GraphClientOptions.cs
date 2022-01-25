// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// The options for setting up a given graph client
    /// </summary>
    public class GraphClientOptions
    {
        /// <summary>
        /// The target version of the api endpoint we are targeting (v1 or beta)
        /// </summary>
        public string GraphServiceTargetVersion { get; set; }

        /// <summary>
        /// The version of the service library in use. Should be in the format `x.x.x` (Semantic version)
        /// </summary>
        public string GraphServiceLibraryClientVersion { get; set; }

        /// <summary>
        /// The version of the core library in use. Should be in the format `x.x.x` (Semantic version).
        /// </summary>
        public string GraphCoreClientVersion { get; set; }

        /// <summary>
        /// The product prefix to use in setting the telemetry headers.
        /// Will default to `graph-dotnet` if not set.
        /// </summary>
        public string GraphProductPrefix { get; set; }
    }
}