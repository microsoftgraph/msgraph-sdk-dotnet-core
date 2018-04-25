# Headers in the Microsoft Graph .NET Client Library


The .NET Client Library allows you to add your own custom request headers and inspect the response headers that come back from the Graph service.

## Adding request headers

Custom headers can be added by creating a new option collection and adding it to the request object:

```csharp
List<Option> options = new List<Option>();
options.Add(new HeaderOption("Etag", etag));
options.Add(new HeaderOption("If-Match", etag));
options.Add(new QueryOption("$filter", filterQuery));

var newObject = graphServiceClient
	.Object
	.Request(options)
	.Patch(updatedObject);
```

You can also pass headers in individually if you only need to include one header:

```csharp
var newObject = graphServiceClient
	.Object
	.Request(new HeaderOption("Etag", etag))
	.Patch(updatedObject);
```

## Reading response headers

HTTP response data is available in the `AdditionalData` property bag of the response object. You can access both the `statusCode` of the response and the `responseHeaders` to get more information, such as the request ID, Content-Type, and other data that may be relevant to you that is not part of the object model inherently.

To work with the response headers, you can deserialize the data using the client's serializer to make it easy to parse through the header dictionary:

```csharp
var user = await graphServiceClient....getAsync();
	
var statusCode = user.AdditionalData["statusCode"];
var responseHeaders = user.AdditionalData["responseHeaders"];

// Deserialize headers to dictionary for easy access to values
var responseHeaderCollection = graphClient
                    .HttpProvider
                    .Serializer
                    .DeserializeObject<Dictionary<string, List<string>>>(responseHeaders.ToString());

var requestId = responseHeaderCollection["request-id"][0];
```


*Currently, requests that have a return type of `void` or `Stream` do not return response headers and cannot be inspected.*
