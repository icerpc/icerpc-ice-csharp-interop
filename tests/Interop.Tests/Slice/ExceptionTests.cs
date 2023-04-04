// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using IceRpc.Features;
using IceRpc.Slice;
using NUnit.Framework;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
public class ExceptionTests
{
    /// <summary>Throws an exception encoded with the .ice-generated code and decodes this exception with the
    /// .slice-generated code.</summary>
    [Test]
    public void Ice_exception_to_slice_exception([Values] bool slicedFormat)
    {
        var cylinderException = new CylinderException("cy123", 4);

        CylinderExceptionTwin? exception = Assert.ThrowsAsync<CylinderExceptionTwin>(
            async () => await IceToSliceAsync(cylinderException, slicedFormat));

        Assert.That(exception!.ErrorCode, Is.EqualTo(cylinderException.errorCode));
        Assert.That(exception!.Cylinder, Is.EqualTo(cylinderException.cylinder));
    }

    /// <summary>Throws an exception encoded with the .slice-generated code and decodes this exception with the .ice
    /// generated code.</summary>
    [Test]
    public void Slice_exception_to_ice_exception()
    {
        var cylinderException = new CylinderExceptionTwin("cy123", 4);

        CylinderException? exception = Assert.ThrowsAsync<CylinderException>(
            async () => await SliceToIceAsync(cylinderException));

        Assert.That(exception!.errorCode, Is.EqualTo(cylinderException.ErrorCode));
        Assert.That(exception.cylinder, Is.EqualTo(cylinderException.Cylinder));
    }

    /// <summary>Throws an exception encoded with the .ice-generated code and decodes this exception with the
    /// .slice-generated code while slicing unknown slices.</summary>
    [Test]
    public void Ice_exception_to_slice_exception_with_slicing()
    {
        var fuelPumpException = new FuelPumpException("fp123");

        EngineExceptionTwin? exception = Assert.ThrowsAsync<EngineExceptionTwin>(
            async () => await IceToSliceAsync(fuelPumpException, slicedFormat: true));

        Assert.That(exception!.ErrorCode, Is.EqualTo(fuelPumpException.errorCode));
    }

    /// <summary>Throws an exception encoded with the .slice-generated code and decodes this exception with the .ice
    /// generated code while slicing unknown slices.</summary>
    [Test]
    public void Slice_exception_to_ice_exception_with_slicing()
    {
        var batteryException = new BatteryException("bt123", 5.4f);

        EngineException? exception = Assert.ThrowsAsync<EngineException>(
            async () => await SliceToIceAsync(batteryException));

        Assert.That(exception!.errorCode, Is.EqualTo(batteryException.ErrorCode));
    }

    /// <summary>Throws an exception encoded with the .ice-generated code and decodes this exception with the
    /// .slice-generated code; this exception is not in the operation's exception specification.</summary>
    [Test]
    public void Ice_exception_to_non_specified_slice_exception([Values] bool slicedFormat)
    {
        var tirePressureException = new TirePressureException("FrontRight");

        // IceRPC-Slice enforces exception specifications on the dispatch-side while Ice-Slice enforces exception
        // specifications on the invoke-side; here, the exception goes through.
        Assert.That(
            async () => await IceToSliceAsync(tirePressureException, slicedFormat),
            Throws.TypeOf<TirePressureExceptionTwin>());
    }

    /// <summary>Throws an exception encoded with the .slice-generated code and decodes this exception with the
    /// .ice-generated code; this exception is not in the operation's exception specification.</summary>
    [Test]
    public void Slice_exception_to_non_specified_ice_exception()
    {
        var tirePressureException = new TirePressureExceptionTwin("FrontRight");

        // IceRPC-Slice encodes the unspecified exception as an UnknownException (not UnknownUserException).
        Assert.That(
            async () => await SliceToIceAsync(tirePressureException),
            Throws.TypeOf<UnknownException>());
    }

    /// <summary>Throws an exception encoded with the .ice-generated code and decodes this exception with the
    /// .slice-generated code; this exception is not in the operation's exception specification and unknown to the
    /// recipient.</summary>
    [Test]
    public void Ice_exception_to_unknown_slice_exception([Values] bool slicedFormat)
    {
        var wiperException = new WiperException();

        // TODO: is this correct?
        Assert.That(
            async () => await IceToSliceAsync(wiperException, slicedFormat),
            Throws.TypeOf<InvalidDataException>());
    }

    private static async Task IceToSliceAsync(UserException userException, bool slicedFormat)
    {
        string[] args = new string[] { $"--Ice.Default.SlicedFormat={(slicedFormat ? 1 : 0)}" };
        using Communicator communicator = Util.initialize(ref args);
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        _ = adapter.add(new TestEngine(userException), Util.stringToIdentity("protoX"));
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        var proxy = new EngineProxy(clientConnection, new Uri("ice:/protoX"));

        await proxy.StartAsync();
    }

    private static async Task SliceToIceAsync(SliceException sliceException)
    {
        await using var server = new Server(new TestEngineTwin(sliceException), new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using Communicator communicator = Util.initialize();
        EnginePrx proxy = EnginePrxHelper.uncheckedCast(communicator.CreateObjectPrx("protoX", serverAddress));
        await proxy.startAsync();
    }

    private class TestEngine : EngineDisp_
    {
        private readonly UserException _userException;

        public override void start(Current? current = null) => throw _userException;

        internal TestEngine(UserException userException) => _userException = userException;
    }

    private class TestEngineTwin : Service, IEngineService
    {
        private readonly SliceException _sliceException;

        public ValueTask StartAsync(IFeatureCollection features, CancellationToken cancellationToken) =>
            throw _sliceException;

        internal TestEngineTwin(SliceException sliceException) => _sliceException = sliceException;
    }
}
