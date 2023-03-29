// Copyright (c) ZeroC, Inc.

using IceRpc;

namespace Interop.Tests;

/// <summary>Provides extension method for the ObjectAdapter interface.</summary>
public static class ObjectAdapterExtensions
{
    /// <summary>Returns the first server address of this object adapter (after activation).</summary>
    public static ServerAddress GetFirstServerAddress(this Ice.ObjectAdapter adapter)
    {
        Ice.EndpointInfo info = adapter.getEndpoints()[0].getInfo();

        if (info is Ice.TCPEndpointInfo tcpInfo)
        {
            return new ServerAddress(Protocol.Ice)
            {
                Host = tcpInfo.host,
                Port = (ushort)tcpInfo.port
            };
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
