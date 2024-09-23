// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using NUnit.Framework;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Interop.Tests;

[Parallelizable(scope: ParallelScope.All)]
public class SslTransportTests
{
    [Test]
    public async Task Create_ssl_connection_from_Ice_to_IceRPC()
    {
        // Arrange
        using var caCertificate = X509CertificateLoader.LoadCertificateFromFile("cacert.der");

        using var serverCertificate = X509CertificateLoader.LoadPkcs12FromFile(
            "server.p12",
            password: null,
            keyStorageFlags: X509KeyStorageFlags.Exportable);

        using var clientCertificate = X509CertificateLoader.LoadPkcs12FromFile(
            "client.p12",
            password: null,
            keyStorageFlags: X509KeyStorageFlags.Exportable);

        X509Certificate2? validatedClientCertificate = null;
        X509Certificate2? validatedServerCertificate = null;
        await using var server = new Server(
            new InlineDispatcher((request, cancellationToken) => throw new NotImplementedException()),
            new Uri("ice://127.0.0.1:0"),
            serverAuthenticationOptions: new SslServerAuthenticationOptions
            {
                ServerCertificateContext = SslStreamCertificateContext.Create(
                    serverCertificate,
                    additionalCertificates: null),
                RemoteCertificateValidationCallback =
                    (sender, certificate, chain, errors) =>
                    {
                        validatedClientCertificate = certificate as X509Certificate2;
                        return certificate?.Issuer == clientCertificate.Issuer;
                    },
                ClientCertificateRequired = true
            });
        ServerAddress serverAddress = server.Listen();


        // Configure IceSSL
        var initData = new InitializationData();
        initData.properties = new Properties();
        initData.clientAuthenticationOptions = new SslClientAuthenticationOptions
        {
            ClientCertificates = [clientCertificate],
            RemoteCertificateValidationCallback =
                (sender, certificate, chain, errors) =>
                {
                    validatedServerCertificate = certificate as X509Certificate2;
                    return certificate?.Issuer == serverCertificate.Issuer;
                }
        };

        using Communicator communicator = Util.initialize(initData);
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress with { Transport = "ssl" });

        // Act
        Connection? connection = await proxy.ice_getConnectionAsync();

        // Assert
        Assert.That(validatedServerCertificate, Is.EqualTo(serverCertificate));
        Assert.That(validatedClientCertificate, Is.EqualTo(clientCertificate));
    }

    [Test]
    public async Task Create_ssl_connection_from_IceRPC_to_Ice()
    {
        // Arrange
        using var caCertificate = X509CertificateLoader.LoadCertificateFromFile("cacert.der");

        using var serverCertificate = X509CertificateLoader.LoadPkcs12FromFile(
            "server.p12",
            password: null,
            keyStorageFlags: X509KeyStorageFlags.Exportable);

        using var clientCertificate = X509CertificateLoader.LoadPkcs12FromFile(
            "client.p12",
            password: null,
            keyStorageFlags: X509KeyStorageFlags.Exportable);

        X509Certificate2? validatedClientCertificate = null;
        X509Certificate2? validatedServerCertificate = null;

        using Communicator communicator = Util.initialize();

        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints(
            "test",
            "ssl -h 127.0.0.1 -p 0",
            serverAuthenticationOptions: new SslServerAuthenticationOptions
            {
                ServerCertificateContext = SslStreamCertificateContext.Create(
                    serverCertificate,
                    additionalCertificates: null),
                RemoteCertificateValidationCallback =
                    (sender, certificate, chain, errors) =>
                    {
                        validatedClientCertificate = certificate as X509Certificate2;
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
                        validatedServerCertificate = certificate as X509Certificate2;
                        return certificate?.Issuer == serverCertificate.Issuer;
                    }
            });

        // Act
        await clientConnection.ConnectAsync();

        // Assert
        Assert.That(validatedServerCertificate, Is.EqualTo(serverCertificate));
        Assert.That(validatedClientCertificate, Is.EqualTo(clientCertificate));
    }
}
