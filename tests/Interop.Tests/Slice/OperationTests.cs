// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using NUnit.Framework;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
public class OperationTests
{
    [Test]
    public async Task Request_from_icerpc_client()
    {
        using Communicator communicator = Util.initialize();
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        _ = adapter.add(new Chatbot(), Util.stringToIdentity("hello"));
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        var proxy = new GreeterProxy(clientConnection, new Uri($"ice:/hello"));

        // Act/Assert
        Assert.That(async () => await proxy.GreetAsync("Alice"), Throws.Nothing);
    }

    [Test]
    public async Task Request_from_icerpc_client_with_empty_identity()
    {
        using Communicator communicator = Util.initialize();
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.addDefaultServant(new Chatbot(), ""); // catch-all
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        var proxy = new GreeterProxy(clientConnection, new ServiceAddress(Protocol.Ice)); // keep "/" identity-path

        // Act/Assert
        Assert.That(async () => await proxy.GreetAsync("Alice"), Throws.Nothing);
    }

    [Test]
    public async Task Request_from_ice_client()
    {
        await using var server = new Server(new ChatbotTwin(), new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using Communicator communicator = Util.initialize();
        GreeterPrx proxy = GreeterPrxHelper.uncheckedCast(communicator.CreateObjectPrx("hello", serverAddress));

        // Act/Assert
        Assert.That(async () => await proxy.greetAsync("Alice"), Throws.Nothing);
    }
}
