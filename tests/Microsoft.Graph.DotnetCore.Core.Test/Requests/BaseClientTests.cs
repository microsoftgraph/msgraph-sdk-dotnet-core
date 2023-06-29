// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using Xunit;
    public class BaseClientTests
    {
        private MockAuthenticationProvider authenticationProvider;
        private MockTokenCredential tokenCredential;

        public BaseClientTests()
        {
            this.authenticationProvider = new MockAuthenticationProvider();
            this.tokenCredential = new MockTokenCredential();
        }

        [Fact]
        public void BaseClient_InitializeBaseUrlWithoutTrailingSlash()
        {
            var expectedBaseUrl = "https://localhost";

            var baseClient = new BaseClient(expectedBaseUrl, this.authenticationProvider.Object);

            Assert.Equal(expectedBaseUrl, baseClient.RequestAdapter.BaseUrl);
        }
        
        [Fact]
        public void BaseClient_InitializeBaseUrlTrailingSlash()
        {
            var expectedBaseUrl = "https://localhost";

            var baseClient = new BaseClient("https://localhost/", this.authenticationProvider.Object);

            Assert.Equal(expectedBaseUrl, baseClient.RequestAdapter.BaseUrl);
        }
    }
}
