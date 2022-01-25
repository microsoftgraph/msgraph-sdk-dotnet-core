// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Microsoft.Kiota.Abstractions;
    using Microsoft.Kiota.Abstractions.Authentication;
    using Microsoft.Kiota.Abstractions.Serialization;
    using Microsoft.Kiota.Http.HttpClientLibrary;
    using System.Net.Http;

    /// <summary>
    /// The <see cref="IRequestAdapter"/> instance for use with microsoft graph
    /// </summary>
    public class BaseGraphRequestAdapter : HttpClientRequestAdapter
    {
        /// <summary>
        /// The public constructor for <see cref="BaseGraphRequestAdapter"/>
        /// </summary>
        /// <param name="authenticationProvider">The authentication provider.</param>
        /// <param name="graphClientOptions">The options for the graph client</param>
        /// <param name="parseNodeFactory">The parse node factory.</param>
        /// <param name="serializationWriterFactory">The serialization writer factory.</param>
        /// <param name="httpClient">The native HTTP client.</param>
        public BaseGraphRequestAdapter(IAuthenticationProvider authenticationProvider, GraphClientOptions graphClientOptions = null, IParseNodeFactory parseNodeFactory = null, ISerializationWriterFactory serializationWriterFactory = null, HttpClient httpClient = null)
            : base(authenticationProvider, parseNodeFactory ?? ParseNodeFactoryRegistry.DefaultInstance, serializationWriterFactory ?? SerializationWriterFactoryRegistry.DefaultInstance, httpClient ?? GraphClientFactory.Create(graphClientOptions))
        {
        }
    }
}
