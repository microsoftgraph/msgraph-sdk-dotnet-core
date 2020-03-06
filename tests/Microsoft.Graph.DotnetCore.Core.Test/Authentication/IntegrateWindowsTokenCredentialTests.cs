// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------


namespace Microsoft.Graph.DotnetCore.Core.Test.Authentication
{
    using System;
    using Azure.Core;
    using Microsoft.Identity.Client;
    using Xunit;
    using System.Collections.Generic;

    public class IntegrateWindowsTokenCredentialTests
    {
        [Fact]
        public void ShouldConstructAuthProviderWithPublicClientApp()
        {
            string clientId = "00000000-0000-0000-0000-000000000000";
            string authority = "https://login.microsoftonline.com/organizations/";
            IEnumerable<string> scopes = new List<string> { "User.ReadBasic.All" };

            IPublicClientApplication publicClientApplication = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(authority)
                .Build();

            IntegratedWindowsTokenCredential integratedWindowsTokenCredential = new IntegratedWindowsTokenCredential(publicClientApplication);

            Assert.IsAssignableFrom<TokenCredential>(integratedWindowsTokenCredential);
        }

        [Fact]
        public void ConstructorShouldThrowExceptionWithNullPublicClientApp()
        {
            IEnumerable<string> scopes = new List<string> { "User.ReadBasic.All" };

            ArgumentException ex = Assert.Throws<ArgumentException>(() => new IntegratedWindowsTokenCredential(null));

            Assert.Equal(ex.ParamName, "publicClientApplication");
        }

    }
}