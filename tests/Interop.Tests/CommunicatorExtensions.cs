// Copyright (c) ZeroC, Inc.

using IceRpc;

namespace Interop.Tests;

/// <summary>Provides extension method for the Communicator interface.</summary>
internal static class CommunicatorExtensions
{
    internal static Ice.ObjectPrx CreateObjectPrx(
        this Ice.Communicator communicator,
        string identity,
        ServerAddress serverAddress)
    {
        string transport = serverAddress.Transport ?? "default";
        return Ice.ObjectPrxHelper.createProxy(communicator, $"{identity}:{transport} -h {serverAddress.Host} -p {serverAddress.Port}");
    }
}
