// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc.Features;
using IceRpc.Slice;
using IceRpc.Slice.Ice;

namespace Interop.Tests.Slice;

/// <summary>A basic, reusable implementation of <see cref="GreeterDisp_" /> and <see cref="IGreeterService" />.
/// </summary>
[SliceService]
public partial class Chatbot : GreeterDisp_, IGreeterService, IIceObjectService
{
    public override string greet(string name, Current current) => $"Hello, {name}!";

    public ValueTask<string> GreetAsync(
        string name,
        IFeatureCollection features,
        CancellationToken cancellationToken) => new(greet(name, null!));
}
