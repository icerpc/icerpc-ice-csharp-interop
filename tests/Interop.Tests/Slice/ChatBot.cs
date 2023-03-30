// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc.Features;
using IceRpc.Slice;

namespace Interop.Tests.Slice;

/// <summary>A basic, reusable implementation of IHelloService.</summary>
public class ChatBot : Service, IHelloService
{
    public ValueTask<string> SayHelloAsync(
        string name,
        IFeatureCollection features,
        CancellationToken cancellationToken) => new($"Hello, {name}!");
}

/// <summary>A basic, reusable implementation of HelloDisp_.</summary>
public class IceChatBot : HelloDisp_
{
    public override string sayHello(string name, Current? current = null) => $"Hello, {name}!";
}
