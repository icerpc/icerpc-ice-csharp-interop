// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using IceRpc.Ice;
using IceRpc.Ice.Codec;
using NUnit.Framework;
using System.Buffers;
using System.IO.Pipelines;

namespace Interop.Tests.Generator;

[Parallelizable(scope: ParallelScope.All)]
internal class ProxyTests
{
    /// <summary>Encodes a proxy with Ice, then decodes it as a proxy, then encodes this proxy with
    /// IceRpc and decodes it with Ice, then verifies only proxy options are lost.</summary>
    [TestCase("hello:tcp -h 127.0.0.1 -p 10000 -z -t 10000")]
    [TestCase("hello:tcp -h \"::1\" -p 4061:ssl -h \"::2\" -p 4061")]
    [TestCase("hello -s -o:tcp -h 127.0.0.1 -p 10000")]
    [TestCase("hello -D -e 1.0:udp -h 127.0.0.1 -p 10000")]
    [TestCase("hello -o -s:udp -h 127.0.0.1 -p 10000:ws -h foo -p 10000 -t 10000:wss -h bar -p 10000 -z")]
    [TestCase("hello -p 1.0 -s:tcp -h 127.0.0.1 -p 10000")]
    [TestCase("hello:opaque -t 99 -e 1.1 -v JmljZXJwYzovLzEyNy4wLjAuMToxMDAwMD90cmFuc3BvcnQ9dGNw")]
    [TestCase("hello -f myFacet @adapter")]
    [TestCase("hello -f myFacet")]
    [TestCase("波/hello%20 -f 😍")]
    [TestCase("hello -p 2.0:opaque -t 0 -e 1.1 -v J2ljZXJwYzovLzEyNy4wLjAuMToxMDAwMC8/dHJhbnNwb3J0PXRjcA==")]
    [TestCase("cat/hello -p 2.0 @adapter")]
    [TestCase("cat/hello -p 2.0")]
    public void Proxy_to_proxy_and_back(string iceString)
    {
        // Arrange
        using var communicator = new Ice.Communicator();
        ObjectPrx iceProxy = ObjectPrxHelper.createProxy(communicator, iceString);

        byte[] buffer = EncodeIceProxy(communicator, iceProxy);
        var decoder = new IceDecoder(buffer);
        IceObjectProxy? proxy = decoder.DecodeIceObjectProxy();

        var inputStream = new InputStream(communicator, EncodeProxy(proxy));

        // Act
        ObjectPrx? newIceProxy = inputStream.readProxy();

        // Assert
        Assert.That(newIceProxy, Is.Not.Null);
        Assert.That(decoder.Consumed, Is.EqualTo(buffer.Length));
        Assert.That(newIceProxy.ice_getIdentity(), Is.EqualTo(iceProxy.ice_getIdentity()));
        Assert.That(newIceProxy.ice_getFacet(), Is.EqualTo(iceProxy.ice_getFacet()));
        Assert.That(newIceProxy.ice_getEndpoints(), Is.EqualTo(iceProxy.ice_getEndpoints()));
        Assert.That(newIceProxy.ice_getAdapterId(), Is.EqualTo(iceProxy.ice_getAdapterId()));

        Assert.That(newIceProxy.ice_isTwoway(), Is.True);
        Assert.That(newIceProxy.ice_getEncodingVersion(), Is.EqualTo(Util.Encoding_1_1));
    }

    /// <summary>Encodes a proxy with IceRpc, then decodes it as a proxy, then encodes this proxy with
    /// Ice and decodes it with IceRpc, then verifies they are the same.</summary>
    /// <remarks>Transports unknown to IceRpc such as udp, ws, xyz are encoded with transport code 0 (Uri); they are
    /// opaque for Ice.</remarks>
    [TestCase("ice://127.0.0.1:10000/hello?transport=tcp")]
    [TestCase("ice://[::1]/hello?transport=ssl")]
    // Any t value except 60000 should work; since 60000 is the default, it's equivalent (and gets replaced by) no
    // "t=value" at all.
    [TestCase("ice://127.0.0.1:10000/hello?transport=ssl&z&t=10000")]
    [TestCase("ice://127.0.0.1:10000/hello?transport=udp&foo=bar")]
    [TestCase("ice://127.0.0.1:10000/hello?transport=tcp&alt-server=foo?transport=wss&alt-server=bar?transport=xyz")]
    [TestCase("ice:/hello?adapter-id=foo#facet")]
    [TestCase("icerpc://127.0.0.1:10000/hello?transport=tcp")]
    [TestCase("icerpc:/cat/hello?adapter-id=foo")]
    public void Proxy_to_proxy_and_back_icerpc(string proxyString)
    {
        // Arrange
        using var communicator = new Ice.Communicator();

        var proxy = new IceObjectProxy(InvalidInvoker.Instance, new Uri(proxyString));

        var inputStream = new InputStream(communicator, EncodeProxy(proxy));
        ObjectPrx? iceProxy = inputStream.readProxy();

        byte[] buffer = EncodeIceProxy(communicator, iceProxy);
        var decoder = new IceDecoder(buffer);

        // Act
        IceObjectProxy? newProxy = decoder.DecodeIceObjectProxy();

        // Assert
        Assert.That(decoder.Consumed, Is.EqualTo(buffer.Length));
        Assert.That(newProxy, Is.EqualTo(proxy));
    }

    private static byte[] EncodeIceProxy(Communicator communicator, ObjectPrx? iceProxy)
    {
        var outputStream = new OutputStream(communicator);
        outputStream.writeProxy(iceProxy);
        return outputStream.finished();
    }

    private static byte[] EncodeProxy(IceObjectProxy? proxy)
    {
        var pipe = new Pipe();
        var encoder = new IceEncoder(pipe.Writer);
        encoder.EncodeIceObjectProxy(proxy);
        pipe.Writer.Complete();
        pipe.Reader.TryRead(out ReadResult readResult);

        byte[] result = readResult.Buffer.ToArray();
        pipe.Reader.Complete();
        return result;
    }
}
