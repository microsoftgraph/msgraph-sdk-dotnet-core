// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    public class ChaosHandler : DelegatingHandler
    {
        private DiagnosticSource _logger = new DiagnosticListener("Microsoft.Graph.ChaosHandler");

        private Random _random;
        private ChaosHandlerOption _globalChaosHandlerOptions;
        private List<HttpResponseMessage> _KnownGraphFailures;

        /// <summary>
        /// Create a ChaosHandler.  
        /// </summary>
        /// <param name="chaosHandlerOptions">Optional parameter to change default behavior of handler.</param>
        public ChaosHandler(ChaosHandlerOption chaosHandlerOptions = null)
        {
            _globalChaosHandlerOptions = chaosHandlerOptions ?? new ChaosHandlerOption();
            _random = new Random(DateTime.Now.Millisecond);
            LoadKnownGraphFailures(_globalChaosHandlerOptions.KnownChaos);
        } 

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Select global or per request options
            var chaosHandlerOptions = GetPerRequestOptions(request) ??_globalChaosHandlerOptions;  

            HttpResponseMessage response = null;
            // Planned Chaos or Random?
            if (chaosHandlerOptions.PlannedChaosFactory != null)
            {
                response = chaosHandlerOptions.PlannedChaosFactory(request);
                if (response != null) 
                { 
                    response.RequestMessage = request;
                    if (_logger.IsEnabled("PlannedChaosResponse"))
                        _logger.Write("PlannedChaosResponse", response);
                }
            } 
            else 
            {
                if (_random.Next(100) < chaosHandlerOptions.ChaosPercentLevel)
                {
                    response = CreateChaosResponse(chaosHandlerOptions.KnownChaos ?? _KnownGraphFailures);
                    response.RequestMessage = request;
                    if (_logger.IsEnabled("ChaosResponse"))
                        _logger.Write("ChaosResponse", response);
                }
            }

            if (response == null)
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            return response;
        }

        private ChaosHandlerOption GetPerRequestOptions(HttpRequestMessage request)
        {
            request.Properties.TryGetValue("ChaosRequestOptions", out var optionsObject);
            return (ChaosHandlerOption)optionsObject;
        }

        private HttpResponseMessage CreateChaosResponse(List<HttpResponseMessage> knownFailures)
        {
            var responseIndex = _random.Next(knownFailures.Count);
            return knownFailures[responseIndex];
        }

        private void LoadKnownGraphFailures(List<HttpResponseMessage> knownFailures)
        {
            if (knownFailures != null && knownFailures.Count > 0)
            {
                _KnownGraphFailures = knownFailures;
            } 
            else
            {
                _KnownGraphFailures = new List<HttpResponseMessage>();
                _KnownGraphFailures.Add(Create429TooManyRequestsResponse(new TimeSpan(0, 0, 3)));
                _KnownGraphFailures.Add(Create503Response(new TimeSpan(0, 0, 3)));
                _KnownGraphFailures.Add(Create504GatewayTimeoutResponse(new TimeSpan(0, 0, 3)));
            }
        }

        public static HttpResponseMessage Create429TooManyRequestsResponse(TimeSpan retry)
        {
            var throttleResponse = new HttpResponseMessage()
            {
                StatusCode = (HttpStatusCode)429
            };
            throttleResponse.Headers.RetryAfter = new RetryConditionHeaderValue(retry);
            return throttleResponse;
        }
        public static HttpResponseMessage Create503Response(TimeSpan retry)
        {
            var serverUnavailableResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            };
            serverUnavailableResponse.Headers.RetryAfter = new RetryConditionHeaderValue(retry);
            return serverUnavailableResponse;
        }

        public static HttpResponseMessage Create502BadGatewayResponse()
        {
            var badGatewayResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.BadGateway
            };
            return badGatewayResponse;
        }

        public static HttpResponseMessage Create500InternalServerErrorResponse()
        {
            var internalServerError = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.InternalServerError
            };
            return internalServerError;
        }

        public static HttpResponseMessage Create504GatewayTimeoutResponse(TimeSpan retry)
        {
            var gatewayTimeoutResponse = new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.GatewayTimeout
            };
            gatewayTimeoutResponse.Headers.RetryAfter = new RetryConditionHeaderValue(retry);
            return gatewayTimeoutResponse;
        }

    }
}
