// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using NUnit.Framework;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
public class OperationTests
{
    [Test]
    public async Task SayHello_from_icerpc_client()
    {
        using Communicator communicator = Util.initialize();
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        _ = adapter.add(new IceChatBot(), Util.stringToIdentity("hello"));
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        var proxy = new HelloProxy(clientConnection, new Uri("ice:/hello"));

        // Act/Assert
        Assert.That(async () => await proxy.SayHelloAsync("Alice"), Throws.Nothing);
    }

    [Test]
    public async Task SayHello_from_ice_client()
    {
        await using var server = new Server(new ChatBot(), new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using Communicator communicator = Util.initialize();
        HelloPrx proxy = HelloPrxHelper.uncheckedCast(communicator.CreateObjectPrx("hello", serverAddress));

        // Act/Assert
        Assert.That(async () => await proxy.sayHelloAsync("Alice"), Throws.Nothing);
    }
}
