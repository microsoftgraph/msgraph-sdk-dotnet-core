// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Net.Http;
    public class CompressionHandlerOption: IMiddlewareOption
    {
        public Func<HttpResponseMessage, bool> ShouldDecompressResponseContent { get; set; } = (response) => true;
    }
}
