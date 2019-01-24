// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// Constants for the Graph Core library.
    /// </summary>
    public static class CoreConstants
    {
        /// <summary>
        /// Polling interval for task completion.
        /// </summary>
        public const int PollingIntervalInMs = 5000;

        /// <summary>
        /// Header constants.
        /// </summary>
        public static class Headers
        {
            /// Authorization bearer.
            public const string Bearer = "Bearer";
            
            /// SDK Version header
            public const string SdkVersionHeaderName = "SdkVersion";

            /// SDK Version header
            public const string SdkVersionHeaderValueFormatString = "{0}-dotnet-{1}.{2}.{3}";

            /// Content-Type header
            public const string FormUrlEncodedContentType = "application/x-www-form-urlencoded";

            /// Throw-site header
            public const string ThrowSiteHeaderName = "X-ThrowSite";

            /// Client Request Id
            public const string ClientRequestId = "client-request-id";

            /// Feature Flag
            public const string FeatureFlag = "FeatureFlag";
        }

        /// <summary>
        /// Serialization constants.
        /// </summary>
        public static class Serialization
        {
            /// OData type
            public const string ODataType = "@odata.type";
        }

        /// <summary>
        /// Feature Flag constants
        /// </summary>
        public static class FeatureFlags
        {
            /// Redirect Handler
            public const string RedirectHandler = "0x00000001";
            /// Retry Handler
            public const string RetryHandler = "0x00000002";
            /// Auth Handstring
            public const string AuthHandler = "0x00000003";
            /// Custom HtstringProvider
            public const string DefaultHttpProvider = "0x00000004";
            /// Logging Hstringler
            public const string LoggingHandler = "0x00000008";
            /// Service Dstringovery Handler
            public const string ServiceDiscoveryHandler = "0x00000010";
            /// CompressistringHandler
            public const string CompressionHandler = "0x00000020";
            /// ConnnectistringPool Manager
            public const string ConnectionPoolManager = "0x00000040";
            /// Long Runnstring Operation Handler 
            public const string LongRunnungOperationHandler = "0x00000080";
        }
    }
}
