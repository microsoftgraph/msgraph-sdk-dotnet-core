# Frequently Asked Questions

## How do I get past the System.IO.FileLoadException

We built the .Net Microsoft Graph client library against a specific version of the NewtonSoft.Json client library. That version is specified in the .Net client library assembly manifest. We specify a range of potential NewtonSoft.Json versions that can be used by our client library in our Nuget package specification. This leads to cases where if the project where the .Net client library is added to already had a version of NewtonSoft.Json, or if one of your other dependencies depends on a different version of NewtonSoft.Json, then we could have a mismatch between the NEwtonSoft version in the project, and the version used when compiling the Microsoft Graph .Net client library. We specify the range of NewtonSoft.Json in our Nuget package specification as we are confident that any one of those version will work for your application and we don't want to force you to upgrade if you don't have to. You have three options to get around this scenario:

1. Add a [binding redirect to your application](https://docs.microsoft.com/en-us/dotnet/framework/configure-apps/redirect-assembly-versions#redirecting-assembly-versions-at-the-app-level) to unify the version of NewtonSoft.Json used by your application.
2. [Assembly resolution at runtime](https://docs.microsoft.com/en-us/dotnet/framework/app-domains/resolve-assembly-loads) if this binding redirect option is not available. Here's an [AssemblyResolutionDemo](https://github.com/danmalcolm/AssemblyResolutionDemo) that shows how this works. 
3. Change the version of the Newtonsoft dependency in your project to match the version used by Microsoft Graph .Net client library.

Why aren't we implementing the binding redirect in the .Net client library? We can't foresee which versions of the client library you may want to redirect to and you may put your assemblies in a location that can't be expected by the .Net client library.