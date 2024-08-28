// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using System;
    using System.Net.Http;
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    public class RequestTestBase : IDisposable
    {
        protected string baseUrl = "https://localhost/v1.0";

        protected MockAuthenticationProvider authenticationProvider;
        protected HttpResponseMessage httpResponseMessage;
        protected IBaseClient baseClient;

        public RequestTestBase()
        {
            this.authenticationProvider = new MockAuthenticationProvider();
            this.httpResponseMessage = new HttpResponseMessage();

            this.baseClient = new BaseClient(
                this.baseUrl,
                this.authenticationProvider.Object);
        }

        public void Dispose()
        {
            this.httpResponseMessage.Dispose();
        }
    }
}
