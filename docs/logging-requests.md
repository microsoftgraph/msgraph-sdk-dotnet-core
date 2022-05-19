# Logging requests in the Microsoft Graph .NET Client Library

The SDK does not log of request/response infomation out of the box. If you wish to log request and response information performed by the sdk, there are currently two ways do this. These are :

    a. Implement a LoggingHandler and add it to the list of handlers.
    b. Take advantage of OpenTelemetry.NET's instrumentation of HttpClient

## a. Implementing a LoggingHandler

### Step 1: Implement a HttpClient Message Handler

You can read more about HttpClient Message Handlers [here](https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/httpclient-message-handlers).

The implemented logging handler will look something like the example below. The example simply logs a statement to show that the request has been sent out and the response has come back.

The handler should derive from `DelegatingHandler` and override the `SendAsync` method. This override will always call `base.SendAsync` so that the request can be forwarded through the request pipeline.

```cs
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GraphResponseSample
{
    public class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingHandler> _logger;

        public LoggingHandler(ILogger<LoggingHandler> logger) {
            _logger = logger;
        }

        /// <summary>
        /// Sends a HTTP request.
        /// </summary>
        /// <param name="httpRequest">The <see cref="HttpRequestMessage"/> to be sent.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for the request.</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sending Graph request", httpRequest);// log the request before it goes out.
            // Always call base.SendAsync so that the request is forwarded through the pipeline.
            HttpResponseMessage response = await base.SendAsync(httpRequest, cancellationToken);
            _logger.LogInformation("Received Graph response", response);// log the response as it comes back.
            return response;
        }
    }
}
```

In the event an exception is thrown by your custom handler, always ensure that you drain the response body so that the connection may be released for use by other requests. 
This can be done by simply doing this.

```cs
if (response.Content != null)
{
    await response.Content.ReadAsByteArrayAsync();// Drain response content to free connections.
}
```

### Step 2: Add the Handler to GraphServiceClient's list of handlers

Once the logging handler is implemented. You can add it to the list of handlers used by the SDK as follows. 
The best practice for this is to add this to the end of the handler list so that all requests/responses are captured irrespective of any handlers that are added earlier.

```cs
// create the auth provider
var authenticationProvider = new TokenCredentialAuthProvider(clientCertificateCredential,scopes);

// get the default list of handlers and add the logging handler to the list
var handlers = GraphClientFactory.CreateDefaultHandlers(authenticationProvider);
handlers.Add(new LoggingHandler());

// create the GraphServiceClient with logging support
var httpClient = GraphClientFactory.Create(handlers);
GraphServiceClient graphServiceClient = new GraphServiceClient(httpClient);

// make a request with logging enabled!!
User me = await graphServiceClient.Me.GetAsync();
```

## b. Take advantage of OpenTelemetry's instrumentation of HttpClient

Since the GraphServiceClient uses the native HttpClient internally, it is possible to take advantage of the DiagnositicHandler that is present in the default [HttpClientHandler](https://github.com/dotnet/runtime/blob/766fec7d6a9c4fad3d1f44bfe9ec2733c6689ac8/src/libraries/System.Net.Http/src/System/Net/Http/HttpClientHandler.cs#L33).

This can be done using the following steps.

### Step 1: Install OpenTelemetry.Instrumentation.Http Package

Add the reference to [`OpenTelemetry.Instrumentation.Http`](https://www.nuget.org/packages/OpenTelemetry.Instrumentation.Http) to your project. Also, add any other instrumentations & exporters you will need.

```shell
dotnet add package OpenTelemetry.Instrumentation.Http
```

### Step 2: Enable HTTP Instrumentation in your application.

This example also sets up the OpenTelemetry Console exporter, which requires adding the package
[`OpenTelemetry.Exporter.Console`](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.Console/README.md)
to the application.

```cs
// Sdk comes from the OpenTelemetry namespace provided through the installation of the OpenTelemetry.Instrumentation.Http package
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddHttpClientInstrumentation()
    .AddConsoleExporter()
    .Build();
```

### Step 3 (optional): Filter out tracked requests(if your application has other HttpClients making non-Graph requests)

By default, the instrumentation library collects all the outgoing HTTP requests. If your app makes requests to other endpoints using HttpClient, you can filter them out as follows to only trace calls going to Microsoft Graph.

```cs
// Sdk comes from the OpenTelemetry namespace provided through the installation of the OpenTelemetry.Instrumentation.Http package
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddHttpClientInstrumentation(
        (options) => options.Filter =
            (httpRequestMessage) =>
            {
                // only collect telemetry about HTTP requests to graph.microsoft.com
                return httpRequestMessage.RequestUri.Host.Equals("graph.microsoft.com");
            })
    .AddConsoleExporter()
    .Build();
```

### Step 4: Enrich the collected telemetry by using the Enrich option.

This option allows one to enrich the activity with additional information from the raw request and response objects. 
The [HttpHandlerDiagnosticListener](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Instrumentation.Http/Implementation/HttpHandlerDiagnosticListener.cs) provides three events that one can listen to based on what is happening to the request being made by HttpClient.

For event name "OnStartActivity", the actual object will be HttpRequestMessage and this event occurs when the request is being sent out.
For event name "OnStopActivity", the actual object will be HttpResponseMessage and this event occurs when the response is received back.
For event name "OnException", the actual object will be Exception and this event occurs when an exception is thrown during execution of the request.

The example below enriches the information collected by the intrumentation library with the HTTP version used when eack request is made and the responses received(on top of filtering only Graph requests as shown in the previous step).

```cs
// Sdk comes from the OpenTelemetry namespace provided through the installation of the OpenTelemetry.Instrumentation.Http package
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddHttpClientInstrumentation(
        (options) =>
        {
            options.Filter = (httpRequestMessage) =>
                    {
                        // only collect telemetry about HTTP requests to graph.microsoft.com
                        return httpRequestMessage.RequestUri.Host.Equals("graph.microsoft.com");
                    };
            options.Enrich = (activity, eventName, rawObject) =>
                {
                    switch (eventName)
                    {
                        case "OnStartActivity":
                            if (rawObject is HttpRequestMessage request)
                            {
                                //trace the HTTP version used
                                activity.SetTag("httpVersion", request.Version);
                            }
                            break;
                        case "OnStopActivity":
                            if (rawObject is HttpResponseMessage response)
                            {
                                //trace the HTTP version used
                                activity.SetTag("responseVersion", response.Version);
                            }
                            break;
                        case "OnException":
                            if (rawObject is Exception exception)
                            {
                                //trace the HTTP version used
                                activity.SetTag("stackTrace", exception.StackTrace);
                            }
                            break;
                        default:
                            break;
                    }
                };
        })
    .AddConsoleExporter()
    .Build();
```

### Step 5: Make your requests with GraphServiceClient.

Your requests made using GraphServiceClient will now be instrumented as the HttpClient instrumentation is now enabled.

```cs
// make a request with httpclient tracing enabled!!
User me = await graphServiceClient.Me.GetAsync();
```
