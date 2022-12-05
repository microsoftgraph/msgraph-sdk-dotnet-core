// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------
namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Microsoft.Kiota.Http.HttpClientLibrary;
    using Microsoft.Kiota.Http.HttpClientLibrary.Middleware;

    /// <summary>
    /// GraphClientFactory class to create the HTTP client
    /// </summary>
    public static class GraphClientFactory
    {
        /// The default value for the overall request timeout.
        private static readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(100);

        /// Microsoft Graph service national cloud endpoints
        private static readonly Dictionary<string, string> cloudList = new Dictionary<string, string>
            {
                { Global_Cloud, "https://graph.microsoft.com" },
                { USGOV_Cloud, "https://graph.microsoft.us" },
                { China_Cloud, "https://microsoftgraph.chinacloudapi.cn" },
                { Germany_Cloud, "https://graph.microsoft.de" },
                { USGOV_DOD_Cloud, "https://dod-graph.microsoft.us" },
            };

        /// Global endpoint
        public const string Global_Cloud = "Global";
        /// US_GOV endpoint
        public const string USGOV_Cloud = "US_GOV";
        /// US_GOV endpoint
        public const string USGOV_DOD_Cloud = "US_GOV_DOD";
        /// China endpoint
        public const string China_Cloud = "China";
        /// Germany endpoint
        public const string Germany_Cloud = "Germany";

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> instance configured with the handlers provided.
        /// </summary>
        /// <param name="version">The graph version to use.</param>
        /// <param name="nationalCloud">The national cloud endpoint to use.</param>
        /// <param name="graphClientOptions">The <see cref="GraphClientOptions"/> to use with the client</param>
        /// <param name="proxy">The proxy to be used with created client.</param>
        /// <param name="finalHandler">The last HttpMessageHandler to HTTP calls.
        /// The default implementation creates a new instance of <see cref="HttpClientHandler"/> for each HttpClient.</param>
        /// <returns></returns>
        public static HttpClient Create(
            GraphClientOptions graphClientOptions = null,
            string version = "v1.0",
            string nationalCloud = Global_Cloud,
            IWebProxy proxy = null,
            HttpMessageHandler finalHandler = null)
        {
            IList<DelegatingHandler> handlers = CreateDefaultHandlers(graphClientOptions);
            return Create(handlers, version, nationalCloud, proxy, finalHandler);
        }

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> instance configured with the handlers provided.
        /// </summary>
        /// <param name="version">The graph version to use.</param>
        /// <param name="nationalCloud">The national cloud endpoint to use.</param>
        /// <param name="handlers">An ordered list of <see cref="DelegatingHandler"/> instances to be invoked as an
        /// <see cref="HttpRequestMessage"/> travels from the <see cref="HttpClient"/> to the network and an
        /// <see cref="HttpResponseMessage"/> travels from the network back to <see cref="HttpClient"/>.
        /// The handlers are invoked in a top-down fashion. That is, the first entry is invoked first for
        /// an outbound request message but last for an inbound response message.</param>
        /// <param name="proxy">The proxy to be used with created client.</param>
        /// <param name="finalHandler">The last HttpMessageHandler to HTTP calls.</param>
        /// <param name="disposeHandler">true if the inner handler should be disposed of by Dispose(), false if you intend to reuse the inner handler..</param>
        /// <returns>An <see cref="HttpClient"/> instance with the configured handlers.</returns>
        public static HttpClient Create(
            IEnumerable<DelegatingHandler> handlers,
            string version = "v1.0",
            string nationalCloud = Global_Cloud,
            IWebProxy proxy = null,
            HttpMessageHandler finalHandler = null,
            bool disposeHandler = true)
        {
            if (finalHandler == null)
            {
                finalHandler = GetNativePlatformHttpHandler(proxy);
            }
            else if ((finalHandler is HttpClientHandler) && (finalHandler as HttpClientHandler).Proxy == null && proxy != null)
            {
                (finalHandler as HttpClientHandler).Proxy = proxy;
            }
            else if ((finalHandler is HttpClientHandler) && (finalHandler as HttpClientHandler).Proxy != null && proxy != null)
            {
                throw new ArgumentException(ErrorConstants.Messages.InvalidProxyArgument);
            }

            var pipelineWithFlags = CreatePipelineWithFeatureFlags(handlers, finalHandler);
            HttpClient client = new HttpClient(pipelineWithFlags.Pipeline, disposeHandler);
            client.SetFeatureFlag(pipelineWithFlags.FeatureFlags);
            client.Timeout = defaultTimeout;
            client.BaseAddress = DetermineBaseAddress(nationalCloud, version);
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true, NoStore = true };
            return client;
        }

        /// <summary>
        /// Create a default set of middleware for calling Microsoft Graph
        /// </summary>
        /// <param name="graphClientOptions">The <see cref="GraphClientOptions"/> to use with the client</param>
        /// <returns></returns>
        public static IList<DelegatingHandler> CreateDefaultHandlers(GraphClientOptions graphClientOptions = null)
        {
            var handlers = KiotaClientFactory.CreateDefaultHandlers();
            handlers.Add(new GraphTelemetryHandler(graphClientOptions));// add the telemetry handler last.

            // TODO remove this once https://github.com/microsoft/kiota/issues/598 is closed.
            handlers.Insert(0, new CompressionHandler());

            return handlers;
        }

        /// <summary>
        /// Creates an instance of an <see cref="HttpMessageHandler"/> using the <see cref="DelegatingHandler"/> instances
        /// provided by <paramref name="handlers"/>. The resulting pipeline can be used to manually create <see cref="HttpClient"/>
        /// or <see cref="HttpMessageInvoker"/> instances with customized message handlers.
        /// </summary>
        /// <param name="finalHandler">The inner handler represents the destination of the HTTP message channel.</param>
        /// <param name="handlers">An ordered list of <see cref="DelegatingHandler"/> instances to be invoked as part
        /// of sending an <see cref="HttpRequestMessage"/> and receiving an <see cref="HttpResponseMessage"/>.
        /// The handlers are invoked in a top-down fashion. That is, the first entry is invoked first for
        /// an outbound request message but last for an inbound response message.</param>
        /// <returns>The HTTP message channel.</returns>
        public static HttpMessageHandler CreatePipeline(IEnumerable<DelegatingHandler> handlers, HttpMessageHandler finalHandler = null)
        {
            return CreatePipelineWithFeatureFlags(handlers, finalHandler).Pipeline;
        }

        /// <summary>
        /// Creates an instance of an <see cref="HttpMessageHandler"/> using the <see cref="DelegatingHandler"/> instances
        /// provided by <paramref name="handlers"/>. The resulting pipeline can be used to manually create <see cref="HttpClient"/>
        /// or <see cref="HttpMessageInvoker"/> instances with customized message handlers.
        /// </summary>
        /// <param name="finalHandler">The inner handler represents the destination of the HTTP message channel.</param>
        /// <param name="handlers">An ordered list of <see cref="DelegatingHandler"/> instances to be invoked as part
        /// of sending an <see cref="HttpRequestMessage"/> and receiving an <see cref="HttpResponseMessage"/>.
        /// The handlers are invoked in a top-down fashion. That is, the first entry is invoked first for
        /// an outbound request message but last for an inbound response message.</param>
        /// <returns>A tuple with The HTTP message channel and FeatureFlag for the handlers.</returns>
        internal static (HttpMessageHandler Pipeline, FeatureFlag FeatureFlags) CreatePipelineWithFeatureFlags(IEnumerable<DelegatingHandler> handlers, HttpMessageHandler finalHandler = null)
        {
            FeatureFlag handlerFlags = FeatureFlag.None;
            if (finalHandler == null)
            {
                finalHandler = GetNativePlatformHttpHandler();
            }

            if (handlers == null)
            {
                return (Pipeline: finalHandler, FeatureFlags: handlerFlags);
            }

            HttpMessageHandler httpPipeline = finalHandler;
            IEnumerable<DelegatingHandler> reversedHandlers = handlers.Reverse();
            HashSet<Type> existingHandlerTypes = new HashSet<Type>();
            foreach (DelegatingHandler handler in reversedHandlers)
            {
                if (handler == null)
                {
                    throw new ArgumentNullException(nameof(handlers), "DelegatingHandler array contains null item.");
                }

#if IOS || MACOS || MACCATALYST
#if IOS || MACCATALYST
                // Skip CompressionHandler since NSUrlSessionHandler automatically handles decompression on iOS and macOS and it can't be turned off.
                // See issue https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/481 for more details.
                if (finalHandler.GetType().Equals(typeof(NSUrlSessionHandler)) && handler.GetType().Equals(typeof(CompressionHandler)))
#elif MACOS
                if (finalHandler.GetType().Equals(typeof(Foundation.NSUrlSessionHandler)) && handler.GetType().Equals(typeof(CompressionHandler)))
#endif
                {
                    // Skip chaining of CompressionHandler.
                    continue;
                }
#endif

                // Check for duplicate handler by type.
                if (!existingHandlerTypes.Add(handler.GetType()))
                {
                    throw new ArgumentException($"DelegatingHandler array has a duplicate handler. {handler} has a duplicate handler.", "handlers");
                }

                // Existing InnerHandlers on handlers will be overwritten
                handler.InnerHandler = httpPipeline;
                httpPipeline = handler;

                // Register feature flag for the handler.
                handlerFlags |= GetHandlerFeatureFlag(handler);
            }

            return (Pipeline: httpPipeline, FeatureFlags: handlerFlags);
        }

        /// <summary>
        /// Gets a platform's native http handler i.e. NSUrlSessionHandler for Xamarin.iOS and Xamarin.Mac, AndroidMessageHandler for Xamarin.Android and HttpClientHandler for others.
        /// </summary>
        /// <param name="proxy">The proxy to be used with created client.</param>
        /// <returns>
        /// 1. NSUrlSessionHandler for Xamarin.iOS and Xamarin.Mac
        /// 2. AndroidMessageHandler for Xamarin.Android.
        /// 3. HttpClientHandler for other platforms.
        /// </returns>
        internal static HttpMessageHandler GetNativePlatformHttpHandler(IWebProxy proxy = null)
        {
#if IOS || MACCATALYST
            return new NSUrlSessionHandler { AllowAutoRedirect = false };
#elif MACOS
            return new Foundation.NSUrlSessionHandler { AllowAutoRedirect = false };
#elif ANDROID
            return new Xamarin.Android.Net.AndroidMessageHandler { Proxy = proxy, AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.None };
#elif NETFRAMEWORK
            // If custom proxy is passed, the WindowsProxyUsePolicy will need updating
            // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Net.Http.WinHttpHandler/src/System/Net/Http/WinHttpHandler.cs#L575
            var proxyPolicy = proxy != null ? WindowsProxyUsePolicy.UseCustomProxy : WindowsProxyUsePolicy.UseWinHttpProxy;
            return new WinHttpHandler { Proxy = proxy, AutomaticDecompression = DecompressionMethods.None , WindowsProxyUsePolicy = proxyPolicy };
#elif NET6_0_OR_GREATER
            //use resilient configs when we can https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#alternatives-to-ihttpclientfactory-1
            return new SocketsHttpHandler { Proxy = proxy, AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.None, PooledConnectionLifetime = TimeSpan.FromMinutes(1)}; 
#else
            return new HttpClientHandler { Proxy = proxy, AllowAutoRedirect = false, AutomaticDecompression = DecompressionMethods.None };
#endif
        }

        /// <summary>
        /// Gets feature flag for the specified handler.
        /// </summary>
        /// <param name="delegatingHandler">The <see cref="DelegatingHandler"/> to get its feaure flag.</param>
        /// <returns>Delegating handler feature flag.</returns>
        private static FeatureFlag GetHandlerFeatureFlag(DelegatingHandler delegatingHandler)
        {
            if (delegatingHandler is CompressionHandler)
                return FeatureFlag.CompressionHandler;
            else if (delegatingHandler is RetryHandler)
                return FeatureFlag.RetryHandler;
            else if (delegatingHandler is RedirectHandler)
                return FeatureFlag.RedirectHandler;
            else
                return FeatureFlag.None;
        }

        private static Uri DetermineBaseAddress(string nationalCloud, string version)
        {
            string cloud = "";
            if (!cloudList.TryGetValue(nationalCloud, out cloud))
            {
                throw new ArgumentException(String.Format("{0} is an unexpected national cloud.", nationalCloud, "nationalCloud"));
            }
            string cloudAddress = $"{cloud}/{version}/";
            return new Uri(cloudAddress);

        }
    }
}
