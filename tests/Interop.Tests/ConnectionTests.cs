// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using NUnit.Framework;

namespace Interop.Tests;

[Parallelizable(scope: ParallelScope.All)]
public class ConnectionTests
{
    // TODO: what's the point of this test??
    [Test]
    public async Task IceRPC_client_ice_server()
    {
        using Communicator communicator = Util.initialize();
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.activate();
        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());

        // Act/Assert
        Assert.That(async () => await clientConnection.ConnectAsync(), Throws.Nothing);
    }
}
