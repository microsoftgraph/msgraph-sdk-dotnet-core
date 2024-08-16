# Contributing to the Microsoft Graph .Net Client Library
Thanks for considering making a contribution! Read over our guidelines and we will do our best to see your PRs merged successfully.

**NOTE**: A signed a contribution license agreement is required for all contributions and is checked automatically on new pull requests. You will be asked to read and sign the agreement https://cla.microsoft.com/ after submitting a request to this repository.

There are a few different recommended paths to get contributions into the released version of this library.

## File issues
The best way to get started with a contribution is to start a dialog with us. Sometimes features will be under development or out of scope for this library and it's best to check before starting work on contribution, especially for large work items.

## Pull requests
All pull requests should be submitted against the **main** branch or a specific feature branch. The **main** branch is intended to represent the code released in the most-recent Nuget package.

Some things to note about this project:

## Commit message format
To support our automated release process, pull requests are required to follow the [Conventional Commit](https://www.conventionalcommits.org/en/v1.0.0/)
format.

Each commit message consists of a **header**, an optional **body** and an optional **footer**. The header is the first line of the commit and
MUST have a **type** (see below for a list of types) and a **description**. An optional **scope** can be added to the header to give extra context.

```
<type>[optional scope]: <short description>
<BLANK LINE>
<optional body>
<BLANK LINE>
<optional footer(s)>
```

The recommended commit types used are:

 - **feat** for feature updates (increments the _minor_ version)
 - **fix** for bug fixes (increments the _patch_ version)
 - **perf** for performance related changes e.g. optimizing an algorithm
 - **refactor** for code refactoring changes
 - **test** for test suite updates e.g. adding a test or fixing a test
 - **style** for changes that don't affect the meaning of code. e.g. formatting changes
 - **docs** for documentation updates e.g. ReadMe update or code documentation updates
 - **build** for build system changes (gradle updates, external dependency updates)
 - **ci** for CI configuration file changes e.g. updating a pipeline
 - **chore** for miscallaneous non-sdk changesin the repo e.g. removing an unused file

Adding a footer with the prefix **BREAKING CHANGE:** will cause an increment of the _major_ version.
