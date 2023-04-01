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
            CreateEncapsulation(expectedPayload));

        // Assert
        Assert.That(async () => await tcs.Task, Is.EqualTo(expectedPayload));
    }

    /// <summary>Sends a response from IceRPC to Ice.</summary>
    [Test]
    public async Task Send_response_from_IceRPC_to_Ice()
    {
        // Arrange
        byte[] expectedPayload = Enumerable.Range(0, 4096).Select(p => (byte)p).ToArray();
        var dispatcher = new InlineDispatcher((request, cancellationToken) =>
            new(
                new OutgoingResponse(request)
                {
                    Payload = PipeReader.Create(new ReadOnlySequence<byte>(expectedPayload))
                }));
        await using var server = new Server(dispatcher, new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using Communicator communicator = Util.initialize();
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress);

        // Act
        Object_Ice_invokeResult result = await proxy.ice_invokeAsync(
            operation: "op",
            mode: OperationMode.Normal,
            CreateEncapsulation(Array.Empty<byte>()));

        // Assert
        Assert.That(result.outEncaps[6..], Is.EqualTo(expectedPayload));
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
                    return (true,  CreateEncapsulation(Array.Empty<byte>()));
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

    /// <summary>Sends a response from Ice to IceRPC.</summary>
    [Test]
    public async Task Send_response_from_Ice_to_IceRPC()
    {
        // Arrange
        byte[] expectedPayload = Enumerable.Range(0, 4096).Select(p => (byte)p).ToArray();

        using Communicator communicator = Util.initialize();
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.addDefaultServant(
            new InlineBlobject((inParams, current) => (true, CreateEncapsulation(expectedPayload))),
            "");
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        using var request = new OutgoingRequest(new ServiceAddress(new Uri("ice:/hello")));

        // Act
        IncomingResponse response = await clientConnection.InvokeAsync(request);
        ReadResult readResult = await response.Payload.ReadAtLeastAsync(expectedPayload.Length + 1);

        // Assert
        Assert.That(readResult.Buffer.ToArray(), Is.EqualTo(expectedPayload));
    }

    private static byte[] CreateEncapsulation(byte[] payload)
    {
        var outputStream = new OutputStream();
        outputStream.startEncapsulation();
        outputStream.writeBlob(payload);
        outputStream.endEncapsulation();
        return outputStream.finished();
    }

    private class InlineBlobject : Blobject
    {
        private readonly Func<byte[], Current, (bool Ok, byte[] OutParams)> _func;

        public override bool ice_invoke(byte[] inParams, out byte[] outParams, Current current)
        {
            (bool ok, outParams) = _func(inParams, current);
            return ok;
        }

        internal InlineBlobject(Func<byte[], Current, (bool Ok, byte[] OutParams)> func) => _func = func;
    }
}
