// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Authentication;
    using Microsoft.Kiota.Http.HttpClientLibrary.Middleware.Options;
    using Xunit;

    public class GraphUriReplacementHandlerTests
    {
        [Theory]
        [InlineData("https://graph.microsoft.com/v1.0/users/TokenToReplace", "https://graph.microsoft.com/v1.0/me")]
        [InlineData("https://graph.microsoft.us/beta/users/TokenToReplace/messages?$filter=subject%20eq%20%27a%2Fb%27", "https://graph.microsoft.us/beta/me/messages?$filter=subject%20eq%20%27a%2Fb%27")]
        [InlineData("https://localhost:8443/custom/v1.0/users/TokenToReplace/photo/$value#fragment", "https://localhost:8443/custom/v1.0/me/photo/$value#fragment")]
        [InlineData("https://graph.microsoft.com/v1.0/users/TokenToReplace/messages/a%2Fb?value=%252F", "https://graph.microsoft.com/v1.0/me/messages/a%2Fb?value=%252F")]
        [InlineData("https://graph.microsoft.com/v1.0/users/actual-id/messages", "https://graph.microsoft.com/v1.0/users/actual-id/messages")]
        [InlineData("https://graph.microsoft.com/v1.0/users/TokenToReplaceExtra", "https://graph.microsoft.com/v1.0/users/TokenToReplaceExtra")]
        [InlineData("https://graph.microsoft.com/v1.0/me?next=/users/TokenToReplace", "https://graph.microsoft.com/v1.0/me?next=/users/TokenToReplace")]
        [InlineData("https://graph.microsoft.com/v1.0/users/tokentoreplace", "https://graph.microsoft.com/v1.0/users/tokentoreplace")]
        public async Task DefaultPipelineReplacesOnlyReservedUserPathAsync(string url, string expected)
        {
            using var httpClient = GraphClientFactory.Create(finalHandler: new FakeSuccessHandler());
            using var response = await httpClient.GetAsync(url);

            Assert.Equal(expected, response.RequestMessage.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task RequestOptionCanDisableReplacementAsync()
        {
            using var httpClient = GraphClientFactory.Create(finalHandler: new FakeSuccessHandler());
            using var adapter = new BaseGraphRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                URI = new Uri("https://graph.microsoft.com/v1.0/users/TokenToReplace")
            };
            requestInfo.AddRequestOptions(new[] { new GraphUriReplacementHandlerOption(false) });
            using var request = await adapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
            using var response = await httpClient.SendAsync(request);

            Assert.Equal(requestInfo.URI, response.RequestMessage.RequestUri);
        }

        [Fact]
        public async Task KiotaRequestOptionStillAppliesAsync()
        {
            using var httpClient = GraphClientFactory.Create(finalHandler: new FakeSuccessHandler());
            using var adapter = new BaseGraphRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                URI = new Uri("https://graph.microsoft.com/v1.0/users/TokenToReplace/messages")
            };
            requestInfo.AddRequestOptions(new[]
            {
                new UriReplacementHandlerOption(true, new[] { new KeyValuePair<string, string>("/messages", "/events") })
            });
            using var request = await adapter.ConvertToNativeRequestAsync<HttpRequestMessage>(requestInfo);
            using var response = await httpClient.SendAsync(request);

            Assert.Equal("https://graph.microsoft.com/v1.0/me/events", response.RequestMessage.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task ExplicitCustomPipelineIsNotReplacedAsync()
        {
            using var httpClient = GraphClientFactory.Create(Array.Empty<DelegatingHandler>(), finalHandler: new FakeSuccessHandler());
            const string url = "https://graph.microsoft.com/v1.0/users/TokenToReplace";
            using var response = await httpClient.GetAsync(url, CancellationToken.None);

            Assert.Equal(url, response.RequestMessage.RequestUri.AbsoluteUri);
        }
    }
}
