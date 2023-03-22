// Copyright (c) ZeroC, Inc.

using NUnit.Framework;
using System.Diagnostics;

namespace IceRpc.Tests.Slice.Identifiers;

[Parallelizable(scope: ParallelScope.All)]
public class ConnectionTests
{
    [Test]
    public async Task IceRPC_client_ice_server()
    {
        using Ice.Communicator communicator = Ice.Util.initialize();
        var adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.activate();
        var endpointInfo = GetTCPEndpointInfo(adapter.getEndpoints()[0].getInfo());
        Debug.Assert(endpointInfo is not null);

        await using var clientConnection = new ClientConnection(new Uri($"ice://127.0.0.1:{endpointInfo.port}"));

        // Act/Assert
        Assert.That(async () => await clientConnection.ConnectAsync(default), Throws.Nothing);
    }

    private static Ice.TCPEndpointInfo? GetTCPEndpointInfo(Ice.EndpointInfo info)
    {
        for (; info != null; info = info.underlying)
        {
            if (info is Ice.TCPEndpointInfo)
            {
                return info as Ice.TCPEndpointInfo;
            }
        }
        return null;
    }
}