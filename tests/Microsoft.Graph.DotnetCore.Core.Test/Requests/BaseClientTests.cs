// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests
{
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using Microsoft.Kiota.Authentication.Azure;
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

            Assert.Equal(expectedBaseUrl, baseClient.BaseUrl);
        }

        [Fact]
        public void BaseClient_InitializeBaseUrlWithTrailingSlash()
        {
            var expectedBaseUrl = "https://localhost";

            var baseClient = new BaseClient("https://localhost/", this.authenticationProvider.Object);

            Assert.Equal(expectedBaseUrl, baseClient.BaseUrl);
        }

        [Fact]
        public void BaseClient_InitializeEmptyBaseUrl()
        {
            ServiceException exception = Assert.Throws<ServiceException>(() => new BaseClient(null, this.authenticationProvider.Object));
            Assert.Equal(ErrorConstants.Codes.InvalidRequest, exception.Error.Code);
            Assert.Equal(ErrorConstants.Messages.BaseUrlMissing, exception.Error.Message);
        }

        [Fact]
        public void BaseClient_InitializeWithTokenCredential()
        {
            var expectedBaseUrl = "https://localhost";

            var baseClient = new BaseClient(expectedBaseUrl, this.tokenCredential.Object);

            Assert.Equal(expectedBaseUrl, baseClient.BaseUrl);
            Assert.IsType<AzureIdentityAuthenticationProvider>(baseClient.AuthenticationProvider);

        }
    }
}
