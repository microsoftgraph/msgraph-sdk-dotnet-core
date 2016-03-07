# Microsoft Graph SDK for C#

Integrate the [Microsoft Graph API](https://graph.microsoft.io) into your C#
project!

The Microsoft Graph SDK is built as a Portable Class Library and targets the
following frameworks:

* .NET 4.5.1
* .NET for Windows Store apps
* Windows Phone 8.1 and higher

## Installation via NuGet

To install the Graph SDK via NuGet:

* Search for `Microsoft.Graph` in the NuGet Library, or
* Type `Install-Package Microsoft.Graph` into the Package Manager Console.

## Getting started

### 1. Register your application

Register your application to use Microsoft Graph API using one of the following
supported authentication providers:

* [Microsoft Application Registration Portal](https://apps.dev.microsoft.com):
  Register a new application that works with Microsoft Account and/or
  organizational accounts using the Azure unified authentication end point.
* [Microsoft Azure Active Directory](https://manage.windowsazure.com): Register
  a new application in your tenant's Active Directory to support work or school
  users for your tenant or multiple tenants.

### 2. Setting your application id and scopes

Before being granted access to Microsoft Graph APIs your application requests
consent from the user for the aspects of Microsoft Graph it will be using. These
are defined using authorization scopes. Your application can either ask for all
required permissions initially, or can request essential scopes and then request
additional scopes as necessary.

For more information on available scopes, see [Authentication scopes](https://dev.onedrive.com/auth/msa_oauth.htm#authentication-scopes).

### 3. Create a Microsoft Graph Client object with an authentication provider

An instance of the **GraphServicesClient** class handles building requests,
sending them to Microsoft Graph API, and processing the responses. To create a
new instance of this class, you need to provide an instance of
IAuthenticationProvider which can authenticate requests to Microsoft Graph.

For consumer and converged scenarios, you can use the OAuth2AuthProvider supplied
by the **Microsoft.Graph.OAuth** NuGet package.

```csharp
var scopes = new string[] {
  "https://graph.microsoft.com/files.readwrite",
  "https://graph.microsoft.com/users.read",
  "https://graph.microsoft.com/mail.read"
};
var authProvider = new Microsoft.Graph.OAuth2AuthProvider("app_id", scopes)
var graphClient = new Microsoft.Graph.GraphServicesClient("https://graph.microsoft.com", authProvider);
```

Before using the newly created **graphCliet** object, you need to have the user
login and consent to the requested scopes. You initiate the login flow by calling
**LoginAsync()** on the **OAuth2AuthProvider** instance.

```csharp
await authProvider.LoginAsync();
```

For more information on using the OAuth2AuthProvider, see
[Using the OAuth2 Authentication Provider](docs/oauth2authprovider.md).

For enterprise and advanced scenarios, you can use ADAL or another authentication
library and built-in **DelegateAuthProvider** class to authenticate each request. For an
example, see
[more information about using ADAL with Microsoft Graph SDK](docs/UsingAdalWithGraphSDK.md).

### 4. Make requests to the graph

Once you have completed authentication and have a GraphServicesClient, you can
begin to make calls to the service. The requests in the SDK follow the format
of the Microsoft Graph API's RESTful syntax.

For example, to retrieve a user's OneDrive:

```csharp
var drive = await graphClient.Me.Drive.Request().GetAsync();
```

`GetAsync` will return a `Drive` object on success and throw a
`GraphException` on error.

To get the current user's root folder of their OneDrive:

```csharp
var rootItem = await graphClient.Me.Drive.Root.Request().GetAsync();
```

`GetAsync` will return a `DriveItem` object on success and throw a
`GraphException` on error.

For a general overview of how the SDK is designed, see [overview](docs/overview.md).

The following sample applications are also available:
* [OneDrive API Browser](samples/OneDriveApiBrowser) - Windows Forms app
* [OneDrive Photo Browser](samples/OneDrivePhotoBrowser) - Windows Universal app

To run the OneDrivePhotoBrowser sample app your machine will need to be
configured for [UWP app development](https://msdn.microsoft.com/en-us/library/windows/apps/dn609832.aspx)
and the project must be associated with the Windows Store.

## Documentation and resources

* [Overview](docs/overview.md)
* [Auth](docs/auth.md)
* [Items](docs/items.md)
* [Collections](docs/collections.md)
* [Errors](docs/errors.md)
* [OneDrive API](http://dev.onedrive.com)

## Issues

To view or log issues, see [issues](https://github.com/OneDrive/onedrive-sdk-csharp/issues).

## Other resources

* NuGet Package: [https://www.nuget.org/packages/Microsoft.OneDriveSDK](https://www.nuget.org/packages/Microsoft.OneDriveSDK)


## License

[License](LICENSE.txt)
