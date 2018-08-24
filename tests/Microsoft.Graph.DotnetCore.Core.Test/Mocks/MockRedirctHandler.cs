// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;

namespace Microsoft.Graph.DotnetCore.Core.Test.Mocks
{
    public class MockRedirectHander : HttpMessageHandler
    {
        private HttpResponseMessage _response1
        { get; set; }
        private HttpResponseMessage _response2
        { get; set; }

        private bool _response1Sent = false;

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!_response1Sent)
            {
                _response1Sent = true;
                _response1.RequestMessage = request;
                return _response1;
            }
            else
            {
                _response1Sent = false;
                _response2.RequestMessage = request;
                return _response2;
            }
        }

        public void SetHttpResponse(HttpResponseMessage response1, HttpResponseMessage response2 = null)
        {
            this._response1 = response1;
            this._response2 = response2;
        }
            
    }
}
