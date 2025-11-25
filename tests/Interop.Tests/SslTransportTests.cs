// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using NUnit.Framework;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Interop.Tests;

[Parallelizable(scope: ParallelScope.All)]
internal class SslTransportTests
{
    [Test]
    public async Task Create_ssl_connection_from_Ice_to_IceRPC()
    {
        // Arrange
        using X509Certificate2 caCertificate = X509CertificateLoader.LoadCertificateFromFile("cacert.der");
        using X509Certificate2 serverCertificate = X509CertificateLoader.LoadPkcs12FromFile("server.p12", "password");
        using X509Certificate2 clientCertificate = X509CertificateLoader.LoadPkcs12FromFile("client.p12", "password");
        X509Certificate2? serverPeerCertificate = null;
        X509Certificate2? clientPeerCertificate = null;
        await using var server = new Server(
            new InlineDispatcher((request, cancellationToken) => throw new NotImplementedException()),
            new Uri("ice://127.0.0.1:0"),
            serverAuthenticationOptions: new SslServerAuthenticationOptions
            {
                ServerCertificate = serverCertificate,
                RemoteCertificateValidationCallback =
                    (sender, certificate, chain, errors) =>
                    {
                        serverPeerCertificate = certificate as X509Certificate2;
                        return certificate?.Issuer == clientCertificate.Issuer;
                    },
                ClientCertificateRequired = true
            });

        ServerAddress serverAddress = server.Listen();

        var initData = new Ice.InitializationData()
        {
            properties = new Properties(),
            clientAuthenticationOptions = new SslClientAuthenticationOptions
            {
                ClientCertificates = [clientCertificate],
                RemoteCertificateValidationCallback =
                (sender, certificate, chain, errors) =>
                {
                    clientPeerCertificate = certificate as X509Certificate2;
                    return certificate?.Issuer == serverCertificate.Issuer;
                }
            }
        };
        using var communicator = new Communicator(initData);
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress with { Transport = "ssl" });

        // Act
        Connection? connection = await proxy.ice_getConnectionAsync();
        Assert.That(connection, Is.Not.Null);

        // Assert
        Assert.That(clientPeerCertificate, Is.EqualTo(serverCertificate));
        Assert.That(serverPeerCertificate, Is.EqualTo(clientCertificate));
    }

    [Test]
    public async Task Create_ssl_connection_from_IceRPC_to_Ice()
    {
        // Arrange

        using X509Certificate2 caCertificate = X509CertificateLoader.LoadCertificateFromFile("cacert.der");
        using X509Certificate2 serverCertificate = X509CertificateLoader.LoadPkcs12FromFile("server.p12", "password");
        using X509Certificate2 clientCertificate = X509CertificateLoader.LoadPkcs12FromFile("client.p12", "password");
        X509Certificate2? clientPeerCertificate = null;
        X509Certificate2? serverPeerCertificate = null;

        using var communicator = new Communicator();
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints(
            "test",
            "ssl -h 127.0.0.1 -p 0",
            serverAuthenticationOptions: new SslServerAuthenticationOptions
            {
                ServerCertificate = serverCertificate,
                RemoteCertificateValidationCallback =
                    (sender, certificate, chain, errors) =>
                    {
                        serverPeerCertificate = certificate as X509Certificate2;
                        return certificate?.Issuer == clientCertificate.Issuer;
                    },
                ClientCertificateRequired = true
            });
        adapter.activate();

        await using var clientConnection = new ClientConnection(
            adapter.GetFirstServerAddress(),
            clientAuthenticationOptions: new SslClientAuthenticationOptions
            {
                ClientCertificates = [clientCertificate],
                RemoteCertificateValidationCallback =
                    (sender, certificate, chain, errors) =>
                    {
                        clientPeerCertificate = certificate as X509Certificate2;
                        return certificate?.Issuer == serverCertificate.Issuer;
                    }
            });

        // Act
        await clientConnection.ConnectAsync();

        // Assert
        Assert.That(serverPeerCertificate, Is.EqualTo(clientCertificate));
        Assert.That(clientPeerCertificate, Is.EqualTo(serverCertificate));
    }
}
