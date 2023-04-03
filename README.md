# IceRPC Interop Tests

[![Continuous Integration](https://github.com/zeroc-ice/icerpc-ice-csharp-interop/actions/workflows/dotnet.yml/badge.svg)](https://github.com/zeroc-ice/icerpc-ice-csharp-interop/actions/workflows/dotnet.yml)


- [Build Requirements](#build-requirements)
- [Building](#building)
- [Testing](#testing)

This repository contains C# tests for [IceRPC](1) interoperability with [Ice](2)

## Build Requirements

On non Windows platforms you need to install the Slice to C# compiler as documented in
https://zeroc.com/downloads/ice/3.7/csharp.


Install IceRPC NuGet packages from sources as documented in

https://github.com/zeroc-ice/icerpc-csharp#packaging


## Building

The interop tests can be built from a regular command prompt, using the following command

For Linux and macOS

```shell
./build.sh
```

For Windows

```shell
build.cmd
```

## Testing

The test suite can be run from the command line by running `dotnet test` command in the repository top-level
directory, this command builds `Interop.Tests.sln` solution an executes all tests from the solution.

For additional options see <https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test>.

You can also run the tests from Visual Studio and Visual Studio for Mac using the built-in test explorer, in this
case you need to use `Interop.Tests.sln` solution file.

Visual Studio Code users can install [.NET Core Test Explorer](https://marketplace.visualstudio.com/items?itemName=formulahendry.dotnet-test-explorer)
plug-in to run tests from it.


[1]: https://github.com:zeroc-ice/icerpc-csharp
[1]: https://github.com:zeroc-ice/ice
