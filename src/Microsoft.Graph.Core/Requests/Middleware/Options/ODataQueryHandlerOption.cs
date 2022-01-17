// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions;
    using System;
    using System.Net.Http;

    public class ODataQueryHandlerOption : IRequestOption
    {
        /// <summary>
        /// Function to determine whether a request should be modified. Defaults to returning true.
        /// </summary>
        public Func<HttpRequestMessage, bool> ShouldReplace { get; set; } = (requestMessage) => true;
    }
}


