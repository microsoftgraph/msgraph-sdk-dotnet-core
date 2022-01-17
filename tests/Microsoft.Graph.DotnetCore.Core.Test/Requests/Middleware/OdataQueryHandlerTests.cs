// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests.Middleware
{
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Authentication;
    using Microsoft.Kiota.Http.HttpClientLibrary;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class OdataQueryHandlerTests
    {
        private readonly HttpMessageInvoker _invoker;
        private readonly HttpClientRequestAdapter requestAdapter;

        public OdataQueryHandlerTests()
        {
            var odataQueryHandler = new OdataQueryHandler
            {
                InnerHandler = new FakeSuccessHandler()
            };
            this._invoker = new HttpMessageInvoker(odataQueryHandler);
            requestAdapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider());
        }

        [Fact]
        public async Task ItReplacesOdataQueryParametersByDefaultAsync()
        {
            // Arrange
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost?Select=something&exPand=somethingElse(select=nested)&$top=10")
            };

            // Act and get a request message
            var requestMessage = requestAdapter.GetRequestMessageFromRequestInformation(requestInfo);
            Assert.Empty(requestMessage.Headers);

            // Act
            var response = await _invoker.SendAsync(requestMessage, new CancellationToken());

            var queryString = response.RequestMessage.RequestUri.Query;

            // Assert the request was enriched as expected
            Assert.Contains("$Select=something", queryString);
            Assert.Contains("$exPand=somethingElse", queryString);
            Assert.Contains("$select=nested", queryString);
            Assert.Contains("&$top=10", queryString); // No doulble $ for already existing ones
        }

        [Fact]
        public async Task ItDoesNotReplaceOdataQueryParametersUsingConsfigurator()
        {
            // Arrange
            var requestInfo = new RequestInformation
            {
                HttpMethod = Method.GET,
                URI = new Uri("http://localhost?Select=something&exPand=somethingElse(select=nested)&$top=10")
            };
            var requestOption = new ODataQueryHandlerOption
            {
                ShouldReplace = (request) => false // do not change the query options
            };


            // Act and get a request message
            requestInfo.AddRequestOptions(requestOption);
            var requestMessage = requestAdapter.GetRequestMessageFromRequestInformation(requestInfo);
            Assert.Empty(requestMessage.Headers);

            // Act
            var response = await _invoker.SendAsync(requestMessage, new CancellationToken());

            var queryString = response.RequestMessage.RequestUri.Query;

            // Assert the request was enriched as expected
            Assert.Contains("Select=something", queryString);
            Assert.DoesNotContain("$Select=something", queryString);
            Assert.Contains("exPand=somethingElse", queryString);
            Assert.DoesNotContain("$exPand=somethingElse", queryString);
            Assert.Contains("select=nested", queryString);
            Assert.DoesNotContain("$select=nested", queryString);
            Assert.Contains("&$top=10", queryString); // No doulble $ for already existing ones
        }
    }

    internal class FakeSuccessHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                RequestMessage = request
            };
            return Task.FromResult(response);
        }
    }
}
