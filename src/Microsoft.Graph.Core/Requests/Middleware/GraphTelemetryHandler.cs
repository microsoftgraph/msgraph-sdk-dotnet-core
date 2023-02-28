// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Net.Http;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A <see cref="DelegatingHandler"/> implementation that telemetry for graph.
    /// </summary>
    public class GraphTelemetryHandler : DelegatingHandler
    {
        /// The version for current assembly.
        private static Version assemblyVersion = typeof(GraphTelemetryHandler).GetTypeInfo().Assembly.GetName().Version;

        /// The value for the SDK version header.
        private static string SdkVersionHeaderValue = string.Format(
                    CoreConstants.Headers.SdkVersionHeaderValueFormatString,
                    assemblyVersion.Major,
                    assemblyVersion.Minor,
                    assemblyVersion.Build);

        private readonly GraphClientOptions graphClientOptions;

        /// <summary>
        /// The <see cref="GraphClientOptions"/> constructor.
        /// </summary>
        /// <param name="graphClientOptions"></param>
        public GraphTelemetryHandler(GraphClientOptions graphClientOptions = null)
        {
            this.graphClientOptions = graphClientOptions ?? new GraphClientOptions();
        }

        /// <summary>
        /// Sends a HTTP request.
        /// </summary>
        /// <param name="httpRequest">The <see cref="HttpRequestMessage"/> to be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
        {
            if (httpRequest == null)
                throw new ArgumentNullException(nameof(httpRequest));

            // Build the service library string from the options
            var serviceLibraryString = string.Empty;
            if (!string.IsNullOrEmpty(graphClientOptions?.GraphServiceLibraryClientVersion))
            {
                serviceLibraryString = graphClientOptions?.GraphProductPrefix ?? "graph-dotnet";
                if (!string.IsNullOrEmpty(graphClientOptions?.GraphServiceTargetVersion))
                    serviceLibraryString += $"-{graphClientOptions?.GraphServiceTargetVersion}";
                serviceLibraryString += $"/{graphClientOptions?.GraphServiceLibraryClientVersion},";
            }

            // Default to the version string we have, otherwise use the ope provided
            var coreLibraryString = SdkVersionHeaderValue;
            if (!string.IsNullOrEmpty(graphClientOptions?.GraphCoreClientVersion) && !string.IsNullOrEmpty(graphClientOptions?.GraphProductPrefix))
            {
                coreLibraryString = $"{graphClientOptions?.GraphProductPrefix}-core/{graphClientOptions?.GraphCoreClientVersion}";
            }

            // Get the features section of the telemetry header
            var features = string.Empty;
            if (Environment.OSVersion != null)
                features += " hostOS=" + Environment.OSVersion + ";" + " hostArch=" + RuntimeInformation.OSArchitecture + ";"; ;
            features += " runtimeEnvironment=" + RuntimeInformation.FrameworkDescription + ";";

            var telemetryString = $"{serviceLibraryString} {coreLibraryString} (featureUsage={Enum.Format(typeof(FeatureFlag), httpRequest.GetFeatureFlags(), "x")};{features})";
            if(!httpRequest.Headers.Contains(CoreConstants.Headers.SdkVersionHeaderName))
                httpRequest.Headers.Add(CoreConstants.Headers.SdkVersionHeaderName, telemetryString);
            if (!httpRequest.Headers.Contains(CoreConstants.Headers.ClientRequestId))
                httpRequest.Headers.Add(CoreConstants.Headers.ClientRequestId, Guid.NewGuid().ToString());

            return base.SendAsync(httpRequest, cancellationToken);
        }

    }
}
