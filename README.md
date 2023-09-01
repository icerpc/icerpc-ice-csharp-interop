# The IceRPC-Ice C# Interop Tests

[![Continuous Integration][ci-badge]][ci-home]

This repository provides a test suite to verify that [IceRPC for C#][icerpc-csharp] interoperates with
[Ice for C#][ice-csharp].

The test suite uses by default the Ice and IceRPC versions specified in [build/Versions.props](build/Versions.props).

## Linux and macOS prerequisites

Install the Slice compilers for the Ice version you want to test as described on the
[Ice for C# download page](https://zeroc.com/downloads/ice/3.7/csharp).

> IceRPC for C# and Ice for C# provide distinct Slice compilers to compile Slice definitions into C# code. These
> Slice compilers are native tools.
> [IceRpc.Slice.Tools][icerpc-slice-tools] includes IceRPC's Slice compiler (`slicec-cs`) for all platforms while
> [zeroc.ice.net][zeroc-ice-net] includes Ice's Slice compiler (`slice2cs`) only for Windows x86. This is why you need
> to install manually `slice2cs` on Linux and macOS.

## Building the tests

```shell
dotnet build
```

The Ice and IceRPC versions used by the build are defined in `build/Version.props`.

## Running the tests

```shell
dotnet test
```

This command executes all tests known to the `Interop.Tests.sln` solution. See
[dotnet-test](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test) for additional options.

[ci-badge]: https://github.com/icerpc/icerpc-ice-csharp-interop/actions/workflows/dotnet.yaml/badge.svg
[ci-home]: https://github.com/icerpc/icerpc-ice-csharp-interop/actions/workflows/dotnet.yaml
[icerpc-csharp]: https://github.com:icerpc/icerpc-csharp
[icerpc-slice-tools]: https://www.nuget.org/packages/icerpc.slice.tools
[ice-csharp]: https://github.com:zeroc-ice/ice
[zeroc-ice-net]: https://www.nuget.org/packages/zeroc.ice.net
