// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using NUnit.Framework;

namespace Interop.Tests;

[Parallelizable(scope: ParallelScope.All)]
internal class AcmTests
{
    /// <summary>Verifies an IceRPC->Ice connection remains alive after idling for > idle timeout.</summary>
    [Test]
    public async Task Idle_connection_to_Ice_server_remains_alive()
    {
        var initData = new InitializationData
        {
            properties = new Properties()
        };
        initData.properties.setProperty("Ice.Connection.Server.IdleTimeout", "3");
        using var communicator = new Communicator(initData);
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.activate();

        var clientConnectionFactory = new ClientProtocolConnectionFactory(
            new ConnectionOptions { IceIdleTimeout = TimeSpan.FromSeconds(3) });

        await using IProtocolConnection clientConnection =
            clientConnectionFactory.CreateConnection(adapter.GetFirstServerAddress());

        Task shutdownRequested = (await clientConnection.ConnectAsync()).ShutdownRequested;

        // Act/Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.That(shutdownRequested.IsCompleted, Is.False);
    }

    /// <summary>Verifies an Ice->IceRPC connection remains alive after idling for > idle timeout.</summary>
    [Test]
    public async Task Idle_connection_to_IceRpc_server_remains_alive()
    {
        await using var server = new Server(
            new ServerOptions
            {
                ConnectionOptions = new ConnectionOptions
                {
                    Dispatcher = new InlineDispatcher(
                        (request, cancellationToken) => new(new IceRpc.OutgoingResponse(request))),
                    IceIdleTimeout = TimeSpan.FromSeconds(3)
                },
                ServerAddress = new ServerAddress(new Uri("ice://127.0.0.1:0")),
            });

        ServerAddress serverAddress = server.Listen();

        var initData = new InitializationData
        {
            properties = new Properties()
        };
        initData.properties.setProperty("Ice.Connection.Client.IdleTimeout", "3");
        using var communicator = new Communicator(initData);
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        bool connectionClosed = false;
        Connection? connection = await proxy.ice_getConnectionAsync();
        connection!.setCloseCallback(_ => connectionClosed = true);

        // Act/Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.That(connectionClosed, Is.False);
    }

    /// <summary>Verifies an IceRPC->Ice connection is shut down when inactive. That's because the InactivityTimeout
    /// shuts down the connection.</summary>
    [Test]
    public async Task Inactive_connection_to_Ice_is_shutdown()
    {
        var initData = new InitializationData
        {
            properties = new Properties()
        };
        initData.properties.setProperty("Ice.Connection.Server.IdleTimeout", "2");
        using var communicator = new Communicator(initData);
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.activate();

        var clientConnectionFactory = new ClientProtocolConnectionFactory(
            new ConnectionOptions
            {
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
                    Dispatcher = new InlineDispatcher(
                        (request, cancellationToken) => new(new IceRpc.OutgoingResponse(request))),
                    EnableIceIdleCheck = enableIdleCheck,
                    IceIdleTimeout = TimeSpan.FromSeconds(2),
                    InactivityTimeout = TimeSpan.FromSeconds(3)
                },
                ServerAddress = new ServerAddress(new Uri("ice://127.0.0.1:0")),
            });

        ServerAddress serverAddress = server.Listen();

        var initData = new Ice.InitializationData
        {
            properties = new Ice.Properties()
        };
        initData.properties.setProperty("Ice.Connection.Client.EnableIdleCheck", enableIdleCheck ? "1" : "0");
        initData.properties.setProperty("Ice.Connection.Client.IdleTimeout", "2");

        using var communicator = new Ice.Communicator(initData);
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        bool connectionClosed = false;
        Connection? connection = await proxy.ice_getConnectionAsync();
        connection!.setCloseCallback(_ => connectionClosed = true);

        // Act/Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.That(connectionClosed, Is.True);
    }
}
