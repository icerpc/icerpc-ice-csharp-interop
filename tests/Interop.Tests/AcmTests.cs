// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using NUnit.Framework;

namespace Interop.Tests;

[Parallelizable(scope: ParallelScope.All)]
public class AcmTests
{
    /// <summary>Verifies an IceRPC->Ice connection remains alive after idling for > idle timeout. That's because the
    /// IceRPC-side sends ValidateConnection and by default doesn't expect ValidateConnection from the Ice side.
    /// </summary>
    [Test]
    public async Task Idle_connection_to_Ice_server_remains_alive([Values] bool enableIdleCheck)
    {
        using Communicator communicator = CreateCommunicator(3, enableIdleCheck);
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.activate();

        var clientConnectionFactory = new ClientProtocolConnectionFactory(
            new ConnectionOptions { EnableIceIdleCheck = enableIdleCheck, IceIdleTimeout = TimeSpan.FromSeconds(3) });

        await using IProtocolConnection clientConnection =
            clientConnectionFactory.CreateConnection(adapter.GetFirstServerAddress());

        Task shutdownRequested = (await clientConnection.ConnectAsync()).ShutdownRequested;

        // Act/Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.That(shutdownRequested.IsCompleted, Is.False);
    }

    /// <summary>Verifies an Ice->IceRPC connection remains alive after idling for > idle timeout. That's because the
    /// IceRPC-side sends ValidateConnection and by default doesn't expect ValidateConnection from the Ice side.
    /// </summary>
    [Test]
    public async Task Idle_connection_to_IceRpc_server_remains_alive([Values] bool enableIdleCheck)
    {
        await using var server = new Server(
            new ServerOptions
            {
                ConnectionOptions = new ConnectionOptions
                {
                    Dispatcher = new IceRpc.Slice.Service(),
                    EnableIceIdleCheck = enableIdleCheck,
                    IceIdleTimeout = TimeSpan.FromSeconds(3)
                },
                ServerAddress = new ServerAddress(new Uri("ice://127.0.0.1:0")),
            });

        ServerAddress serverAddress = server.Listen();

        using Communicator communicator = CreateCommunicator(3, enableIdleCheck);
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        bool connectionLost = false;
        Connection connection = await proxy.ice_getConnectionAsync();
        connection.setCloseCallback(_ => connectionLost = true);

        // Act/Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.That(connectionLost, Is.False);
    }

    /// <summary>Verifies an IceRPC->Ice connection is shut down when inactive. That's because the InactivityTimeout
    /// shuts down the connection.</summary>
    [Test]
    public async Task Inactive_connection_to_Ice_is_shutdown([Values] bool enableIdleCheck)
    {
        using Communicator communicator = CreateCommunicator(2, enableIdleCheck);
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.activate();

        var clientConnectionFactory = new ClientProtocolConnectionFactory(
            new ConnectionOptions
            {
                EnableIceIdleCheck = enableIdleCheck,
                IceIdleTimeout = TimeSpan.FromSeconds(2),
                InactivityTimeout = TimeSpan.FromSeconds(3)
            });

        await using IProtocolConnection clientConnection =
            clientConnectionFactory.CreateConnection(adapter.GetFirstServerAddress());

        Task shutdownRequested = (await clientConnection.ConnectAsync()).ShutdownRequested;

        // Act/Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.That(shutdownRequested.IsCompleted, Is.True);
    }

    /// <summary>Verifies an Ice->IceRPC connection is shut down when inactive. That's because the InactivityTimeout
    /// shuts down the connection.</summary>
    [Test]
    public async Task Inactive_connection_to_IceRpc_is_shutdown([Values] bool enableIdleCheck)
    {
        await using var server = new Server(
            new ServerOptions
            {
                ConnectionOptions = new ConnectionOptions
                {
                    Dispatcher = new IceRpc.Slice.Service(),
                    EnableIceIdleCheck = enableIdleCheck,
                    IceIdleTimeout = TimeSpan.FromSeconds(2),
                    InactivityTimeout = TimeSpan.FromSeconds(3)
                },
                ServerAddress = new ServerAddress(new Uri("ice://127.0.0.1:0")),
            });

        ServerAddress serverAddress = server.Listen();

        using Communicator communicator = CreateCommunicator(2, enableIdleCheck);
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        bool connectionLost = false;
        Connection connection = await proxy.ice_getConnectionAsync();
        connection.setCloseCallback(_ => connectionLost = true);

        // Act/Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.That(connectionLost, Is.True);
    }

    private static Communicator CreateCommunicator(int acmTimeout, bool enableIdleCheck)
    {
        string[] args = new string[]
        {
            $"--Ice.ACM.Timeout={acmTimeout}",
            $"--Ice.ACM.Heartbeat={(enableIdleCheck ? 3 : 1)}"
        };

        return Util.initialize(ref args);
    }
}
