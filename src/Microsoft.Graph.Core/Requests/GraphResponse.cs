﻿// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Microsoft.Kiota.Abstractions;
    /// <summary>
    /// The GraphResponse Object
    /// </summary>
    public class GraphResponse : IDisposable
    {
        /// <summary>
        /// The GraphResponse Constructor
        /// </summary>
        /// <param name="requestInformation">The Request made for the response</param>
        /// <param name="httpResponseMessage">The response</param>
        public GraphResponse(RequestInformation requestInformation, HttpResponseMessage httpResponseMessage)
        {
            this.httpResponseMessage = httpResponseMessage ??
                               throw new ArgumentException(string.Format(ErrorConstants.Messages.NullParameter, nameof(httpResponseMessage)));
            this.RequestInformation = requestInformation ??
                               throw new ArgumentException(string.Format(ErrorConstants.Messages.NullParameter, nameof(requestInformation)));
        }

        private readonly HttpResponseMessage httpResponseMessage;

        /// <summary>
        /// The Response Status code
        /// </summary>
        public HttpStatusCode StatusCode => httpResponseMessage.StatusCode;

        /// <summary>
        /// The Response Content
        /// </summary>
        public HttpContent Content => httpResponseMessage.Content;

        /// <summary>
        /// The Response Headers
        /// </summary>
        public HttpResponseHeaders HttpHeaders => httpResponseMessage.Headers;

        /// <summary>
        /// The reference to the original request
        /// </summary>
        public RequestInformation RequestInformation;

        /// <summary>
        /// Get the native Response Message
        /// </summary>
        /// <returns></returns>
        public HttpResponseMessage ToHttpResponseMessage()
        {
            return httpResponseMessage;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            httpResponseMessage?.Dispose();
        }
    }
}
