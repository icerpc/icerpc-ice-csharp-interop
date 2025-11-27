// Copyright (c) ZeroC, Inc.

using Ice;

namespace Interop.Tests;

/// <summary>Implements <see cref="Ice.Object" /> inline with a function.</summary>
internal class InlineObject : Ice.Object
{
    private readonly Func<IncomingRequest, ValueTask<OutgoingResponse>> _func;

    /// <inheritdoc />
    public ValueTask<OutgoingResponse> dispatchAsync(IncomingRequest request) => _func(request);

    /// <summary>Constructs an inline Object.</summary>
    /// <param name="func">The function that implements the dispatch.</param>
    internal InlineObject(Func<IncomingRequest, ValueTask<OutgoingResponse>> func) => _func = func;
}
