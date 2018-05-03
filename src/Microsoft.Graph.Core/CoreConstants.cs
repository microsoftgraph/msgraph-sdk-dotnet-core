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
        }

        /// <summary>
        /// Serialization constants.
        /// </summary>
        public static class Serialization
        {
            /// OData type
            public const string ODataType = "@odata.type";
        }
    }
}
