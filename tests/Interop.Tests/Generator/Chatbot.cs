// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using IceRpc.Features;
using IceRpc.Ice;

namespace Interop.Tests.Generator;

/// <summary>A basic, reusable implementation of <see cref="GreeterDisp_" /> and <see cref="IGreeterService" />.
/// </summary>
[Service]
internal partial class Chatbot : GreeterDisp_, IGreeterService, IIceObjectService
{
    public override string greet(string name, Current? current = null) => $"Hello, {name}!";

    public ValueTask<string> GreetAsync(
        string name,
        IFeatureCollection features,
        CancellationToken cancellationToken) => new(greet(name));
}
