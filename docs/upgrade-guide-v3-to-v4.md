---
title: "Microsoft Graph Core SDK Upgrade Guide: v3.x to v4.x"
applies-to: "msgraph-sdk-dotnet-core"
from-version: "3.2.x"
to-version: "4.0.0"
breaking-changes:
  - removed-interface: IAsyncParseNodeFactory
  - changed-kiota-version: "1.22.1 → 2.0.0"
  - updated-identity-model: "8.16.0 → 8.18.0"
  - updated-test-sdk: "17.13.0 → 18.5.1"
  - pinned-xunit-runner: "2.8.2 (net462 compatibility)"
---

# Upgrade Guide: Microsoft Graph Core SDK v3.x to v4.x

This guide covers the breaking changes introduced in version 4.0 of the Microsoft Graph Core SDK and provides instructions for updating your code. The primary breaking change is the upgrade to **Kiota 2.0.0**, which consolidates parse node factory interfaces and removes synchronous deserialization methods.

---

## Summary of Breaking Changes

| Area | Change |
|------|--------|
| **Kiota Dependency** | Upgraded from 1.22.1 to 2.0.0 |
| **IAsyncParseNodeFactory** | Merged into `IParseNodeFactory`; interface removed |
| **Parse Node Factory** | Casting removed; use `IParseNodeFactory` directly |
| **Response Handlers** | Updated: `AsyncMonitor<T>`, `ResponseHandler<T>`, `DeltaResponseHandler<T>`, `UploadResponseHandler` |
| **Identity Model Packages** | Updated from 8.16.0 to 8.18.0 |
| **Test Dependencies** | Updated to support net8.0 and net10.0 |

---

## Kiota 2.0 Dependency Upgrade

Version 4.0 upgrades the Kiota packages from 1.22.1 to 2.0.0. This is a major version bump that includes breaking changes. See [Kiota Upgrade Guide: v1.x to v2.x](https://github.com/microsoft/kiota-dotnet/blob/main/docs/upgrade-guide-v1-to-v2.md) for comprehensive details.

### Updated Package Versions

| Package | v3.x | v4.x |
|---------|------|------|
| `Microsoft.Kiota.Abstractions` | 1.22.1 | 2.0.0 |
| `Microsoft.Kiota.Authentication.Azure` | 1.22.1 | 2.0.0 |
| `Microsoft.Kiota.Serialization.Json` | 1.22.1 | 2.0.0 |
| `Microsoft.Kiota.Serialization.Text` | 1.22.1 | 2.0.0 |
| `Microsoft.Kiota.Serialization.Form` | 1.22.1 | 2.0.0 |
| `Microsoft.Kiota.Http.HttpClientLibrary` | 1.22.1 | 2.0.0 |
| `Microsoft.Kiota.Serialization.Multipart` | 1.22.1 | 2.0.0 |
| `Microsoft.IdentityModel.Protocols.OpenIdConnect` | 8.16.0 | 8.18.0 |
| `Microsoft.IdentityModel.Validators` | 8.16.0 | 8.18.0 |
| `Microsoft.SourceLink.GitHub` | 8.0.0 | 10.0.203 |
| `System.Net.Http.WinHttpHandler` | 9.0.14 | 10.0.7 |

**Action required:** Update your project's package references to the versions listed above.

---

## IParseNodeFactory Interface Changes

The `IAsyncParseNodeFactory` interface has been merged into `IParseNodeFactory` in Kiota 2.0. This SDK has been updated to use the consolidated interface directly without casting.

### Before (v3.x)

```csharp
// ResponseHandler example
public class ResponseHandler<T> : IResponseHandler where T : IParsable, new()
{
    private readonly IAsyncParseNodeFactory _jsonParseNodeFactory;

    public ResponseHandler(IParseNodeFactory parseNodeFactory = null)
    {
        // Cast to IAsyncParseNodeFactory for async operations
        _jsonParseNodeFactory = parseNodeFactory as IAsyncParseNodeFactory ?? ParseNodeFactoryRegistry.DefaultInstance;
    }

    public async Task<ModelType> HandleResponseAsync<NativeResponseType, ModelType>(NativeResponseType response, Dictionary<string, ParsableFactory<IParsable>> errorMappings)
    {
        // ...
        var jsonParseNode = await _jsonParseNodeFactory.GetRootParseNodeAsync(...);
        // ...
    }
}
```

### After (v4.x)

```csharp
// ResponseHandler example
public class ResponseHandler<T> : IResponseHandler where T : IParsable, new()
{
    private readonly IParseNodeFactory _jsonParseNodeFactory;

    public ResponseHandler(IParseNodeFactory parseNodeFactory = null)
    {
        // Direct assignment; no casting needed
        _jsonParseNodeFactory = parseNodeFactory ?? ParseNodeFactoryRegistry.DefaultInstance;
    }

    public async Task<ModelType> HandleResponseAsync<NativeResponseType, ModelType>(NativeResponseType response, Dictionary<string, ParsableFactory<IParsable>> errorMappings)
    {
        // ...
        var jsonParseNode = await _jsonParseNodeFactory.GetRootParseNodeAsync(...);
        // ...
    }
}
```

### Migration Steps

If you have custom response handler implementations or use response handlers directly:

1. **Replace `IAsyncParseNodeFactory` with `IParseNodeFactory`** in field declarations and method parameters.
2. **Remove casting**: Change `parseNodeFactory as IAsyncParseNodeFactory` to direct null-coalescing `parseNodeFactory ??`.
3. **Verify `GetRootParseNodeAsync` calls**: The method signature remains the same; ensure it is awaited.

**Example migration:**

```csharp
// Before
private readonly IAsyncParseNodeFactory factory;
this.factory = parseNodeFactory as IAsyncParseNodeFactory ?? ParseNodeFactoryRegistry.DefaultInstance;

// After
private readonly IParseNodeFactory factory;
this.factory = parseNodeFactory ?? ParseNodeFactoryRegistry.DefaultInstance;
```

---

## Updated Response Handlers

The following response handler classes have been updated to use `IParseNodeFactory` directly:

- **AsyncMonitor<T>** — `src/Microsoft.Graph.Core/Requests/AsyncMonitor.cs`
- **ResponseHandler<T>** — `src/Microsoft.Graph.Core/Requests/ResponseHandler.cs`
- **DeltaResponseHandler<T>** — `src/Microsoft.Graph.Core/Requests/DeltaResponseHandler.cs`
- **UploadResponseHandler** — `src/Microsoft.Graph.Core/Requests/Upload/UploadResponseHandler.cs`

If you extend or override any of these classes, ensure you follow the migration steps above.

---

## Support Package Updates

### Test Framework Updates

- `Microsoft.NET.Test.Sdk`: 17.13.0 → 18.5.1
- `coverlet.collector`: 6.0.4 → 10.0.0
- `coverlet.msbuild`: 6.0.4 → 10.0.0
- `Microsoft.VisualStudio.Threading.Analyzers`: 17.13.2 → 17.14.15

### Test Framework Compatibility Note

**xunit.runner.visualstudio** is pinned to **2.8.2** instead of 3.1.5 to maintain compatibility with the `net462` target framework. If you upgrade your project to remove `net462` support, you may update this to 3.1.5 or later.

---

## Quick Reference: Find and Replace

| v3.x Pattern | v4.x Replacement |
|---|---|
| `: IAsyncParseNodeFactory` | `: IParseNodeFactory` |
| `as IAsyncParseNodeFactory ??` | `??` |
| `parseNodeFactory as IAsyncParseNodeFactory ??` | `parseNodeFactory ??` |

---

## Common Compiler Errors After Upgrading

### CS0246 — Type or namespace not found: IAsyncParseNodeFactory

```
error CS0246: The type or namespace name 'IAsyncParseNodeFactory' could not be found
```

**Fix:** Replace `IAsyncParseNodeFactory` with `IParseNodeFactory`. Remove any casting:

```csharp
// Before
private readonly IAsyncParseNodeFactory factory;
this.factory = parseNodeFactory as IAsyncParseNodeFactory ?? ParseNodeFactoryRegistry.DefaultInstance;

// After
private readonly IParseNodeFactory factory;
this.factory = parseNodeFactory ?? ParseNodeFactoryRegistry.DefaultInstance;
```

---

### CS0308 — Cannot use non-invocable member

```
error CS0308: The non-generic method 'GetFactory' cannot be used with type arguments
```

**Fix:** If you're using `ParseNodeFactoryRegistry.GetFactory`, refer to the [Kiota upgrade guide](https://github.com/microsoft/kiota-dotnet/blob/main/docs/upgrade-guide-v1-to-v2.md#parsenodeFactoryregistry-changes) for the updated method signature. The method now returns a tuple and is non-generic.

---

## Validation Checklist

After upgrading to v4.x:

- [ ] Update all NuGet package references to the versions listed in [Updated Package Versions](#updated-package-versions)
- [ ] Replace `IAsyncParseNodeFactory` with `IParseNodeFactory` in your code
- [ ] Remove casting expressions `as IAsyncParseNodeFactory`
- [ ] Ensure all calls to `GetRootParseNodeAsync` are awaited
- [ ] Run `dotnet build` to verify compilation
- [ ] Run `dotnet test` to validate functionality
- [ ] Test custom response handler implementations if you have any

---

## Related Resources

- [Kiota .NET Upgrade Guide: v1.x to v2.x](https://github.com/microsoft/kiota-dotnet/blob/main/docs/upgrade-guide-v1-to-v2.md)
- [Microsoft Graph Core SDK Release Notes](../../CHANGELOG.md)
- [Microsoft Graph Documentation](https://developer.microsoft.com/graph)

