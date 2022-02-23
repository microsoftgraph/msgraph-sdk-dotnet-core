// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides method(s) to deserialize raw HTTP responses into strong types.
    /// </summary>
    public class ResponseHandler<T> : IResponseHandler where T : IParsable
    {
        private readonly IParseNodeFactory _jsonParseNodeFactory;

        /// <summary>
        /// Constructs a new <see cref="ResponseHandler{T}"/>.
        /// </summary>
        /// <param name="parseNodeFactory"> The <see cref="IParseNodeFactory"/> to use for response handling</param>
        public ResponseHandler(IParseNodeFactory parseNodeFactory = null)
        {
            _jsonParseNodeFactory = parseNodeFactory ?? ParseNodeFactoryRegistry.DefaultInstance; ;
        }

        /// <summary>
        /// Process raw HTTP response into requested domain type.
        /// </summary>
        /// <typeparam name="NativeResponseType">The type of the response</typeparam>
        /// <typeparam name="ModelType">The type to return</typeparam>
        /// <param name="response">The HttpResponseMessage to handle</param>
        /// <param name="errorMappings">The errorMappings to use in the event of failed requests</param>
        /// <returns></returns>
        public async Task<ModelType> HandleResponseAsync<NativeResponseType, ModelType>(NativeResponseType response, Dictionary<string, Func<IParsable>> errorMappings)
        {
            if (response is HttpResponseMessage responseMessage && responseMessage.Content != null)
            {
                await ValidateSuccessfulResponse(responseMessage, errorMappings).ConfigureAwait(false);
                using var responseStream = await responseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
                if (typeof(T).IsAssignableFrom(typeof(ModelType)))
                {
                    var jsonParseNode = _jsonParseNodeFactory.GetRootParseNode(responseMessage.Content.Headers?.ContentType?.MediaType?.ToLowerInvariant(), responseStream);
                    return (ModelType)(object)jsonParseNode.GetObjectValue<T>();
                }
                else
                {
                    return JsonSerializer.Deserialize<ModelType>(responseStream);
                }
            }

            return default;
        }

        /// <summary>
        /// Validates the HttpResponse message is a successful response. Otherwise, throws a ServiceException with the error information
        /// present in the response body.
        /// </summary>
        /// <param name="httpResponseMessage">The <see cref="HttpResponseMessage"/> to validate</param>
        /// <param name="errorMapping">The errorMappings to use in the event of failed requests</param>
        private async Task ValidateSuccessfulResponse(HttpResponseMessage httpResponseMessage, Dictionary<string, Func<IParsable>> errorMapping)
        {
            if (httpResponseMessage.IsSuccessStatusCode)
                return;

            var statusCodeAsInt = (int)httpResponseMessage.StatusCode;
            var statusCodeAsString = statusCodeAsInt.ToString();
            using var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var rootNode = _jsonParseNodeFactory.GetRootParseNode(httpResponseMessage.Content.Headers?.ContentType?.MediaType?.ToLowerInvariant(), responseStream);
            Func<IParsable> errorFactory;
            if (errorMapping == null ||
                !errorMapping.TryGetValue(statusCodeAsString, out errorFactory) &&
                !(statusCodeAsInt >= 400 && statusCodeAsInt < 500 && errorMapping.TryGetValue("4XX", out errorFactory)) &&
                !(statusCodeAsInt >= 500 && statusCodeAsInt < 600 && errorMapping.TryGetValue("5XX", out errorFactory)))
            {
                // We don't have an error mapping available to match. So default to using the odata specification
                Error error;
                string rawResponseBody = null;
                ErrorResponse errorResponse = rootNode.GetObjectValue<ErrorResponse>();

                if (errorResponse?.Error == null)
                {
                    if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
                        error = new Error { Code = ErrorConstants.Codes.ItemNotFound };
                    else
                        error = new Error
                        {
                            Code = ErrorConstants.Codes.GeneralException,
                            Message = ErrorConstants.Messages.UnexpectedExceptionResponse
                        };
                }
                else
                {
                    error = errorResponse.Error;
                }

                if (httpResponseMessage.Content?.Headers.ContentType.MediaType == CoreConstants.MimeTypeNames.Application.Json)
                {
                    rawResponseBody = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                throw new ServiceException(error, httpResponseMessage.Headers, httpResponseMessage.StatusCode, rawResponseBody);
            }
            else
            {
                // Use the errorFactory to create an error response
                var result = rootNode.GetErrorValue(errorFactory);
                if (result is not Exception ex)
                    throw new ApiException($"The server returned an unexpected status code and the error registered for this code failed to deserialize: {statusCodeAsString}");
                else
                    throw ex;
            }
        }
    }
}
