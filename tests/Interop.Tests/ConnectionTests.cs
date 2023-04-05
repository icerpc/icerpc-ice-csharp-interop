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

            yield return new TestCaseData(new ObjectNotExistException(), StatusCode.ServiceNotFound, null);
            yield return new TestCaseData(new FacetNotExistException(), StatusCode.ServiceNotFound, null);
            yield return new TestCaseData(new OperationNotExistException(), StatusCode.OperationNotFound, null);

            yield return new TestCaseData(
                new UnknownException(errorMessage),
                StatusCode.UnhandledException,
                errorMessage);

            yield return new TestCaseData(
                new UnknownLocalException(errorMessage),
                StatusCode.UnhandledException,
                errorMessage);

            yield return new TestCaseData(
                new UnknownUserException(errorMessage),
                StatusCode.UnhandledException,
                errorMessage);
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
            return new OutgoingResponse(request);
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
        _ = await proxy.ice_invokeAsync(
            operation: "op",
            mode: OperationMode.Normal,
            expectedPayload.CreateEncapsulation());

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
                new OutgoingResponse(request) { Payload = payload } :
                new OutgoingResponse(request, StatusCode.ApplicationError, "") { Payload = payload });
        });

        await using var server = new Server(dispatcher, new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using Communicator communicator = Util.initialize();
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        // Act
        Object_Ice_invokeResult result = await proxy.ice_invokeAsync(
            operation: "op",
            mode: OperationMode.Normal,
            Array.Empty<byte>().CreateEncapsulation());

        // Assert
        Assert.That(result.outEncaps[6..], Is.EqualTo(expectedPayload));
        Assert.That(result.returnValue, Is.EqualTo(success));
    }

    /// <summary>Sends a failure from IceRPC to Ice.</summary>
    [TestCase(StatusCode.ServiceNotFound, typeof(ObjectNotExistException))]
    [TestCase(StatusCode.OperationNotFound, typeof(OperationNotExistException))]
    [TestCase(StatusCode.UnhandledException, typeof(UnknownException))]
    [TestCase(StatusCode.DeadlineExpired, typeof(UnknownException))]
    [TestCase((StatusCode)999, typeof(UnknownException))]
    public async Task Send_failure_from_IceRPC_to_Ice(StatusCode statusCode, Type exceptionType)
    {
        var dispatcher = new InlineDispatcher((request, cancellationToken) =>
            new(new OutgoingResponse(request, statusCode, "error message")));
        await using var server = new Server(dispatcher, new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using Communicator communicator = Util.initialize();
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        // Act/Assert
        Assert.That(
            async() => await proxy.ice_invokeAsync(
                operation: "op",
                mode: OperationMode.Normal,
                Array.Empty<byte>().CreateEncapsulation()),
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
                (inParams, current) =>
                {
                    tcs.SetResult(inParams[6..]);
                    return (true, Array.Empty<byte>().CreateEncapsulation());
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
        adapter.addDefaultServant(
            new InlineBlobject((inParams, current) => (success, expectedPayload.CreateEncapsulation())),
            "");
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        using var request = new OutgoingRequest(new ServiceAddress(new Uri("ice:/hello")));

        // Act
        IncomingResponse response = await clientConnection.InvokeAsync(request);
        ReadResult readResult = await response.Payload.ReadAtLeastAsync(expectedPayload.Length + 1);

        // Assert
        Assert.That(readResult.Buffer.ToArray(), Is.EqualTo(expectedPayload));
        Assert.That(response.StatusCode, Is.EqualTo(success ? StatusCode.Success : StatusCode.ApplicationError));
    }

    /// <summary>Sends a failure from Ice to IceRPC.</summary>
    [Test, TestCaseSource(nameof(SystemExceptionToStatusCode))]
    public async Task Send_failure_from_Ice_to_IceRPC(
        Ice.Exception systemException,
        StatusCode statusCode,
        string? errorMessage)
    {
        // Arrange
        string[] args = new string[] { "--Ice.Warn.Dispatch=0" };
        using Communicator communicator = Util.initialize(ref args);
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.addDefaultServant(
            new InlineBlobject((inParams, current) => throw systemException),
            "");
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
