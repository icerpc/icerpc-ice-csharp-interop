# IceRPC Interop Tests

[![Continuous Integration](https://github.com/zeroc-ice/icerpc-ice-csharp-interop/actions/workflows/dotnet.yml/badge.svg)](https://github.com/zeroc-ice/icerpc-ice-csharp-interop/actions/workflows/dotnet.yml)

- [Build Requirements](#build-requirements)
- [Building](#building)
- [Testing](#testing)

This repository contains C# tests for [IceRPC](1) interoperability with [Ice](2)

## Build Requirements

On non Windows platforms you need to install the Slice to C# compiler as documented in
https://zeroc.com/downloads/ice/3.7/csharp.

### Using pre-built IceRPC preview packages

For testing with the pre-built IceRPC preview packages, you must create a `nuget.config` file, in the project directory
or in one of its parent directories, with the following contents:

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

This adds the NuGet source containing the private preview packages to the available sources for the build.

*the packages only include the slicec-cs Linux compiler, and will not work in other platforms*

### Using the IceRPC packages from the source distribution

Follow the instructions in `icerpc-csharp` repository for packaging and pushing the `IceRPC` NuGet packages.

https://github.com/zeroc-ice/icerpc-csharp#packaging

## Building

You can build the interop tests from a regular command prompt, using the following command

For Linux and macOS

```shell
./build.sh
```

For Windows

```shell
build.cmd
```

## Testing

You can run the test suite from the command line by running `dotnet test` command in the repository's top-level
directory, this command builds `Interop.Tests.sln` solution an executes all tests from the solution.

For additional options see <https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test>.

[1]: https://github.com:zeroc-ice/icerpc-csharp
[1]: https://github.com:zeroc-ice/ice
