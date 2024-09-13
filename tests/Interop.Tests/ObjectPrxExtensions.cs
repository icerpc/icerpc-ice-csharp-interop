// Copyright (c) ZeroC, Inc.

using Ice;

namespace Interop.Tests;

/// <summary>Provides extension methods for interface <see cref="ObjectPrx" />.</summary>
public static class ObjectPrxExtensions
{
    /// <summary>A thin wrapper around <see cref="ObjectPrx.ice_invokeAsync" /> that accepts an un-encapsulated request
    /// payload.</summary>
    public static Task<Object_Ice_invokeResult> IceInvokeAsync(
        this ObjectPrx prx,
        string operation,
        OperationMode mode,
        byte[] payload,
        Dictionary<string, string>? context = null,
        IProgress<bool>? progress = null,
        CancellationToken cancel = default) =>
        prx.ice_invokeAsync(operation, mode, payload.ToEncapsulation(), context, progress, cancel);
}
