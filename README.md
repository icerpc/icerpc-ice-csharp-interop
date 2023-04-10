# IceRPC Ice C# Interop Tests

[![Continuous Integration](https://github.com/zeroc-ice/icerpc-ice-csharp-interop/actions/workflows/dotnet.yaml/badge.svg)](https://github.com/zeroc-ice/icerpc-ice-csharp-interop/actions/workflows/dotnet.yaml)

- [Build Requirements](#build-requirements)
- [Building the tests](#building-the-tests)
- [Running the tests](#running-the-tests)

This repository provides a test suite to verify that [IceRPC for C#](1) interoperates with [Ice for C#](2).

## Build Requirements

### Linux and macOS

You need to install the Slice compilers for Ice. See https://zeroc.com/downloads/ice/3.7/csharp.

### Using pre-built IceRPC preview packages on Linux

On Linux, you can use pre-built IceRPC preview packages by creating a `nuget.config` file in this project directory with
the following contents:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <add key="github" value="https://nuget.pkg.github.com/zeroc-ice/index.json" />
    </packageSources>
    <packageSourceCredentials>
        <github>
            <add key="Username" value="USERNAME" />
            <add key="ClearTextPassword" value="TOKEN" />
        </github>
    </packageSourceCredentials>
</configuration>
```

This `nuget.config` adds a new package source with the private preview packages for IceRPC C#.

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
