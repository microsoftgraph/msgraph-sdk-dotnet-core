// ------------------------------------------------------------------------------
//  Copyright (c) 2016 Microsoft Corporation
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    public static class Constants
    {
        public static class Headers
        {
            public const string Bearer = "Bearer";
            
            public const string SdkVersionHeaderName = "X-ClientService-ClientTag";

            public const string FormUrlEncodedContentType = "application/x-www-form-urlencoded";

            public const string SdkVersionHeaderValue = "SDK-Version=CSharp-v{0}";

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
