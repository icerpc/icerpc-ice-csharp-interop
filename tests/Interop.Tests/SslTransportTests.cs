// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using IceRpc.Ice;
using NUnit.Framework;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Interop.Tests;

[Parallelizable(scope: ParallelScope.All)]
public class SslTransportTests
{
    [Test]
    public async Task Send_request_over_ssl_from_IceRpc_to_Ice()
    {
        // Arrange
        using var caCertificate = new X509Certificate2("../../../../../certs/cacert.der");
        using var serverCertificate = new X509Certificate2("../../../../../certs/server.p12", "password");
        using var clientCertificate = new X509Certificate2("../../../../../certs/client.p12", "password");
        var dispatcher = new IceRpc.Slice.Service();
        X509Certificate2? peerCertificate = null;
        await using var server = new Server(
            dispatcher,
            new Uri("ice://127.0.0.1:0"),
            serverAuthenticationOptions: new SslServerAuthenticationOptions
            {
                ServerCertificate = serverCertificate,
                RemoteCertificateValidationCallback =
                    (sender, certificate, chain, errors) =>
                    {
                        peerCertificate = certificate as X509Certificate2;
                        return certificate?.Issuer == clientCertificate.Issuer;
                    },
                ClientCertificateRequired = true
            });

        ServerAddress serverAddress = server.Listen();
        // Load and configure the IceSSL plugin.
        string[] args = new string[] 
        { 
            "--Ice.Plugin.IceSSL=IceSSL:IceSSL.PluginFactory",
            "--IceSSL.DefaultDir=../../../../../certs/",
            "--IceSSL.CertFile=client.p12",
            "--IceSSL.CAs=cacert.der",
            "--IceSSL.Password=password",
        };
        using Communicator communicator = Util.initialize(ref args);
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress with { Transport = "ssl" });

        // Act
        Connection connection = await proxy.ice_getConnectionAsync();

        // Assert
        var info = connection.getInfo() as IceSSL.ConnectionInfo;
        Assert.That(info, Is.Not.Null);
        Assert.That(info.verified, Is.True);
        Assert.That(info.certs[0], Is.EqualTo(serverCertificate));
        Assert.That(info.certs[1], Is.EqualTo(caCertificate));

        Assert.That(peerCertificate, Is.EqualTo(clientCertificate));
    }

    [Test]
    public async Task Send_request_from_IceRPC_to_Ice()
    {
        // Arrange
        using var caCertificate = new X509Certificate2("../../../../../certs/cacert.der");
        using var serverCertificate = new X509Certificate2("../../../../../certs/server.p12", "password");
        using var clientCertificate = new X509Certificate2("../../../../../certs/client.p12", "password");

        // Load and configure the IceSSL plugin.
        string[] args = new string[]
        {
            "--Ice.Plugin.IceSSL=IceSSL:IceSSL.PluginFactory",
            "--IceSSL.DefaultDir=../../../../../certs/",
            "--IceSSL.CertFile=server.p12",
            "--IceSSL.CAs=cacert.der",
            "--IceSSL.Password=password",
        };

        Connection? peerConnection = null;
        X509Certificate2? peerCertificate = null;
        using Communicator communicator = Util.initialize(ref args);
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "ssl -h 127.0.0.1 -p 0");
        adapter.addDefaultServant(
            new InlineBlobject(
                (payload, current) =>
                {
                    peerConnection = current?.con;
                    return (true, Array.Empty<byte>());
                }), "");
        adapter.activate();

        await using var clientConnection = new ClientConnection(
            adapter.GetFirstServerAddress(),
            clientAuthenticationOptions: new SslClientAuthenticationOptions
            {
                ClientCertificates = new X509CertificateCollection
                {
                    clientCertificate
                },
                RemoteCertificateValidationCallback =
                    (sender, certificate, chain, errors) =>
                    {
                        peerCertificate = certificate as X509Certificate2;
                        return certificate?.Issuer == serverCertificate.Issuer;
                    }
            });

        var objectProxy = new IceObjectProxy(clientConnection, new ServiceAddress(new Uri("ice:///hello")));
        // Act
        await objectProxy.IcePingAsync();

        // Assert
        var info = peerConnection?.getInfo() as IceSSL.ConnectionInfo;
        Assert.That(info, Is.Not.Null);
        Assert.That(info.verified, Is.True);
        Assert.That(info.certs[0], Is.EqualTo(clientCertificate));
        Assert.That(info.certs[1], Is.EqualTo(caCertificate));

        Assert.That(peerCertificate, Is.EqualTo(serverCertificate));
    }
}
