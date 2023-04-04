// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using IceRpc.Features;
using IceRpc.RequestContext;
using IceRpc.Slice;
using Interop.Tests.Slice;
using NUnit.Framework;

namespace Interop.Tests;

[Parallelizable(scope: ParallelScope.All)]
public class RequestContextTests
{
    private static IEnumerable<Dictionary<string, string>> RequestContextSource
    {
        get
        {
            yield return new Dictionary<string, string>();
            yield return new Dictionary<string, string>
            {
                ["Foo"] = "Bar",
                ["Empty"] = "",
            };
        }
    }

    /// <summary>Sends a request with a request context from Ice to IceRPC.</summary>
    [TestCaseSource(nameof(RequestContextSource))]
    public async Task Send_request_with_context_from_Ice_to_IceRpc(
        Dictionary<string, string> context)
    {
        // Arrange
        var chatBoot = new ChatBotTwin();
        await using var server = new Server(
            new RequestContextMiddleware(chatBoot),
            new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using Communicator communicator = Util.initialize();
        GreeterPrx proxy = GreeterPrxHelper.uncheckedCast(communicator.CreateObjectPrx("greater", serverAddress));

        // Act
        await proxy.greetAsync("Bob", context);

        // Assert
        Assert.That(async () => await chatBoot.RequestContext, Is.EqualTo(context));
    }

    /// <summary>Sends a request with a request context from IceRPC to Ice.</summary>
    [TestCaseSource(nameof(RequestContextSource))]
    public async Task Send_request_with_context_from_IceRpc_to_Ice(
        Dictionary<string, string> context)
    {
        // Arrange
        var chatBoot = new ChatBot();
        using Communicator communicator = Util.initialize();
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        _ = adapter.add(chatBoot, Util.stringToIdentity("greater"));
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        Pipeline pipeline = new Pipeline().UseRequestContext().Into(clientConnection);
        var proxy = new GreeterProxy(pipeline, new Uri("ice:/greater"));

        var features = new FeatureCollection();
        features.Set<IRequestContextFeature>(new RequestContextFeature{ Value = context });

        // Act
        await proxy.GreetAsync("Bob", features);

        // Assert
        Assert.That(async () => await chatBoot.RequestContext, Is.EqualTo(context));
    }

    private class ChatBot : GreeterDisp_
    {
        public Task<IDictionary<string, string>> RequestContext => _requestContextTcs.Task;

        private readonly TaskCompletionSource<IDictionary<string, string>> _requestContextTcs = new();

        public override string greet(string name, Current? current)
        {
            _requestContextTcs.SetResult(current?.ctx ?? new Dictionary<string, string>());
            return $"Hello, {name}!";
        }
    }

    private class ChatBotTwin : Service, IGreeterService
    {
        public Task<IDictionary<string, string>> RequestContext => _requestContextTcs.Task;

        private readonly TaskCompletionSource<IDictionary<string, string>> _requestContextTcs = new();


        public ValueTask<string> GreetAsync(
            string name,
            IFeatureCollection features,
            CancellationToken cancellationToken)
        {

            _requestContextTcs.SetResult(
                features.Get<IRequestContextFeature>()?.Value ?? new Dictionary<string, string>());
            return new($"Hello, {name}!");
        }
    }
}
