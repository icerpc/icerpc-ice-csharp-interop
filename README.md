# The IceRPC-Ice C# Interop Tests

[![CI](https://github.com/icerpc/icerpc-ice-csharp-interop/actions/workflows/ci.yml/badge.svg)][ci-home]
[![License](https://img.shields.io/github/license/icerpc/icerpc-ice-csharp-interop?color=blue)][license]

This repository provides a test suite to verify that [IceRPC for C#][icerpc-csharp] interoperates with
[Ice for C#][ice-csharp].

The test suite uses by default the Ice and IceRPC versions specified in [build/Versions.props](build/Versions.props).

## Building the tests

```shell
dotnet build
```

## Running the tests

```shell
dotnet test
```

This command executes all tests known to the `Interop.Tests.sln` solution. See
[dotnet-test](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test) for additional options.

[ci-home]: https://github.com/icerpc/icerpc-ice-csharp-interop/actions/workflows/ci.yml
[icerpc-csharp]: https://github.com/icerpc/icerpc-csharp
[ice-csharp]: https://github.com/zeroc-ice/ice
[license]: https://github.com/icerpc/icerpc-ice-csharp-interop/blob/main/LICENSE
