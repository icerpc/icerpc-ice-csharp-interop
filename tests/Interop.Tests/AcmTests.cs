// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using IceRpc.Transports;
using NUnit.Framework;

namespace Interop.Tests;

[Parallelizable(scope: ParallelScope.All)]
public class AcmTests
{
    /// <summary>Verifies an IceRPC->Ice connection remains alive after idling for > idle timeout.</summary>
    [Test]
    public async Task Idle_connection_remains_alive()
    {
        string[] args = new string[] { "--Ice.ACM.Timeout", "3" };
        using Communicator communicator = Util.initialize(ref args);
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.activate();
        await using var clientConnection = new ClientConnection(
            new ClientConnectionOptions
            {
                ServerAddress = adapter.GetFirstServerAddress(),
                IceIdleTimeout = TimeSpan.FromSeconds(3)
            });

        TransportConnectionInformation information = await clientConnection.ConnectAsync();

        // Act/Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
        // We still have the same underlying connection.
        Assert.That(await clientConnection.ConnectAsync(), Is.SameAs(information));
    }

    /// <summary>Verifies an Ice->IceRPC connection remains alive after idling for > idle timeout.</summary>
    [Test]
    public async Task Idle_connection_remains_alive2()
    {
        await using var server = new Server(
            new ServerOptions
            {
                ConnectionOptions = new ConnectionOptions
                {
                    Dispatcher = new IceRpc.Slice.Service(),
                    IceIdleTimeout = TimeSpan.FromSeconds(3)
                },
                ServerAddress = new ServerAddress(new Uri("ice://127.0.0.1:0")),
            });

        ServerAddress serverAddress = server.Listen();

        string[] args = new string[] { "--Ice.ACM.Timeout", "60", "--Ice.Trace.Protocol", "1", "--Ice.Trace.Network", "1" };
        using Communicator communicator = Util.initialize(ref args);
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        Connection connection = await proxy.ice_getConnectionAsync();

        // Act/Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
        await proxy.ice_pingAsync(); // get the latest connection
        Assert.That(connection, Is.SameAs(proxy.ice_getCachedConnection()));
    }

    /// <summary>Verifies an IceRPC->Ice connection is shut down (and later reconnected) when inactive.</summary>
    [Test]
    public async Task Inactive_connection_is_shutdown()
    {
        string[] args = new string[] { "--Ice.ACM.Timeout", "2" };
        using Communicator communicator = Util.initialize(ref args);
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.activate();
        await using var clientConnection = new ClientConnection(
            new ClientConnectionOptions
            {
                ServerAddress = adapter.GetFirstServerAddress(),
                IceIdleTimeout = TimeSpan.FromSeconds(2),
                InactivityTimeout = TimeSpan.FromSeconds(3)
            });

        TransportConnectionInformation information = await clientConnection.ConnectAsync();

        // Act/Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.That(await clientConnection.ConnectAsync(), Is.Not.SameAs(information));
    }
}
