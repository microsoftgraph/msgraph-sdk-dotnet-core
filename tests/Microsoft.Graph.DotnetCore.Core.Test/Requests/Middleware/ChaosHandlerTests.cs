// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Net.Http;

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests.Middleware
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class ChaosHandlerTests
    {
        // Planned Chaos


        // Random Chaos
        [Fact]
        public async Task RandomChaosShouldReturnRandomFailures()
        {

            // Arrange
            var handler = new ChaosHandler()
            {
                InnerHandler = new FakeSuccessHandler()
            };

            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage();

            // Act
            Dictionary<HttpStatusCode, object> responses = new Dictionary<HttpStatusCode, object>();
            HttpResponseMessage response;

            // Make calls until all known failures have been triggered
            while (responses.Count < 3)
            {
                response = await invoker.SendAsync(request, new CancellationToken());
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    responses[response.StatusCode] = null;
                }
            }

            // Assert
            Assert.True(responses.ContainsKey((HttpStatusCode)429));
            Assert.True(responses.ContainsKey((HttpStatusCode)503));
            Assert.True(responses.ContainsKey((HttpStatusCode)504));
        }

        [Fact]
        public async Task RandomChaosWithCustomKnownFailuresShouldReturnAllFailuresRandomly()
        {

            // Arrange
            var handler = new ChaosHandler(new ChaosHandlerOption()
            {
                KnownChaos = new List<HttpResponseMessage>()
                {
                    ChaosHandler.Create429TooManyRequestsResponse(new TimeSpan(0,0,5)),
                    ChaosHandler.Create500InternalServerErrorResponse(),
                    ChaosHandler.Create503Response(new TimeSpan(0,0,5)),
                    ChaosHandler.Create502BadGatewayResponse()
                }
            })
            {
                InnerHandler = new FakeSuccessHandler()
            };

            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage();

            // Act
            Dictionary<HttpStatusCode, object> responses = new Dictionary<HttpStatusCode, object>();
            HttpResponseMessage response;

            // Make calls until all known failures have been triggered
            while (responses.Count < 4)
            {
                response = await invoker.SendAsync(request, new CancellationToken());
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    responses[response.StatusCode] = null;
                }
            }

            // Assert
            Assert.True(responses.ContainsKey((HttpStatusCode)429));
            Assert.True(responses.ContainsKey((HttpStatusCode)500));
            Assert.True(responses.ContainsKey((HttpStatusCode)502));
            Assert.True(responses.ContainsKey((HttpStatusCode)503));
        }

        [Fact]
        public async Task PlannedChaosShouldReturnChaosWhenPlanned()
        {
            // Arrange

            Func<HttpRequestMessage, HttpResponseMessage> plannedChaos = (req) =>
             {
                 if (req.RequestUri.OriginalString.Contains("/fail"))
                 {
                     return ChaosHandler.Create429TooManyRequestsResponse(new TimeSpan(0, 0, 5));
                 }

                return null;
             };

            // Create ChaosHandler with PlannedChaosFactory to 
            var handler = new ChaosHandler(new ChaosHandlerOption()
            {
                PlannedChaosFactory = plannedChaos
            })
            {
                InnerHandler = new FakeSuccessHandler()
            };

            var invoker = new HttpMessageInvoker(handler);
            

            // Act

            var request1 = new HttpRequestMessage() { 
                RequestUri = new Uri("http://example.org/success")
            };
            var response1 = await invoker.SendAsync(request1, new CancellationToken());

            var request2 = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://example.org/fail")
            };
            var response2 = await invoker.SendAsync(request2, new CancellationToken());


            // Assert
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
            Assert.Equal((HttpStatusCode)429, response2.StatusCode);
        }

    }

    internal class FakeSuccessHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                RequestMessage = request
            };
            return Task.FromResult(response);
        }
    }
}
