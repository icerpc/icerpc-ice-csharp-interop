// Copyright (c) ZeroC, Inc.

using IceRpc.Slice;
using NUnit.Framework;

namespace IceRpc.Tests.FooInterop; // TODO: fix namespace!

[Parallelizable(scope: ParallelScope.All)]
public class IceObjectTests
{
    /// <summary>An Ice client sends ice_ping to an IceRPC service.</summary>
    [Test]
    public async Task Ice_ping_on_IceRPC_service()
    {
        await using var server = new Server(new Service(), new Uri("icerpc://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using Ice.Communicator communicator = Ice.Util.initialize();
        Ice.ObjectPrx proxy = communicator.stringToProxy($"hello:tcp -h 127.0.0.1 -p {serverAddress.Port}");

        Console.WriteLine(proxy.ToString());

        proxy.ice_ping();
    }
}
