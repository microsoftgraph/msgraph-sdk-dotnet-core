// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Authentication
{
    using System.Threading.Tasks;
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using System.Net.Http;
    using Xunit;

    public class TokenCredentialAuthProviderTests
    {
        private readonly MockTokenCredential _mockTokenCredential;

        public TokenCredentialAuthProviderTests()
        { 
            _mockTokenCredential = new MockTokenCredential();
        }

        [Fact]
        public async Task TokenCredentialAuthProviderReadsScopesFromContext()
        {
            // Arrange
            HttpRequestMessage htpHttpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "http://localhost");
            TokenCredentialAuthProvider tokenCredentialAuthProvider = new TokenCredentialAuthProvider(_mockTokenCredential.Object);

            // Act
            Assert.Null(htpHttpRequestMessage.Headers.Authorization);
            await tokenCredentialAuthProvider.AuthenticateRequestAsync(htpHttpRequestMessage);

            // Assert
            Assert.NotNull(htpHttpRequestMessage.Headers.Authorization);
            Assert.Equal("Bearer", htpHttpRequestMessage.Headers.Authorization.Scheme);
            Assert.Equal("mockToken", htpHttpRequestMessage.Headers.Authorization.Parameter);
        }
    }
}