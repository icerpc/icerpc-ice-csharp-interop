// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using IceRpc.Slice;
using NUnit.Framework;
using System.Buffers;
using System.IO.Pipelines;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
public class ServiceAddressTests
{
    /// <summary>Encodes a proxy with Ice, then decodes it as a service address, then encodes this service address with
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
    public void Proxy_to_service_address_and_back(string iceString)
    {
        // Arrange

        // We need to load the IceSSL plugin to parse ssl and wss endpoints.
        string[] args = new string[] { "--Ice.Plugin.IceSSL=IceSSL:IceSSL.PluginFactory" };
        using Communicator communicator = Util.initialize(ref args);
        ObjectPrx iceProxy = communicator.stringToProxy(iceString);

        byte[] buffer = EncodeIceProxy(iceProxy);
        var decoder = new SliceDecoder(buffer, SliceEncoding.Slice1);
        ServiceAddress serviceAddress = decoder.DecodeServiceAddress();

        var inputStream = new InputStream(communicator, EncodeServiceAddress(serviceAddress));

        // Act
        ObjectPrx newIceProxy = inputStream.readProxy();

        // Assert
        Assert.That(decoder.Consumed, Is.EqualTo(buffer.Length));
        Assert.That(newIceProxy.ice_getIdentity(), Is.EqualTo(iceProxy.ice_getIdentity()));
        Assert.That(newIceProxy.ice_getFacet(), Is.EqualTo(iceProxy.ice_getFacet()));
        Assert.That(newIceProxy.ice_getEndpoints(), Is.EqualTo(iceProxy.ice_getEndpoints()));
        Assert.That(newIceProxy.ice_getAdapterId(), Is.EqualTo(iceProxy.ice_getAdapterId()));

        Assert.That(newIceProxy.ice_isSecure(), Is.False);
        Assert.That(newIceProxy.ice_isTwoway(), Is.True);
        Assert.That(newIceProxy.ice_getEncodingVersion(), Is.EqualTo(Util.Encoding_1_1));
    }

    /// <summary>Encodes a service address with IceRpc, then decodes it as a proxy, then encodes this proxy with
    /// Ice and decodes it with IceRpc, then verifies they are the same.</summary>
    /// <remarks>Transports unknown to IceRpc such as udp, ws, xyz are encoded with transport code 0 (Uri); they are
    /// opaque for Ice.</remarks>
    [TestCase("ice://127.0.0.1:10000/hello?transport=tcp")]
    [TestCase("ice://[::1]/hello?transport=ssl")]
    [TestCase("ice://127.0.0.1:10000/hello?transport=ssl&z&t=10000")] // any t value except 60000 should work
    [TestCase("ice://127.0.0.1:10000/hello?transport=udp&foo=bar")] // opaque for Ice
    [TestCase("ice://127.0.0.1:10000/hello?transport=tcp&alt-server=foo?transport=wss&alt-server=bar?transport=xyz")]
    [TestCase("ice:/hello?adapter-id=foo#facet")]
    [TestCase("icerpc://127.0.0.1:10000/hello?transport=tcp")]
    [TestCase("icerpc:/cat/hello?adapter-id=foo")]
    public void Service_address_to_proxy_and_back(ServiceAddress serviceAddress)
    {
        // Arrange

        // We need to load the IceSSL plugin to decode ssl endpoints.
        string[] args = new string[] { "--Ice.Plugin.IceSSL=IceSSL:IceSSL.PluginFactory" };
        using Communicator communicator = Util.initialize(ref args);

        var inputStream = new InputStream(communicator, EncodeServiceAddress(serviceAddress));
        ObjectPrx iceProxy = inputStream.readProxy();

        byte[] buffer = EncodeIceProxy(iceProxy);
        var decoder = new SliceDecoder(buffer, SliceEncoding.Slice1);

        // Act
        ServiceAddress newServiceAddress = decoder.DecodeServiceAddress();

        // Assert
        Assert.That(decoder.Consumed, Is.EqualTo(buffer.Length));
        Assert.That(newServiceAddress, Is.EqualTo(serviceAddress));
    }

    private static byte[] EncodeIceProxy(ObjectPrx iceProxy)
    {
        var outputStream = new OutputStream(iceProxy.ice_getCommunicator());
        outputStream.writeProxy(iceProxy);
        return outputStream.finished();
    }

    private static byte[] EncodeServiceAddress(ServiceAddress serviceAddress)
    {
        var pipe = new Pipe();
        var encoder = new SliceEncoder(pipe.Writer, SliceEncoding.Slice1);
        encoder.EncodeServiceAddress(serviceAddress);
        pipe.Writer.Complete();
        pipe.Reader.TryRead(out ReadResult readResult);

        byte[] result = readResult.Buffer.ToArray();
        pipe.Reader.Complete();
        return result;
    }
}
