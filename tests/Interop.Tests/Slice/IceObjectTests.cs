// Copyright (c) ZeroC, Inc.

using IceRpc;
using IceRpc.Slice;
using IceRpc.Slice.Ice;
using NUnit.Framework;
using ZeroC.Slice;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
internal partial class IceObjectTests
{
    /// <summary>An Ice client sends ice_ping to an IceRPC service that implements Ice::Object.</summary>
    [Test]
    public async Task Ice_ping_on_IceRPC_service()
    {
        await using var server = new Server(new IceService(), new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using var communicator = new Ice.Communicator();
        Ice.ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        Assert.That(async () => await proxy.ice_pingAsync(), Throws.Nothing);
    }

    /// <summary>An IceRPC client sends ice_ping to an Ice object.</summary>
    [Test]
    public async Task Ice_ping_on_Ice_object()
    {
        using var communicator = new Ice.Communicator();
        Ice.ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        _ = adapter.add(new Chatbot(), new Ice.Identity("hello", ""));
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        var proxy = new IceObjectProxy(clientConnection, new Uri("ice:/hello"));

        Assert.That(async () => await proxy.IcePingAsync(), Throws.Nothing);
    }

    /// <summary>An Ice client sends ice_isA to an IceRPC service.</summary>
    [Test]
    public async Task Ice_isA_on_IceRPC_service()
    {
        await using var server = new Server(new Chatbot(), new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using var communicator = new Ice.Communicator();
        Ice.ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        Assert.That(async () => await proxy.ice_isAAsync(typeof(GreeterProxy).GetSliceTypeId()!), Is.True);
    }

    /// <summary>An IceRPC client sends ice_isA to an Ice object.</summary>
    [Test]
    public async Task Ice_isA_on_Ice_object()
    {
        using var communicator = new Ice.Communicator();
        Ice.ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        _ = adapter.add(new Chatbot(), new Ice.Identity("hello", ""));
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        var proxy = new IceObjectProxy(clientConnection, new Uri("ice:/hello"));

        Assert.That(async () => await proxy.IceIsAAsync(typeof(GreeterProxy).GetSliceTypeId()!), Is.True);
    }

    /// <summary>Verifies that ice_ids return the same value with Ice and IceRPC.</summary>
    [Test]
    public async Task Ice_ids()
    {
        using var communicator = new Ice.Communicator();
        Ice.ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        _ = adapter.add(new Chatbot(), new Ice.Identity("hello", ""));
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        var proxy1 = new IceObjectProxy(clientConnection, new Uri("ice:/hello"));

        await using var server = new Server(new Chatbot(), new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();
        Ice.ObjectPrx proxy2 = communicator.CreateObjectPrx("hello", serverAddress);

        // Act/Assert
        string[] ids = await proxy1.IceIdsAsync();
        Assert.That(async () => await proxy2.ice_idsAsync(), Is.EqualTo(ids));
    }

    [SliceService]
    internal partial class IceService : IIceObjectService
    {
    }
}
