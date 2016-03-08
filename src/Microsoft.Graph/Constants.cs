// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    public static class Constants
    {
        public static class Headers
        {
            public const string Bearer = "Bearer";
            
            public const string SdkVersionHeaderName = "SdkVersion";

            public const string FormUrlEncodedContentType = "application/x-www-form-urlencoded";

            public const string SdkVersionHeaderValue = "graph-dotnet-{0}.{1}.{2}";

            public const string ThrowSiteHeaderName = "X-ThrowSite";
        }

        public static class Serialization
        {
            public const string ODataType = "@odata.type";
        }

        public static class Url
        {
            public const string AppRoot = "approot";

            public const string DeletedItems = "DeletedItems";

            public const string Drafts = "Drafts";

            public const string GraphBaseUrlFormatString = "https://graph.microsoft.com/{0}";

            public const string Inbox = "Inbox";

            public const string SentItems = "SentItems";
        }
    }
}
