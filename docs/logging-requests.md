# Logging requests in the Microsoft Graph .NET Client Library

The SDK does not log of request/response infomation out of the box. If you wish to log request and response information performed by the sdk there are two options. These are :-

a. Implement a LoggingHandler and add it to the list of handlers.
b. Take advantage of the fact that .NET has an inbuilt [DiagnosticHandler](https://github.com/dotnet/runtime/blob/4f8be1992cc79807d377afce640f219d4caffb5b/src/libraries/System.Net.Http/src/System/Net/Http/DiagnosticsHandler.cs#L15) in the [HttpClientHandler](https://github.com/dotnet/runtime/blob/4f8be1992cc79807d377afce640f219d4caffb5b/src/libraries/System.Net.Http/src/System/Net/Http/HttpClientHandler.cs#L33) which is used by default by the SDK.

## a. Implementing a LoggingHandler

You can implement a logging handler that looks something like this. (The example below simply logs a statement to show that the request has been sent out and the response has come back)

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
            HttpResponseMessage response = await base.SendAsync(httpRequest, cancellationToken);
            _logger.LogInformation("Received Graph response", response);// log the response as it comes back.
            return response;
        }
    }
}
```

Once the logging handler is implemented. You can add it to the list of handlers used by the SDK as follows.

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
User me = await graphServiceClient.Me.Request().GetAsync();
```

## b. Take advantage of .NET's inbuilt [DiagnosticHandler](https://github.com/dotnet/runtime/blob/4f8be1992cc79807d377afce640f219d4caffb5b/src/libraries/System.Net.Http/src/System/Net/Http/DiagnosticsHandler.cs#L15)