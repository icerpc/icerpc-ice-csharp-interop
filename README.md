# IceRPC Ice C# Interop Tests

[![Continuous Integration](https://github.com/zeroc-ice/icerpc-ice-csharp-interop/actions/workflows/dotnet.yaml/badge.svg)](https://github.com/zeroc-ice/icerpc-ice-csharp-interop/actions/workflows/dotnet.yaml)

- [Build Requirements](#build-requirements)
- [Building the tests](#building-the-tests)
- [Running the tests](#running-the-tests)

This repository provides a test suite to verify that [IceRPC for C#](1) interoperates with [Ice for C#](2).

## Build Requirements

### Linux and macOS

You need to install the Slice compilers for Ice. See https://zeroc.com/downloads/ice/3.7/csharp.

### Using the IceRPC packages from the source distribution

Follow the instructions in the `icerpc-csharp` repository for packaging and pushing the `IceRPC` NuGet packages:
https://github.com/zeroc-ice/icerpc-csharp#packaging

## Building the tests

### Linux or macOS

```shell
./build.sh
```

### Windows

```shell
build.cmd
```

## Running the tests

```shell
dotnet test
```

This command builds the `Interop.Tests.sln` solution and executes all tests.

See <https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test> for additional options.

[1]: https://github.com:zeroc-ice/icerpc-csharp
[1]: https://github.com:zeroc-ice/ice
