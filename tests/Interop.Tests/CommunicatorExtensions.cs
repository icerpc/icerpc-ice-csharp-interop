// Copyright (c) ZeroC, Inc.

using IceRpc;

namespace Interop.Tests;

/// <summary>Provides extension method for the Communicator interface.</summary>
public static class CommunicatorExtensions
{
    public static Ice.ObjectPrx CreateObjectPrx(
        this Ice.Communicator communicator,
        string identity,
        ServerAddress serverAddress)
    {
        string transport = serverAddress.Transport ?? "default";
        return communicator.stringToProxy($"{identity}:{transport} -h {serverAddress.Host} -p {serverAddress.Port}");
    }
}
