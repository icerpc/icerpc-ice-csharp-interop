// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using NUnit.Framework;
using System.Buffers;
using System.IO.Pipelines;

namespace Interop.Tests;

[Parallelizable(scope: ParallelScope.All)]
public class ConnectionTests
{
    private static IEnumerable<TestCaseData> SystemExceptionToStatusCode
    {
        get
        {
            string errorMessage = "super custom error message";

            yield return new TestCaseData(new ObjectNotExistException(), StatusCode.NotFound, null)
                .SetName("Send_failure_from_Ice_to_IceRPC(ObjectNotExistException)");
            yield return new TestCaseData(new FacetNotExistException(), StatusCode.NotFound, null)
                .SetName("Send_failure_from_Ice_to_IceRPC(FacetNotExistException)");
            yield return new TestCaseData(new OperationNotExistException(), StatusCode.NotImplemented, null)
                .SetName("Send_failure_from_Ice_to_IceRPC(OperationNotExistException)");

            yield return new TestCaseData(
                new UnknownException(errorMessage),
                StatusCode.InternalError,
                errorMessage)
                .SetName("Send_failure_from_Ice_to_IceRPC(UnknownException)");

            yield return new TestCaseData(
                new UnknownLocalException(errorMessage),
                StatusCode.InternalError,
                errorMessage)
                .SetName("Send_failure_from_Ice_to_IceRPC(UnknownLocalException)");

            yield return new TestCaseData(
                new UnknownUserException(errorMessage),
                StatusCode.InternalError,
                errorMessage)
                .SetName("Send_failure_from_Ice_to_IceRPC(UnknownUserException)");
        }
    }

    [Test]
    public async Task Establish_connection_from_IceRpc_to_Ice()
    {
        using Communicator communicator = Util.initialize();
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.activate();
        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());

        // Act/Assert
        Assert.That(async () => await clientConnection.ConnectAsync(), Throws.Nothing);
    }

    /// <summary>Verifies an IceRPC->Ice connection remains alive after idling for > idle timeout. That's because the
    /// both sides use the same idle timeout / enable idle check mechanism (as of Ice 3.8 / IceRPC 0.4).
    /// </summary>
    [Test]
    public async Task Idle_connection_to_Ice_server_remains_alive()
    {
        const int idleTimeout = 3; // in seconds

        string[] args = [ $"--Ice.Connection.Server.IdleTimeout={idleTimeout}" ];
        using Communicator communicator = Util.initialize(ref args);
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.activate();

        var clientConnectionFactory = new ClientProtocolConnectionFactory(
            new ConnectionOptions { IceIdleTimeout = TimeSpan.FromSeconds(idleTimeout) });

        await using IProtocolConnection clientConnection =
            clientConnectionFactory.CreateConnection(adapter.GetFirstServerAddress());

        Task shutdownRequested = (await clientConnection.ConnectAsync()).ShutdownRequested;

        // Act/Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.That(shutdownRequested.IsCompleted, Is.False);
    }

    /// <summary>Verifies an Ice->IceRPC connection remains alive after idling for > idle timeout.
    /// </summary>
    [Test]
    public async Task Idle_connection_to_IceRpc_server_remains_alive()
    {
        const int idleTimeout = 3; // in seconds

        await using var server = new Server(
            new ServerOptions
            {
                ConnectionOptions = new ConnectionOptions
                {
                    Dispatcher = new InlineDispatcher(
                        (request, cancellationToken) => new(new IceRpc.OutgoingResponse(request))),
                    IceIdleTimeout = TimeSpan.FromSeconds(idleTimeout)
                },
                ServerAddress = new ServerAddress(new Uri("ice://127.0.0.1:0")),
            });

        ServerAddress serverAddress = server.Listen();

        string[] args = [ $"--Ice.Connection.Client.IdleTimeout={idleTimeout}" ];
        using Communicator communicator = Util.initialize(ref args);
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        bool connectionClosed = false;
        Connection? connection = await proxy.ice_getConnectionAsync();
        connection!.setCloseCallback(_ => connectionClosed = true);

        // Act/Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.That(connectionClosed, Is.False);
    }

    /// <summary>Verifies an IceRPC->Ice connection is shut down when inactive. That's because the inactivity timeout
    /// shuts down the connection.</summary>
    [Test]
    public async Task Inactive_connection_to_Ice_is_shutdown([Values] bool byIceRpc)
    {
        const int idleTimeout = 1; // in seconds
        const int inactivityTimeout = 3; // in seconds

        string[] args =
        [
            $"--Ice.Connection.Server.IdleTimeout={idleTimeout}",
            $"--Ice.Connection.Server.InactivityTimeout={(byIceRpc ? 0 : inactivityTimeout)}"
        ];

        using Communicator communicator = Util.initialize(ref args);
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.activate();

        var clientConnectionFactory = new ClientProtocolConnectionFactory(
            new ConnectionOptions
            {
                IceIdleTimeout = TimeSpan.FromSeconds(idleTimeout),
                InactivityTimeout = byIceRpc ? TimeSpan.FromSeconds(inactivityTimeout) : Timeout.InfiniteTimeSpan
            });

        await using IProtocolConnection clientConnection =
            clientConnectionFactory.CreateConnection(adapter.GetFirstServerAddress());

        Task shutdownRequested = (await clientConnection.ConnectAsync()).ShutdownRequested;

        // Act/Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.That(shutdownRequested.IsCompleted, Is.True);
    }

    /// <summary>Verifies an Ice->IceRPC connection is shut down when inactive.</summary>
    [Test]
    public async Task Inactive_connection_to_IceRpc_is_shutdown([Values] bool byIceRpc)
    {
        const int idleTimeout = 1; // in seconds
        const int inactivityTimeout = 3; // in seconds

        await using var server = new Server(
            new ServerOptions
            {
                ConnectionOptions = new ConnectionOptions
                {
                    Dispatcher = new InlineDispatcher(
                        (request, cancellationToken) => new(new IceRpc.OutgoingResponse(request))),
                    IceIdleTimeout = TimeSpan.FromSeconds(idleTimeout),
                    InactivityTimeout = byIceRpc ? TimeSpan.FromSeconds(inactivityTimeout) : Timeout.InfiniteTimeSpan,
                },
                ServerAddress = new ServerAddress(new Uri("ice://127.0.0.1:0")),
            });

        ServerAddress serverAddress = server.Listen();

        string[] args =
        [
            $"--Ice.Connection.Client.IdleTimeout={idleTimeout}",
            $"--Ice.Connection.Client.InactivityTimeout={(byIceRpc ? 0 : inactivityTimeout)}"
        ];
        using var communicator = Util.initialize(ref args);
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        bool connectionClosed = false;
        Connection? connection = await proxy.ice_getConnectionAsync();
        connection!.setCloseCallback(_ => connectionClosed = true);

        // Act/Assert
        await Task.Delay(TimeSpan.FromSeconds(5));
        Assert.That(connectionClosed, Is.True);
    }

    /// <summary>Sends a request from Ice to IceRPC.</summary>
    [Test]
    public async Task Send_request_from_Ice_to_IceRpc([Values] bool oneway)
    {
        // Arrange
        byte[] expectedPayload = Enumerable.Range(0, 4096).Select(p => (byte)p).ToArray();
        var tcs = new TaskCompletionSource<byte[]>();
        var dispatcher = new InlineDispatcher(async (request, cancellationToken) =>
        {
            ReadResult readResult = await request.Payload.ReadAtLeastAsync(
                expectedPayload.Length + 1,
                cancellationToken);
            tcs.SetResult(readResult.Buffer.ToArray());
            request.Payload.AdvanceTo(readResult.Buffer.End);
            return new IceRpc.OutgoingResponse(request);
        });
        await using var server = new Server(dispatcher, new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using Communicator communicator = Util.initialize();
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);
        if (oneway)
        {
            proxy = proxy.ice_oneway();
        }

        // Act
        _ = await proxy.IceInvokeAsync(operation: "op", mode: OperationMode.Normal, expectedPayload);

        // Assert
        Assert.That(async () => await tcs.Task, Is.EqualTo(expectedPayload));
    }

    /// <summary>Sends a response from IceRPC to Ice. It can be either a success or an application error aka user
    /// exception.</summary>
    [Test]
    public async Task Send_response_from_IceRPC_to_Ice([Values] bool success)
    {
        // Arrange
        byte[] expectedPayload = Enumerable.Range(0, 4096).Select(p => (byte)p).ToArray();
        var dispatcher = new InlineDispatcher((request, cancellationToken) =>
        {
            var payload = PipeReader.Create(new ReadOnlySequence<byte>(expectedPayload));
            return new(success ?
                new IceRpc.OutgoingResponse(request) { Payload = payload } :
                new IceRpc.OutgoingResponse(request, StatusCode.ApplicationError, "") { Payload = payload });
        });

        await using var server = new Server(dispatcher, new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using Communicator communicator = Util.initialize();
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        // Act
        Object_Ice_invokeResult result = await proxy.IceInvokeAsync(
            operation: "op",
            mode: OperationMode.Normal,
            []);

        // Assert
        Assert.That(result.outEncaps[6..], Is.EqualTo(expectedPayload));
        Assert.That(result.returnValue, Is.EqualTo(success));
    }

    /// <summary>Sends a failure from IceRPC to Ice.</summary>
    [TestCase(StatusCode.NotFound, typeof(ObjectNotExistException))]
    [TestCase(StatusCode.NotImplemented, typeof(OperationNotExistException))]
    [TestCase(StatusCode.InternalError, typeof(UnknownException))]
    [TestCase(StatusCode.DeadlineExceeded, typeof(UnknownException))]
    [TestCase((StatusCode)999, typeof(UnknownException))]
    public async Task Send_failure_from_IceRPC_to_Ice(StatusCode statusCode, Type exceptionType)
    {
        var dispatcher = new InlineDispatcher((request, cancellationToken) =>
            new(new IceRpc.OutgoingResponse(request, statusCode, "error message")));
        await using var server = new Server(dispatcher, new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using Communicator communicator = Util.initialize();
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        // Act/Assert
        Assert.That(
            async () => await proxy.IceInvokeAsync(operation: "op", mode: OperationMode.Normal, []),
            Throws.InstanceOf(exceptionType));
    }

    /// <summary>Sends a request from IceRPC to Ice.</summary>
    [Test]
    public async Task Send_request_from_IceRPC_to_Ice([Values] bool oneway)
    {
        // Arrange
        byte[] expectedPayload = Enumerable.Range(0, 4096).Select(p => (byte)p).ToArray();
        var tcs = new TaskCompletionSource<byte[]>();

        using Communicator communicator = Util.initialize();
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.addDefaultServant(
            new InlineBlobject(
                (payload, current) =>
                {
                    tcs.SetResult(payload);
                    return (true, Array.Empty<byte>());
                }),
            "");
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        using var request = new OutgoingRequest(new ServiceAddress(new Uri("ice:/hello")))
        {
            IsOneway = oneway,
            Payload = PipeReader.Create(new ReadOnlySequence<byte>(expectedPayload))
        };

        // Act
        _ = await clientConnection.InvokeAsync(request);

        // Assert
        Assert.That(async () => await tcs.Task, Is.EqualTo(expectedPayload));
    }

    /// <summary>Sends a response from Ice to IceRPC. It can be either a success or an application error aka user
    /// exception.</summary>
    [Test]
    public async Task Send_response_from_Ice_to_IceRPC([Values] bool success)
    {
        // Arrange
        byte[] expectedPayload = Enumerable.Range(0, 4096).Select(p => (byte)p).ToArray();

        using Communicator communicator = Util.initialize();
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.addDefaultServant(new InlineBlobject((payload, current) => (success, expectedPayload)), "");
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        using var request = new OutgoingRequest(new ServiceAddress(new Uri("ice:/hello")));

        // Act
        IncomingResponse response = await clientConnection.InvokeAsync(request);
        ReadResult readResult = await response.Payload.ReadAtLeastAsync(expectedPayload.Length + 1);

        // Assert
        Assert.That(readResult.Buffer.ToArray(), Is.EqualTo(expectedPayload));
        Assert.That(response.StatusCode, Is.EqualTo(success ? StatusCode.Ok : StatusCode.ApplicationError));
    }

    /// <summary>Sends a failure from Ice to IceRPC.</summary>
    [Test, TestCaseSource(nameof(SystemExceptionToStatusCode))]
    public async Task Send_failure_from_Ice_to_IceRPC(
        Ice.Exception systemException,
        StatusCode statusCode,
        string? errorMessage)
    {
        // Arrange
        string[] args = ["--Ice.Warn.Dispatch=0"];
        using Communicator communicator = Util.initialize(ref args);
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.addDefaultServant(new InlineBlobject((payload, current) => throw systemException), "");
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        using var request = new OutgoingRequest(new ServiceAddress(new Uri("ice:/hello")));

        // Act
        IncomingResponse response = await clientConnection.InvokeAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(statusCode));
        if (errorMessage is not null)
        {
            Assert.That(response.ErrorMessage, Is.EqualTo(errorMessage));
        }
    }
}
