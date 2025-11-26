// Copyright (c) ZeroC, Inc.

using IceRpc;

namespace Interop.Tests;

/// <summary>Provides extension method for the ObjectAdapter interface.</summary>
internal static class ObjectAdapterExtensions
{
    /// <summary>Returns the first server address of this object adapter (after activation).</summary>
    internal static ServerAddress GetFirstServerAddress(this Ice.ObjectAdapter adapter)
    {
        Ice.EndpointInfo info = adapter.getEndpoints()[0].getInfo();
        Ice.IPEndpointInfo? ipInfo = info as Ice.IPEndpointInfo ?? info.underlying as Ice.IPEndpointInfo;
        if (ipInfo is not null)
        {
            return new ServerAddress(Protocol.Ice)
            {
                Host = ipInfo.host,
                Port = (ushort)ipInfo.port
            };
        }
        else
        {
            throw new NotImplementedException();
        }
    }
}
