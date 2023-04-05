// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using NUnit.Framework;
using System.Buffers;
using System.IO.Pipelines;

namespace Interop.Tests;

[Parallelizable(scope: ParallelScope.All)]
public class SslTransportTests
{
    [Test]
    public async Task Send_request_over_ssl_from_IceRpc_to_Ice()
    {

    }
}
