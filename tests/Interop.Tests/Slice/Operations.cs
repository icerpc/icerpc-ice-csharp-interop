// Copyright (c) ZeroC, Inc.

using Ice;
using NUnit.Framework;
using System.Diagnostics;

namespace IceRpc.Tests.Slice.Identifiers;

[Parallelizable(scope: ParallelScope.All)]
public class OperationsTests
{
    [Test]
    public async Task Call_ice_operation_from_icerpc_client()
    {
        using Communicator communicator = Util.initialize();
        var adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        adapter.add(new IceOperations(), Util.stringToIdentity("operations"));
        adapter.activate();
        var endpointInfo = GetTCPEndpointInfo(adapter.getEndpoints()[0].getInfo());
        Debug.Assert(endpointInfo is not null);

        await using var clientConnection = new ClientConnection(new Uri($"ice://127.0.0.1:{endpointInfo.port}"));
        var operationsProxy = new Interop.Generated.OperationsProxy(clientConnection, new Uri("ice:///operations"));

        // Act/Assert
        Assert.That(async () => await operationsProxy.OpAsync(), Throws.Nothing);
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

    class IceOperations : Ice.Interop.Generated.Interop.OperationsDisp_
    {
        public override void op(Current current)
        {
        }
    }
}