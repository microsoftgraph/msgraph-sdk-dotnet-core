# Headers in the Microsoft Graph .NET Client Library

The .NET Client Library allows you to add your own custom request headers and inspect the response headers that come back from the Graph service.

## Adding request headers

Custom headers can be added by using the requestConfiguration object and adding it to the headers collection:

```csharp
var message = await graphServiceClient
    .Me
    .Messages["message-id"]
    .GetAsync((requestConfiguration) =>
    {
        requestConfiguration.Headers.Add("Etag", "etag");
        requestConfiguration.Headers.Add("If-Match", "ifmatch");
    });
```
