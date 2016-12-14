# Microsoft Graph .NET Client Library

[![Build status](https://ci.appveyor.com/api/projects/status/3av5qjyletkwf6h8/branch/master?svg=true)](https://ci.appveyor.com/project/OneDrive/msgraph-sdk-dotnet/branch/master)
[![NuGet Version](https://buildstats.info/nuget/Microsoft.Graph)](https://www.nuget.org/packages/Microsoft.Graph/)

Integrate the [Microsoft Graph API](https://graph.microsoft.io) into your .NET
project!

The Microsoft Graph .NET Client Library is built as a Portable Class Library targeting profile 111.
This targets the following frameworks:

* .NET 4.5
* .NET for Windows Store apps
* Windows Phone 8.1 and higher

## Installation via NuGet

To install the client library via NuGet:

* Search for `Microsoft.Graph` in the NuGet Library, or
* Type `Install-Package Microsoft.Graph` into the Package Manager Console.

## Getting started

### 1. Register your application

Register your application to use Microsoft Graph API using one of the following
supported authentication portals:

* [Microsoft Application Registration Portal](https://apps.dev.microsoft.com):
  Register a new application that works with Microsoft Account and/or
  organizational accounts using the unified V2 Authentication Endpoint.
* [Microsoft Azure Active Directory](https://manage.windowsazure.com): Register
  a new application in your tenant's Active Directory to support work or school
  users for your tenant or multiple tenants.

### 2. Authenticate for the Microsoft Graph service

The Microsoft Graph .NET Client Library does not include any default authentication implementations.
Instead, the user will want to authenticate with the library of their choice, or against the OAuth
endpoint directly, and built-in **DelegateAuthenticationProvider** class to authenticate each request.
For more information on `DelegateAuthenticationProvider`, see the [library overview](docs/overview.md)

The recommended library for authenticating against AAD is [ADAL](https://github.com/AzureAD/azure-activedirectory-library-for-dotnet).

For an example of authenticating a UWP app using the V2 Authentication Endpoint, see the [Microsoft Graph UWP Connect Library](https://github.com/OfficeDev/Microsoft-Graph-UWP-Connect-Library).

### 3. Create a Microsoft Graph client object with an authentication provider

An instance of the **GraphServiceClient** class handles building requests,
sending them to Microsoft Graph API, and processing the responses. To create a
new instance of this class, you need to provide an instance of
`IAuthenticationProvider` which can authenticate requests to Microsoft Graph.

For more information on initializing a client instance, see the [library overview](docs/overview.md)

### 4. Make requests to the graph

Once you have completed authentication and have a GraphServiceClient, you can
begin to make calls to the service. The requests in the SDK follow the format
of the Microsoft Graph API's RESTful syntax.

For example, to retrieve a user's default drive:

```csharp
var drive = await graphClient.Me.Drive.Request().GetAsync();
```

`GetAsync` will return a `Drive` object on success and throw a
`ServiceException` on error.

To get the current user's root folder of their default drive:

```csharp
var rootItem = await graphClient.Me.Drive.Root.Request().GetAsync();
```

`GetAsync` will return a `DriveItem` object on success and throw a
`ServiceException` on error.

For a general overview of how the SDK is designed, see [overview](docs/overview.md).

The following sample applications are also available:
* [Microsoft Graph UWP Connect Sample](https://github.com/microsoftgraph/uwp-csharp-connect-sample)
* [Microsoft Graph UWP Snippets Sample](https://github.com/microsoftgraph/uwp-csharp-snippets-sample)
* [Microsoft Graph MeetingBot sample for UWP](https://github.com/microsoftgraph/uwp-csharp-meetingbot-sample)
* [Microsoft Graph Connect Sample for ASP.NET 4.6](https://github.com/microsoftgraph/aspnet-connect-sample)
* [Microsoft Graph Snippets Sample for ASP.NET 4.6](https://github.com/microsoftgraph/aspnet-snippets-sample)
* [Microsoft Graph SDK Snippets Library for Xamarin.Forms](https://github.com/microsoftgraph/xamarin-csharp-snippets-sample)
* [Microsoft Graph Connect Sample for Xamarin Forms](https://github.com/microsoftgraph/xamarin-csharp-connect-sample)
* [Microsoft Graph Meeting Manager Sample for Xamarin.Forms](https://github.com/microsoftgraph/xamarin-csharp-meetingmanager-sample)
* [Microsoft Graph Property Manager Sample for Xamarin Native](https://github.com/microsoftgraph/xamarin-csharp-propertymanager-sample)

## Documentation and resources

* [Overview](docs/overview.md)
* [Collections](docs/collections.md)
* [Errors](docs/errors.md)
* [Microsoft Graph API](https://graph.microsoft.io)
* [Release notes](https://github.com/microsoftgraph/msgraph-sdk-dotnet/releases)

## Notes

Install NewtonSoft.Json first if you want to use a version greater than NewtonSoft.Json 6.0.1. For example, you'll need to install NewtonSoft.Json 9.0.1 first if you want to use this to library while targeting .Net Core with standard1.0.

## Issues

To view or log issues, see [issues](https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues).

## Other resources

* NuGet Package: [https://www.nuget.org/packages/Microsoft.Graph](https://www.nuget.org/packages/Microsoft.Graph)


## License

Copyright (c) Microsoft Corporation. All Rights Reserved. Licensed under the MIT [license](LICENSE.txt)
