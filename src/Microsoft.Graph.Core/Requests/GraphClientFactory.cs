using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Net.Http.Headers;


namespace Microsoft.Graph
{
    public static class GraphClientFactory
    {
        private enum GraphServiceCloudList
        {
            Global = 1,
            US = 2,
            China = 3
        }
        /// The key for the SDK version header.
        private static readonly string SdkVersionHeaderName = CoreConstants.Headers.SdkVersionHeaderName;

        /// The version for current assembly.
        private static Version assemblyVersion = typeof(GraphClientFactory).GetTypeInfo().Assembly.GetName().Version;

        /// The value for the SDK version header.
        private static string SdkVersionHeaderValue = string.Format(
                    CoreConstants.Headers.SdkVersionHeaderValueFormatString,
                    "Graph",
                    assemblyVersion.Major,
                    assemblyVersion.Minor,
                    assemblyVersion.Build);

        /// The default value for the overall request timeout.
        private static readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(100);

     
        /// The default value for the baseAddress of HTTP client
        private static readonly Uri _baseAddress = new Uri("https://graph.microsoft.com/v1.0");

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> instance configured with the handlers provided.
        /// </summary>
        /// <param name="handlers">An ordered list of <see cref="DelegatingHandler"/> instances to be invoked as an 
        /// <see cref="HttpRequestMessage"/> travels from the <see cref="HttpClient"/> to the network and an 
        /// <see cref="HttpResponseMessage"/> travels from the network back to <see cref="HttpClient"/>.
        /// The handlers are invoked in a top-down fashion. That is, the first entry is invoked first for 
        /// an outbound request message but last for an inbound response message.</param>
        /// <returns>An <see cref="HttpClient"/> instance with the configured handlers.</returns>
        public static HttpClient CreateClient(DelegatingHandler[] handlers)
        {
            return CreateClient(handlers, defaultTimeout);
        }

        public static HttpClient CreateClient(DelegatingHandler[] handlers)
        {
            return CreateClient(handlers, defaultTimeout);
        }


        /// <summary>
        /// Creates a new <see cref="HttpClient"/> instance configured with the handlers, timeout, baseAddress,
        /// cacheControlHeaderValue and proxy provided.
        /// </summary>
        /// <param name="handlers">An ordered list of <see cref="DelegatingHandler"/> instances to be invoked as an 
        /// <see cref="HttpRequestMessage"/> travels from the <see cref="HttpClient"/> to the network and an 
        /// <see cref="HttpResponseMessage"/> travels from the network back to <see cref="HttpClient"/>.
        /// The handlers are invoked in a top-down fashion. That is, the first entry is invoked first for 
        /// an outbound request message but last for an inbound response message.</param>
        /// <param name="timeout">A <see cref="TimeSpan"/> object to be passed to the client's Timeout property</param>
        /// <param name="baseAddress">A <see cref="string"/> value to be setted as the client's BaseAddress property</param>
        /// <param name="cacheControlHeaderValue">A <see cref="CacheControlHeaderValue"/> object to be passed to Client's CacheControlHeaderValue 
        /// in DefaultRequestHeaders property.</param>
        /// <param name="proxy">A <see cref="WebProxy"/> object to be passed to client to configure InnderHandler's proxy property.</param>
        /// <returns>An <see cref="HttpClient"/> instance with the configured handlers.</returns>
        public static HttpClient CreateClient(DelegatingHandler[] handlers, TimeSpan timeout, Uri baseAddress = null, CacheControlHeaderValue cacheControlHeaderValue = null, WebProxy proxy = null)
        {
            HttpClientHandler handler = new HttpClientHandler();
            if (proxy != null)
            {
                handler.Proxy = proxy;
            }

            HttpClient client = Create(handler, handlers);

            if (timeout == null)
            {
                timeout = defaultTimeout;
            }

            return Configure(client, timeout, baseAddress, cacheControlHeaderValue);

        }



        /// <summary>
        /// Creates a new <see cref="HttpClient"/> instance configured with the handlers provided and with the
        /// provided <paramref name="innerHandler"/> as the innermost handler.
        /// </summary>
        /// <param name="innerHandler">The inner handler represents the destination of the HTTP message channel.</param>
        /// <param name="handlers">An ordered list of <see cref="DelegatingHandler"/> instances to be invoked as an 
        /// <see cref="HttpRequestMessage"/> travels from the <see cref="HttpClient"/> to the network and an 
        /// <see cref="HttpResponseMessage"/> travels from the network back to <see cref="HttpClient"/>.
        /// The handlers are invoked in a top-down fashion. That is, the first entry is invoked first for 
        /// an outbound request message but last for an inbound response message.</param>
        /// <returns>An <see cref="HttpClient"/> instance with the configured handlers.</returns>
        public static HttpClient Create(HttpMessageHandler innerHandler, params DelegatingHandler[] handlers)
        {
            HttpMessageHandler pipeline = CreatePipeline(innerHandler, handlers);
            HttpClient client = new HttpClient(pipeline);
            client.DefaultRequestHeaders.Add(SdkVersionHeaderName, SdkVersionHeaderValue);
            return client;
        }

        /// <summary>
        /// Creates an instance of an <see cref="HttpMessageHandler"/> using the <see cref="DelegatingHandler"/> instances
        /// provided by <paramref name="handlers"/>. The resulting pipeline can be used to manually create <see cref="HttpClient"/>
        /// or <see cref="HttpMessageInvoker"/> instances with customized message handlers.
        /// </summary>
        /// <param name="innerHandler">The inner handler represents the destination of the HTTP message channel.</param>
        /// <param name="handlers">An ordered list of <see cref="DelegatingHandler"/> instances to be invoked as part 
        /// of sending an <see cref="HttpRequestMessage"/> and receiving an <see cref="HttpResponseMessage"/>.
        /// The handlers are invoked in a top-down fashion. That is, the first entry is invoked first for 
        /// an outbound request message but last for an inbound response message.</param>
        /// <returns>The HTTP message channel.</returns>
        public static HttpMessageHandler CreatePipeline(HttpMessageHandler innerHandler, IEnumerable<DelegatingHandler> handlers)
        {
            if (innerHandler == null)
            {
                innerHandler = new HttpClientHandler();
            }

            if (handlers == null)
            {
                return innerHandler;
            }

            HttpMessageHandler pipeline = innerHandler;
            IEnumerable<DelegatingHandler> reversedHandlers = handlers.Reverse();
            foreach (DelegatingHandler handler in reversedHandlers)
            {
                if (handler == null)
                {
                    throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.DelegatingHandlerArray,
                        Message = ErrorConstants.Messages.DelegatingHandlerArrayContainsNullItem,
                    });
                }

                if (handler.InnerHandler != null)
                {
                    throw new ServiceException(
                   new Error
                   {
                       Code = ErrorConstants.Codes.DelegatingHandlerArrayInnerHandler,
                       Message = ErrorConstants.Messages.DelegatingHandlerArrayHasNullInnerHandler,
                   });
                }

                handler.InnerHandler = pipeline;
                pipeline = handler;
            }

            return pipeline;
        }


        /// <summary>
        /// Configure an instance of an <see cref="HttpClient"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="timeout"></param>
        /// <param name="baseAddress"></param>
        /// <param name="cacheControlHeaderValue"></param>
        /// <returns></returns>
        private static HttpClient Configure(HttpClient client, TimeSpan timeout, Uri baseAddress, CacheControlHeaderValue cacheControlHeaderValue)
        {
            try
            {
                client.Timeout = timeout;
            }
            catch (InvalidOperationException exception)
            {
                throw new ServiceException(
                    new Error
                    {
                        Code = ErrorConstants.Codes.NotAllowed,
                        Message = ErrorConstants.Messages.OverallTimeoutCannotBeSet,
                    },
                    exception);
            }

            client.BaseAddress = baseAddress == null ? _baseAddress : baseAddress;
            client.DefaultRequestHeaders.CacheControl = cacheControlHeaderValue ?? new CacheControlHeaderValue { NoCache = true, NoStore = true };
            return client;
        }

    }
}
