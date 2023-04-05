// Copyright (c) ZeroC, Inc.

using Ice;

namespace Interop.Tests;

internal class InlineBlobject : Blobject
{
    private readonly Func<byte[], Current, (bool Ok, byte[] OutParams)> _func;

    public override bool ice_invoke(byte[] inParams, out byte[] outParams, Current current)
    {
        (bool ok, outParams) = _func(inParams, current);
        return ok;
    }

    internal InlineBlobject(Func<byte[], Current, (bool Ok, byte[] OutParams)> func) => _func = func;
}
