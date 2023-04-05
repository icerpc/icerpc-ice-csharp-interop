// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using NUnit.Framework;
using System.Buffers;
using System.IO.Pipelines;
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

        byte[] expectedPayload = Enumerable.Range(0, 4096).Select(p => (byte)p).ToArray();
        var tcs = new TaskCompletionSource<byte[]>();
        var dispatcher = new InlineDispatcher(async (request, cancellationToken) =>
        {
            ReadResult readResult = await request.Payload.ReadAtLeastAsync(
                expectedPayload.Length + 1,
                cancellationToken);
            tcs.SetResult(readResult.Buffer.ToArray());
            request.Payload.AdvanceTo(readResult.Buffer.End);
            return new OutgoingResponse(request);
        });
#pragma warning disable CA5359 // Do Not Disable Certificate Validation
        await using var server = new Server(
            dispatcher,
            new Uri("ice://127.0.0.1:0"),
            serverAuthenticationOptions: new SslServerAuthenticationOptions
            {
                ServerCertificate = new X509Certificate2("../../../../../certs/server.p12", "password"),
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true,
                ClientCertificateRequired = true
            });
#pragma warning restore CA5359 // Do Not Disable Certificate Validation
        ServerAddress serverAddress = server.Listen();

        // Load and configure the IceSSL plugin.
        string[] args = new string[] 
        { 
            "--Ice.Plugin.IceSSL=IceSSL:IceSSL.PluginFactory",
            "--IceSSL.DefaultDir=../../../../../certs/",
            "--IceSSL.CertFile=client.p12",
            "--IceSSL.CAs=cacert.pem",
            "--IceSSL.Password=password",
        };
        using Communicator communicator = Util.initialize(ref args);
        ObjectPrx proxy = communicator.CreateObjectPrx("hello", serverAddress with { Transport = "ssl" });

        // Act
        _ = await proxy.ice_invokeAsync(
            operation: "op",
            mode: OperationMode.Normal,
            expectedPayload.CreateEncapsulation());

        // Assert
        Assert.That(async () => await tcs.Task, Is.EqualTo(expectedPayload));
    }

    [Test]
    public async Task Send_request_from_IceRPC_to_Ice()
    {
        // Arrange
        byte[] expectedPayload = Enumerable.Range(0, 4096).Select(p => (byte)p).ToArray();
        var tcs = new TaskCompletionSource<byte[]>();

        // Load and configure the IceSSL plugin.
        string[] args = new string[]
        {
            "--Ice.Plugin.IceSSL=IceSSL:IceSSL.PluginFactory",
            "--IceSSL.DefaultDir=../../../../../certs/",
            "--IceSSL.CertFile=server.p12",
            "--IceSSL.CAs=cacert.pem",
            "--IceSSL.Password=password",
        };
        using Communicator communicator = Util.initialize(ref args);
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "ssl -h 127.0.0.1 -p 0");
        adapter.addDefaultServant(
            new InlineBlobject(
                (inParams, current) =>
                {
                    tcs.SetResult(inParams[6..]);
                    return (true, Array.Empty<byte>().CreateEncapsulation());
                }),
            "");
        adapter.activate();

#pragma warning disable CA5359 // Do Not Disable Certificate Validation
        await using var clientConnection = new ClientConnection(
            adapter.GetFirstServerAddress(),
            clientAuthenticationOptions: new SslClientAuthenticationOptions
            {
                ClientCertificates = new X509CertificateCollection
                {
                    new X509Certificate2("../../../../../certs/client.p12", "password")
                },
                RemoteCertificateValidationCallback = (sender, certificate, chain, errors) => true,
            });
#pragma warning restore CA5359 // Do Not Disable Certificate Validation

        using var request = new OutgoingRequest(new ServiceAddress(new Uri("ice:/hello")))
        {
            Payload = PipeReader.Create(new ReadOnlySequence<byte>(expectedPayload))
        };

        // Act
        _ = await clientConnection.InvokeAsync(request);

        // Assert
        Assert.That(async () => await tcs.Task, Is.EqualTo(expectedPayload));
    }
}
