# Microsoft Graph .NET Core Client Library
[![Build Status](https://o365exchange.visualstudio.com/O365%20Sandbox/_apis/build/status/Microsoft%20Graph/.Net/msgraph-sdk-dotnet-build-and-packaging-core?branchName=dev)](https://o365exchange.visualstudio.com/O365%20Sandbox/_build/latest?definitionId=1410&branchName=dev)
[![NuGet Version](https://buildstats.info/nuget/Microsoft.Graph.Core)](https://www.nuget.org/packages/Microsoft.Graph.Core/)

Integrate the [Microsoft Graph API](https://graph.microsoft.com) into your .NET
project!

The Microsoft Graph .NET Core Client Library contains core classes and interfaces used by [Microsoft.Graph Client Library](https://github.com/microsoftgraph/msgraph-sdk-dotnet) to send native HTTP requests to [Microsoft Graph API](https://graph.microsoft.com). The core client library targets .NetStandard 1.1 and .Net Framework 4.5.

## Installation via NuGet

To install the client library via NuGet:

* Search for `Microsoft.Graph.Core` in the NuGet Library, or
* Type `Install-Package Microsoft.Graph.Core` into the Package Manager Console.

## Getting started

### 1. Register your application

Register your application to use Microsoft Graph API by following the steps at [Register your application with the Microsoft identity platform](https://docs.microsoft.com/en-us/graph/auth-register-app-v2).

### 2. Authenticate for the Microsoft Graph service

The Microsoft Graph .NET Client Library supports the use of TokenCredential classes in the [Azure.Identity](https://www.nuget.org/packages/Azure.Identity) library.

You can read more about available Credential classes [here](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme#key-concepts) and examples on how to quickly setup TokenCredential instances can be found [here](https://github.com/microsoftgraph/msgraph-sdk-dotnet/blob/feature/5.0/docs/tokencredentials.md).

The recommended library for authenticating against Microsoft Identity (Azure AD) is [MSAL](https://github.com/AzureAD/microsoft-authentication-library-for-dotnet).

For an example of authenticating a UWP app using the V2 Authentication Endpoint, see the [Microsoft Graph UWP Connect Library](https://github.com/OfficeDev/Microsoft-Graph-UWP-Connect-Library).

### 3. Create a HttpClient object with an authentication provider

You can create an instance of **HttpClient** that is pre-configured for making requests to Microsoft Graph APIs using `GraphClientFactory`.

```cs
HttpClient httpClient = GraphClientFactory.Create( version: "beta");
```

For more information on initializing a client instance, see the [library overview](https://docs.microsoft.com/en-us/graph/sdks/sdks-overview)

### 4. Make requests to the graph

Once you have an authenticated `HttpClient`, you can begin to make calls to the service. The requests to the service follows our [REST API](https://docs.microsoft.com/en-us/graph/use-the-api) syntax.

For example, to retrieve a user's default drive:

```cs
HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "me/drive");
HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
if (response.IsSuccessStatusCode)
{
    string jsonResponse = await response.Content.ReadAsStringAsync();
}
```

To get the current user's root folder of their default drive:

```cs
HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "me/drive/root");
HttpResponseMessage response = await httpClient.SendAsync(requestMessage);
if (response.IsSuccessStatusCode)
{
    string jsonResponse = await response.Content.ReadAsStringAsync();
}
```

## Documentation and resources
* [Microsoft Graph API](https://graph.microsoft.com)
* [Release notes](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/releases)

## Notes

Install System.Runtime.InteropServices.RuntimeInformation before you install Microsoft.Graph >=1.3 if you are having an issue updating the package for a Xamarin solution. You may need to updated references to Microsoft.NETCore.UniversalWindowsPlatform to >=5.2.2 as well.

## Issues

To view or log issues, see [issues](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues).

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Other resources

* NuGet Package: [https://www.nuget.org/packages/Microsoft.Graph.Core](https://www.nuget.org/packages/Microsoft.Graph.Core)

## Building library locally

If you are looking to build the library locally for the purposes of contributing code or running tests, you will need to:

- Have the .NET Core SDK (> 1.0) installed
- Run `dotnet restore` from the command line in your package directory
- Run `nuget restore` and `msbuild` from CLI or run Build from Visual Studio to restore Nuget packages and build the project

## License

Copyright (c) Microsoft Corporation. All Rights Reserved. Licensed under the MIT [license](LICENSE.txt). See [Third Party Notices](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/blob/master/THIRD%20PARTY%20NOTICES) for information on the packages referenced via NuGet.
