// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using NUnit.Framework;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
internal class OperationTests
{
    [Test]
    public async Task Request_from_icerpc_client()
    {
        using var communicator = new Ice.Communicator();
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        _ = adapter.add(new Chatbot(), Util.stringToIdentity("hello"));
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        var proxy = new GreeterProxy(clientConnection, new Uri("ice:/hello"));

        // Act/Assert
        Assert.That(async () => await proxy.GreetAsync("Alice"), Throws.Nothing);
    }

    [Test]
    public async Task Request_from_ice_client()
    {
        await using var server = new Server(new Chatbot(), new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using var communicator = new Ice.Communicator();
        GreeterPrx proxy = GreeterPrxHelper.uncheckedCast(communicator.CreateObjectPrx("hello", serverAddress));

        // Act/Assert
        Assert.That(async () => await proxy.greetAsync("Alice"), Throws.Nothing);
    }
}
