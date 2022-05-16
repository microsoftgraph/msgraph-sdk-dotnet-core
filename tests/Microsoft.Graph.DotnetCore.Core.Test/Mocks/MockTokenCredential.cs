// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------



namespace Microsoft.Graph.DotnetCore.Core.Test.Mocks
{
    using Moq;
    using Azure.Core;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class MockTokenCredential : Mock<TokenCredential>
    {
        public MockTokenCredential()
            : base(MockBehavior.Strict)
        {
            this.Setup( tokenCredential => tokenCredential.GetTokenAsync( It.IsAny<TokenRequestContext>(),CancellationToken.None ))
                .Returns( new ValueTask<AccessToken>(new AccessToken("mockToken", DateTimeOffset.UtcNow.AddMinutes(10))));
        }
    }
}