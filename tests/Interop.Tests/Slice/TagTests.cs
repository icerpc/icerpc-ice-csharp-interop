// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc;
using IceRpc.Features;
using IceRpc.Slice;
using NUnit.Framework;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
public partial class TagTests
{
    private static IEnumerable<TestCaseData> IceClassSource
    {
        get
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(new Shape(100));
        }
    }

    private static IEnumerable<TestCaseData> IceSequenceSource
    {
        get
        {
            yield return new TestCaseData(null, null, null, null);

            yield return new TestCaseData(
                "hello world",
                null,
                Array.Empty<Point>(),
                new string[] { "" });

            yield return new TestCaseData(
                "hello world",
                new OneByte[] { new OneByte(5) },
                new Point[] { new Point(100, 200), new Point(-100, 300) },
                new string[] { "foo", "bar" });
        }
    }

    private static IEnumerable<TestCaseData> SliceSequenceSource
    {
        get
        {
            yield return new TestCaseData(null, null, null, null);

            yield return new TestCaseData(
                "hello world",
                null,
                Array.Empty<PointTwin>(),
                new string[] { "" });

            yield return new TestCaseData(
                "hello world",
                new OneByteTwin[] { new OneByteTwin(5) },
                new PointTwin[] { new PointTwin(100, 200), new PointTwin(-100, 300) },
                new string[] { "foo", "bar" });
        }
    }

    private static IEnumerable<TestCaseData> IceStructSource
    {
        get
        {
            yield return new TestCaseData(null, null);
            yield return new TestCaseData(new Point(100, -200), null);
            yield return new TestCaseData(new Point(100, -200), new Person("Bob", Fruit.Apple));
        }
    }

    private static IEnumerable<TestCaseData> SliceStructSource
    {
        get
        {
            yield return new TestCaseData(null, null);
            yield return new TestCaseData(new PointTwin(100, -200), null);
            yield return new TestCaseData(new PointTwin(100, -200), new PersonTwin("Bob", FruitTwin.Apple));
        }
    }

    [TestCase(null, null, null, null)]               // all null
    [TestCase(null, null, null, 123.456)]            // mix of null and non-null
    [TestCase(true, 30_000, -5, -121_212.3434)]      // all non-null
    public async Task Ice_optional_numeric_to_slice_tagged_numeric(bool? f1, short? f2, int? f4, double? f8)
    {
        TagTestServiceTwin service = await IceToSliceAsync(
            proxy => proxy.opFixedAsync(
                f1.ToOptionalValue(),
                f2.ToOptionalValue(),
                f4.ToOptionalValue(),
                f8.ToOptionalValue()));

        Assert.That(service.F1, Is.EqualTo(f1));
        Assert.That(service.F2, Is.EqualTo(f2));
        Assert.That(service.F4, Is.EqualTo(f4));
        Assert.That(service.F8, Is.EqualTo(f8));
    }

    [TestCase(null, null, null, null)]           // all null
    [TestCase(null, null, null, 123.456)]        // mix of null and non-null
    [TestCase(true, 30_000, -5, -121_212.3434)]  // all non-null
    public async Task Slice_tagged_numeric_to_ice_optional_numeric(bool? f1, short? f2, int? f4, double? f8)
    {
        TagTestService service = await SliceToIceAsync(proxy => proxy.OpFixedAsync(f1, f2, f4, f8));

        Assert.That(service.F1, Is.EqualTo(f1));
        Assert.That(service.F2, Is.EqualTo(f2));
        Assert.That(service.F4, Is.EqualTo(f4));
        Assert.That(service.F8, Is.EqualTo(f8));
    }

    [TestCase(Fruit.Orange)]
    [TestCase(null)]
    public async Task Ice_optional_enum_to_slice_tagged_enum(Fruit? fruit)
    {
        TagTestServiceTwin service = await IceToSliceAsync(proxy => proxy.opEnumAsync(fruit.ToOptionalValue()));

        Assert.That((Fruit?)service.Fruit, Is.EqualTo(fruit));
    }

    [TestCase(FruitTwin.Orange)]
    [TestCase(null)]
    public async Task Slice_tagged_enum_to_ice_optional_enum(FruitTwin? fruit)
    {
        TagTestService service = await SliceToIceAsync(proxy => proxy.OpEnumAsync(fruit));

        Assert.That((FruitTwin?)service.Fruit, Is.EqualTo(fruit));
    }

    [TestCase(null, true)]
    [TestCase(null, false)]
    [TestCase("foo/bar:tcp -h localhost -p 10000", true)]
    public async Task Ice_optional_proxy_to_slice_tagged_service_address(string? iceProxyString, bool sendNull)
    {
        using Communicator parsingCommunicator = Util.initialize();
        ObjectPrx? serviceAddress = iceProxyString is null ? null : parsingCommunicator.stringToProxy(iceProxyString);

        // Ice distinguishes between null and not-set, and can send a "set" null proxy. IceRPC's Slice engine does not
        // distinguish between not-set and null when decoding. When encoding a tagged parameter or field, it always
        // encodes null as not-set.
        TagTestServiceTwin service = await IceToSliceAsync(
            proxy => proxy.opServiceAddressAsync(
                sendNull ? new(serviceAddress!) : serviceAddress.ToOptionalReference()));

        Assert.That(
            service.ServiceAddress?.Path[1..],
            Is.EqualTo(serviceAddress is null ? null : Util.identityToString(serviceAddress.ice_getIdentity())));
    }

    [TestCase(null)]
    [TestCase("ice://localhost:10000/foo/bar")]
    public async Task Slice_tagged_service_address_to_ice_optional_proxy(ServiceAddress? serviceAddress)
    {
        TagTestService service = await SliceToIceAsync(proxy => proxy.OpServiceAddressAsync(serviceAddress));

        Assert.That(
            service.ServiceAddress is null ? null : Util.identityToString(service.ServiceAddress.ice_getIdentity()),
            Is.EqualTo(serviceAddress?.Path[1..]));
    }

    [Test, TestCaseSource(nameof(IceStructSource))]
    public async Task Ice_optional_struct_to_slice_tagged_struct(Point? point, Person? person)
    {
        TagTestServiceTwin service = await IceToSliceAsync(
            proxy => proxy.opStructAsync(
                point.ToOptionalValue(),
                person.ToOptionalReference()));

        Assert.That(service.Point?.X, Is.EqualTo(point?.x));
        Assert.That(service.Point?.Y, Is.EqualTo(point?.y));
        Assert.That(service.Person?.Name, Is.EqualTo(person?.name));
        Assert.That((Fruit?)service.Person?.FavoriteFruit, Is.EqualTo(person?.favoriteFruit));
    }

    [Test, TestCaseSource(nameof(SliceStructSource))]
    public async Task Slice_tagged_struct_to_ice_optional_struct(PointTwin? point, PersonTwin? person)
    {
        TagTestService service = await SliceToIceAsync(proxy => proxy.OpStructAsync(point, person));

        Assert.That(service.Point?.x, Is.EqualTo(point?.X));
        Assert.That(service.Point?.y, Is.EqualTo(point?.Y));
        Assert.That(service.Person?.name, Is.EqualTo(person?.Name));
        Assert.That((FruitTwin?)service.Person?.favoriteFruit, Is.EqualTo(person?.FavoriteFruit));
    }

    [Test, TestCaseSource(nameof(IceSequenceSource))]
    public async Task Ice_optional_sequence_to_slice_tagged_sequence(
        string? str,
        OneByte[]? oneByteSeq,
        Point[]? pointSeq,
        string[]? stringSeq)
    {
        TagTestServiceTwin service = await IceToSliceAsync(
            proxy => proxy.opSequenceAsync(
                str.ToOptionalReference(),
                oneByteSeq.ToOptionalReference(),
                pointSeq.ToOptionalReference(),
                stringSeq.ToOptionalReference()));

        Assert.That(service.Str, Is.EqualTo(str));
        Assert.That(service.OneByteSeq?.Length, Is.EqualTo(oneByteSeq?.Length));
        Assert.That(service.PointSeq?.Length, Is.EqualTo(pointSeq?.Length));
        Assert.That(service.StringSeq, Is.EqualTo(stringSeq));
    }

    [Test, TestCaseSource(nameof(SliceSequenceSource))]
    public async Task Slice_tagged_struct_to_ice_optional_struct(
        string? str,
        OneByteTwin[]? oneByteSeq,
        PointTwin[]? pointSeq,
        string[]? stringSeq)
    {
        TagTestService service = await SliceToIceAsync(
            proxy => proxy.OpSequenceAsync(str, oneByteSeq, pointSeq, stringSeq));

        Assert.That(service.Str, Is.EqualTo(str));
        Assert.That(service.OneByteSeq?.Length, Is.EqualTo(oneByteSeq?.Length));
        Assert.That(service.PointSeq?.Length, Is.EqualTo(pointSeq?.Length));
        Assert.That(service.StringSeq, Is.EqualTo(stringSeq));
    }

    [Test, TestCaseSource(nameof(IceClassSource))]
    public void Ice_optional_class_to_slice(Shape? shape)
    {
        Task<TagTestServiceTwin> task = IceToSliceAsync(proxy => proxy.opClassAsync(shape.ToOptionalReference()));

        if (shape is null)
        {
            // The decoding on the server side succeeds when we don't send a value.
            Assert.That(async () => await task, Throws.Nothing);
        }
        else
        {
            // The decoding on the server-side throws InvalidDataException; it's received as an UnknownException.
            Assert.That(async () => await task, Throws.InstanceOf<UnknownException>());
        }
    }

    private static async Task<TagTestServiceTwin> IceToSliceAsync(Func<TagTestPrx, Task> func)
    {
        var service = new TagTestServiceTwin();
        await using var server = new Server(service, new Uri("ice://127.0.0.1:0"));
        ServerAddress serverAddress = server.Listen();

        using Communicator communicator = Util.initialize();
        TagTestPrx proxy = TagTestPrxHelper.uncheckedCast(communicator.CreateObjectPrx("TagTest", serverAddress));
        await func(proxy);
        return service;
    }

    private static async Task<TagTestService> SliceToIceAsync(Func<ITagTest, Task> func)
    {
        using Communicator communicator = Util.initialize();
        ObjectAdapter adapter = communicator.createObjectAdapterWithEndpoints("test", "tcp -h 127.0.0.1 -p 0");
        var service = new TagTestService();
        _ = adapter.add(service, Util.stringToIdentity("TagTest"));
        adapter.activate();

        await using var clientConnection = new ClientConnection(adapter.GetFirstServerAddress());
        var proxy = new TagTestProxy(clientConnection, new Uri("ice:/TagTest"));
        await func(proxy);
        return service;
    }

    private class TagTestService : TagTestDisp_
    {
        internal bool? F1 { get; private set; }

        internal short? F2 { get; private set; }

        internal int? F4 { get; private set; }

        internal double? F8 { get; private set; }

        internal Fruit? Fruit { get; private set; }

        internal OneByte? OneByte { get; private set; }

        internal OneByte[]? OneByteSeq { get; private set; }

        internal Person? Person { get; private set; }

        internal Point? Point { get; private set; }

        internal Point[]? PointSeq { get; private set; }

        internal ObjectPrx? ServiceAddress { get; private set; }

        internal Shape? Shape { get; private set; }

        internal string? Str { get; private set; }

        internal string[]? StringSeq { get; private set; }

        public override void opClass(Optional<Shape> shape, Current? current = null) =>
            Shape = shape.ToNullableReference();

        public override void opEnum(Optional<Fruit> fruit, Current? current = null) => Fruit = fruit.ToNullableValue();

        public override void opFixed(
            Optional<bool> f1,
            Optional<short> f2,
            Optional<int> f4,
            Optional<double> f8,
            Current? current = null)
        {
            F1 = f1.ToNullableValue();
            F2 = f2.ToNullableValue();
            F4 = f4.ToNullableValue();
            F8 = f8.ToNullableValue();
        }

        public override void opSequence(
            Optional<string> str,
            Optional<OneByte[]> oneByteSeq,
            Optional<Point[]> pointSeq,
            Optional<string[]> stringSeq,
            Current? current = null)
        {
            Str = str.ToNullableReference();
            OneByteSeq = oneByteSeq.ToNullableReference();
            PointSeq = pointSeq.ToNullableReference();
            StringSeq = stringSeq.ToNullableReference();
        }

        public override void opServiceAddress(Optional<ObjectPrx> serviceAddress, Current? current = null) =>
            ServiceAddress = serviceAddress.ToNullableReference();

        public override void opStruct(
            Optional<Point> point,
            Optional<Person> person,
            Current? current = null)
        {
            Point = point.ToNullableValue();
            Person = person.ToNullableReference();
        }
    }

    [SliceService]
    private partial class TagTestServiceTwin : ITagTestService
    {
        internal bool? F1 { get; private set; }

        internal short? F2 { get; private set; }

        internal int? F4 { get; private set; }

        internal double? F8 { get; private set; }

        internal FruitTwin? Fruit { get; private set; }

        internal OneByteTwin? OneByte { get; private set; }

        internal OneByteTwin[]? OneByteSeq { get; private set; }

        internal PersonTwin? Person { get; private set; }

        internal PointTwin? Point { get; private set; }

        internal PointTwin[]? PointSeq { get; private set; }

        internal ServiceAddress? ServiceAddress { get; private set; }

        internal string? Str { get; private set; }

        internal string[]? StringSeq { get; private set; }

        public ValueTask OpClassAsync(IFeatureCollection features, CancellationToken cancellationToken) => default;

        public ValueTask OpEnumAsync(FruitTwin? fruit, IFeatureCollection features, CancellationToken cancellationToken)
        {
            Fruit = fruit;
            return default;
        }

        public ValueTask OpFixedAsync(
            bool? f1,
            short? f2,
            int? f4,
            double? f8,
            IFeatureCollection features,
            CancellationToken cancellationToken)
        {
            F1 = f1;
            F2 = f2;
            F4 = f4;
            F8 = f8;
            return default;
        }

        public ValueTask OpSequenceAsync(
            string? str,
            OneByteTwin[]? oneByteSeq,
            PointTwin[]? pointSeq,
            string[]? stringSeq,
            IFeatureCollection features,
            CancellationToken cancellationToken)
        {
            Str = str;
            OneByteSeq = oneByteSeq;
            PointSeq = pointSeq;
            StringSeq = stringSeq;
            return default;
        }

        public ValueTask OpServiceAddressAsync(
            ServiceAddress? serviceAddress,
            IFeatureCollection features,
            CancellationToken cancellationToken)
        {
            ServiceAddress = serviceAddress;
            return default;
        }

        public ValueTask OpStructAsync(
            PointTwin? point,
            PersonTwin? person,
            IFeatureCollection features,
            CancellationToken cancellationToken)
        {
            Point = point;
            Person = person;
            return default;
        }
    }
}
