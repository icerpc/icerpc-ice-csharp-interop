// Copyright (c) ZeroC, Inc.

using IceRpc;
using IceRpc.Slice;
using NUnit.Framework;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
public class IceObjectTests
{
    /// <summary>An Ice client sends ice_ping to an IceRPC service.</summary>
    [Test]
    public async Task Ice_ping_on_IceRPC_service()
    {
        await using var server = new Server(new Service(), new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using Ice.Communicator communicator = Ice.Util.initialize();
        Ice.ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        Assert.That(async () => await proxy.ice_pingAsync(), Throws.Nothing);
    }

    /// <summary>An IceRPC client sends ice_ping to an Ice object.</summary>
    [Test]
    public async Task Ice_ping_on_Ice_object()
    {
        using Ice.Communicator communicator = Ice.Util.initialize();
        var adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.add(new IceObject(), new Ice.Identity("hello", ""));
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        var proxy = new IceObjectProxy(clientConnection, new Uri("ice:/hello"));

        Assert.That(async () => await proxy.IcePingAsync(), Throws.Nothing);
    }

    private class IceObject : Ice.ObjectImpl
    {
    }
}