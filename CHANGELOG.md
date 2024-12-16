# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project does adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.2.1](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/compare/3.2.0...3.2.1) (2024-11-18)


### Bug Fixes

* removes upper bound on System.Net.Http.WinHttpHandler ([72aa793](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/72aa793b90964335c051d7ba1c06d2f9f6aa5524))
* removes upper bound on System.Net.Http.WinHttpHandler ([4f50933](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/4f509338ed4440ae3f3e4cc6fe7069ce7bd3ee37))

## [3.2.0](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/compare/3.1.22...3.2.0) (2024-11-08)


### Features

* Add create() overloads to GraphClientFactory that enable requests to be authenticated ([ce6a88b](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/ce6a88b658e377692f93b48c0685cef7ba30f225))

## [3.1.22](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/compare/3.1.21...3.1.22) (2024-09-10)


### Bug Fixes

* adds missing cancellation token parameter ([2213321](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/22133215fc2279a9b686f3ba81cbcd5c3bac54e9))
* moves parse async out of the condition since it always accepts a cancellation token ([f29e4cd](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/f29e4cd33bf6c1e3e541aa21614aabaa52570cc5))
* resolved handling of larger batch request message ([c78b39d](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/c78b39d44e7e43054322ea124a35d380bcda6c79))

## [3.1.21](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/compare/3.1.20...3.1.21) (2024-09-04)


### Bug Fixes

* resolve trimming warnings in project ([6036cf7](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/6036cf71013e2bbcc6485adbb456df9c1556893b))

## [3.1.20](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/compare/3.1.19...3.1.20) (2024-08-28)


### Bug Fixes

* corrects async suffixes where wrongly used ([0e37d8a](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/0e37d8a286c22f5c10b98d0508150937d50ac6d1))

## [3.1.19](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/compare/3.1.18...3.1.19) (2024-08-26)


### Bug Fixes

* Retain insertion order of batch request steps ([2e9773d](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/2e9773d053a239140fa45acf6add0c584fa74b8e))

## [3.1.18](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/compare/v3.1.17...3.1.18) (2024-08-26)


### Bug Fixes

* encoded batch request URI issue ([b1714c9](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/b1714c97cd7fda9f752c4b40fda211f3fd9a33f6))

## [3.1.17](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/compare/v3.1.16...3.1.17) (2024-08-19)


### Bug Fixes

* Add option to specify requestId manually when calling `AddBatchRequestStepAsync()` ([#871](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/871))
* Fixing code scanning alert on AAD issuer validation ([523a5dc](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/523a5dc26edfe2f11270658928680eacacfe2f25))

## [3.1.16](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/compare/v3.1.15...3.1.16) (2024-08-15)


### Bug Fixes

* misalignment with RP config version ([e1d8932](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/e1d89327dea6786a3699febf948968776d226d14))
* package name ([64d04b5](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/64d04b5d7a98347e237c713764642738491c7eb7))
* remove v in tag from configuration ([a6a422f](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/a6a422f0d9362111573a4628e3abe6a18610185b))
* start sha for release please ([a6a422f](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/a6a422f0d9362111573a4628e3abe6a18610185b))
* updates dependencies dependabot forgot ([cfc2686](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/commit/cfc2686dc21c8256e79d764d986d269570152639))

## [3.1.15] - 2024-08-09

### Changed

- Updates the Kiota dependencies to the latest version
- Enabled Continuous Access evaluation by default.

## [3.1.14] - 2024-07-23

### Changed

- Obsoletes custom decompression handler in favor of native client capabilities at https://github.com/microsoft/kiota-dotnet/pull/303

## [3.1.12] - 2024-07-03

### Changed

- Updates the Kiota dependencies to the latest version for generation updates removing Linq usage.

## [3.1.12] - 2024-05-28

### Changed

- Updates the Kiota dependencies to the latest version.

## [3.1.11] - 2024-04-30

### Changed

- Updates the Kiota dependencies to the latest version.

## [3.1.10] - 2024-03-28

### Changed

- Updates the Kiota dependencies to the latest version.
- Updates `BatchRequestContentCollection` constructor to accept `IRequestAdapter` to unlock self serve scenarios [#815](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/815)

## [3.1.9] - 2024-03-18

### Changed

- Updates the Kiota dependencies to the latest version.
- Cleans up unreferenced dependencies [#809](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/809)

## [3.1.8] - 2024-02-16

### Changed

- Updates the Kiota dependencies to the latest version.
- Add null-checks to properties of ServiceException when IsMatch is called to prevent a NullReferenceException

## [3.1.7] - 2024-02-09

### Changed

- Updates the Kiota dependencies to the latest version.

## [3.1.6] - 2024-01-23

### Changed

- ReadOnlySubstream is seekable to be compatible with RetryHandler.

## [3.1.5] - 2024-01-15

### Changed

- Bumps JWT dependencies to security vulnerability [#792](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/792).
- Fixes `ITokenValidable` interface to use kiota generated types.

## [3.1.4] - 2024-01-09

### Changed

- Bumps abstraction dependencies to fix url encoding of special characters
- Bumps abstractions http dependencies to fix `ActivitySource` memory leak when the HttpClientRequestAdapter does not construct the HttpClient internally.

## [3.1.3] - 2023-11-29

### Changed

- Fixes a bug when getting failed batch requests with a body.

## [3.1.2] - 2023-11-15

### Changed

- Updates Kiota dependencies for support of NET8.0

## [3.1.1] - 2023-11-07

### Changed

- Improves error messages when using the page iterator.

## [3.1.0] - 2023-10-24

### Added

- Adds support for trimming in .NET.

## [3.0.11] - 2023-09-05

### Changed

- Fixes a bug where large file uploads would not complete due to different cased properties.

## [3.0.10] - 2023-08-08

### Changed

- Fixes a bug where BatchRequestContentCollection.NewBatchWithFailedRequests would fail when more than 20 requests had been sent.

## [3.0.9] - 2023-06-29

### Changed

- Fixes regression in url building when the httpClient base address is used.

## [3.0.8] - 2023-06-27

### Changed

- Fixes nextLink loop exception when making delta requests with the page iterator [#1948](https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/1948)
- Bumps kiota abstraction packages to fix bugs in Backing store and allow large stream downloads.

## [3.0.7] - 2023-05-30

### Changed

- Bumps up abstractions packages for Backing store fixes involving nested properties
- Includes graph.microsoft-ppe.com in Azure Identity default token provider.

## [3.0.6] - 2023-04-18

### Added

- Include Request headers in APIException instances

## [3.0.5] - 2023-03-30

### Added

- Adds support from create a new batch request from failed requests [#636](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/pull/636)
- Updates Batch Request builders to allow passing of error mappings from service libraries.

## [3.0.4] - 2023-03-27

### Changed

- Updates kiota abstraction library dependencies to for generation updates to reduce code size

## [3.0.3] - 2023-03-21

### Changed

- Allows checking for status codes without parsing request bodies in batch requests [#626](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/pull/626)
- Updates Kiota abstraction library dependencies to fix serialization errors.

## [3.0.2] - 2023-03-13

### Changed

- Fixes missing delta link after completed iteration in the PageIterator [#619](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/619)
- Updates Kiota abstraction library dependencies

## [3.0.1] - 2023-03-07

### Added

- Adds support for enhanced batch requests [#612](https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/612)

## [3.0.0] - 2023-02-28

### Added

- GA Release supporting Kiota generated SDKs
