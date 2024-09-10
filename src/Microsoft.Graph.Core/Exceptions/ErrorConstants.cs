// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    internal static class ErrorConstants
    {
        internal static class Codes
        {
            internal const string GeneralException = "generalException";
        }

        internal static class Messages
        {
            internal const string MaximumValueExceeded = "{0} exceeds the maximum value of {1}.";

            internal const string NullParameter = "{0} parameter cannot be null.";

            internal const string UnableToDeserializeContent = "Unable to deserialize content.";

            internal const string InvalidDependsOnRequestId = "Corresponding batch request id not found for the specified dependsOn relation.";

            internal const string ExpiredUploadSession = "Upload session expired. Upload cannot resume";

            internal const string NoResponseForUpload = "No Response Received for upload.";

            internal const string MissingRetryAfterHeader = "Missing retry after header.";

            internal const string PageIteratorRequestError = "Error occured when making a request with the page iterator. See inner exception for more details.";

            internal const string BatchRequestError = "Error occured when making the batch request. See inner exception for more details.";

            internal const string InvalidProxyArgument = "Proxy cannot be set more once. Proxy can only be set on the proxy or defaultHttpHandler argument and not both.";
        }
    }
}
