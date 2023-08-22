// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc.Features;
using IceRpc.Slice;
using IceRpc.Slice.Ice;

namespace Interop.Tests.Slice;

/// <summary>A basic, reusable implementation of <see cref="GreeterDisp_" />.</summary>
public class Chatbot : GreeterDisp_
{
    public override string greet(string name, Current? current = null) => $"Hello, {name}!";
}

/// <summary>A basic, reusable implementation of <see cref="IGreeterService" />.</summary>
public class ChatbotTwin : Service, IGreeterService, IIceObjectService
{
    public ValueTask<string> GreetAsync(
        string name,
        IFeatureCollection features,
        CancellationToken cancellationToken) => new($"Hello, {name}!");
}
