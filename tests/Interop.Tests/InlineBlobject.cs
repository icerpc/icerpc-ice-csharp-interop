// Copyright (c) ZeroC, Inc.

using Ice;

namespace Interop.Tests;

/// <summary>Implements <see cref="Blobject" /> inline with a function.</summary>
internal class InlineBlobject : Blobject
{
    private readonly Func<byte[], Current?, (bool Ok, byte[] ResponsePayload)> _func;

    /// <summary>Dispatches an incoming request.</summary>
    /// <param name="inParams">The request payload wrapped in an encapsulation.</param>
    /// <param name="outParams">The response payload wrapped in an encapsulation.</param>
    /// <param name="current">The Current object.</param>
    /// <returns><see langword="true" /> if <paramref name="outParams" /> carries a return value; otherwise,
    /// <paramref name="outParams" /> carries a <see cref="UserException" />.</returns>
    public override bool ice_invoke(byte[] inParams, out byte[] outParams, Current? current)
    {
        (bool ok, byte[] payload) = _func(inParams[6..], current);
        outParams = payload.ToEncapsulation();
        return ok;
    }

    /// <summary>Constructs an inline blobject.</summary>
    /// <param name="func">The function that implements the dispatch. It accepts an un-encapsulated payload and returns
    /// an un-encapsulated payload.</param>
    internal InlineBlobject(Func<byte[], Current?, (bool Ok, byte[] Payload)> func) => _func = func;
}
