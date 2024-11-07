// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions.Authentication;
using Moq;

namespace Microsoft.Graph.DotnetCore.Core.Test.Mocks
{
    public class MockAccessTokenProvider : Mock<IAccessTokenProvider>
    {
        public MockAccessTokenProvider(string accessToken = null) : base(MockBehavior.Strict)
        {
            this.Setup(x => x.GetAuthorizationTokenAsync(
                It.IsAny<Uri>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<CancellationToken>()
            )).Returns(Task.FromResult(accessToken));

            this.Setup(x => x.AllowedHostsValidator).Returns(
                new AllowedHostsValidator(new List<string> { "graph.microsoft.com" })
            );
        }
    }
}
