Handling errors in the Microsoft Graph .NET Client Library
=====

Errors in the Microsoft Graph .NET Client Library behave like errors returned from the Microsoft Graph service. You can read more about them [here](https://graph.microsoft.io/en-us/docs/overview/errors).

Anytime you make a request against the service there is the potential for an error. In the case of an error, the request will throw a `ServiceException` object with an inner `Error` object that contains the service error details.

## Checking the error

There are a few different types of errors that can occur during a network call. These error codes are defined in [GraphErrorCode.cs](../src/Microsoft.Graph/Enums/GraphErrorCode.cs).

### Checking the error code
You can easily check if an error has a specific code by calling `IsMatch` on the error code value. `IsMatch` is not case sensitive:

```csharp
if (exception.IsMatch(GraphErrorCode.AccessDenied.ToString())
{
        // Handle access denied error
}
```

Each error object has a `Message` property as well as code. This message is for debugging purposes and is not be meant to be displayed to the user. Common error codes are defined in [GraphErrorCode.cs](../src/Microsoft.Graph/Enums/GraphErrorCode.cs).