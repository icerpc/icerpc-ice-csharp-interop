// Copyright (c) ZeroC, Inc.

using IceRpc.Ice.Codec;
using NUnit.Framework;

namespace Interop.Tests.Generator;

[Parallelizable(scope: ParallelScope.All)]
internal class PrimitiveTypeTests
{
    /// <summary>Encodes an Ice bool then decodes an IceRpc bool.</summary>
    [Test]
    public void Ice_bool_to_icerpc_bool([Values] bool value)
    {
        bool decodedValue = value.IceToIceRpc(
            (outputStream, value) => outputStream.writeBool(value),
            (ref decoder) => decoder.DecodeBool());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an IceRpc bool then decodes an Ice bool.</summary>
    [Test]
    public void IceRpc_bool_to_ice_bool([Values] bool value)
    {
        using var communicator = new Ice.Communicator();
        bool decodedValue = value.IceRpcToIce(
            communicator,
            (ref encoder, value) => encoder.EncodeBool(value),
            inputStream => inputStream.readBool());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an Ice short then decodes an IceRpc short.</summary>
    [Test]
    public void Ice_short_to_icerpc_short([Values(-30_000, 30_000)] short value)
    {
        short decodedValue = value.IceToIceRpc(
            (outputStream, value) => outputStream.writeShort(value),
            (ref decoder) => decoder.DecodeShort());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an IceRpc short then decodes an Ice short.</summary>
    [Test]
    public void IceRpc_short_to_ice_short([Values(-30_000, 30_000)] short value)
    {
        using var communicator = new Ice.Communicator();
        short decodedValue = value.IceRpcToIce(
            communicator,
            (ref encoder, value) => encoder.EncodeShort(value),
            inputStream => inputStream.readShort());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an Ice double then decodes an IceRpc double.</summary>
    [TestCase(-3.141592653589793238462643383279502884197)]
    [TestCase(0.0)]
    [TestCase(2.71828182845904523536028747135266249775724709369995957)]
    public void Ice_double_to_icerpc_double(double value)
    {
        double decodedValue = value.IceToIceRpc(
            (outputStream, value) => outputStream.writeDouble(value),
            (ref decoder) => decoder.DecodeDouble());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an IceRpc double then decodes an Ice double.</summary>
    [TestCase(-3.141592653589793238462643383279502884197)]
    [TestCase(0.0)]
    [TestCase(2.71828182845904523536028747135266249775724709369995957)]
    public void IceRpc_double_to_ice_double(double value)
    {
        using var communicator = new Ice.Communicator();
        double decodedValue = value.IceRpcToIce(
            communicator,
            (ref encoder, value) => encoder.EncodeDouble(value),
            inputStream => inputStream.readDouble());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an Ice size then decodes an IceRpc size.</summary>
    [Test]
    public void Ice_size_to_icerpc_size([Values(0, 7, 254, 350)] int value)
    {
        int decodedValue = value.IceToIceRpc(
            (outputStream, value) => outputStream.writeSize(value),
            (ref decoder) => decoder.DecodeSize());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an IceRpc size then decodes an Ice size.</summary>
    [Test]
    public void IceRpc_size_to_ice_size([Values(0, 7, 254, 350)] int value)
    {
        using var communicator = new Ice.Communicator();
        int decodedValue = value.IceRpcToIce(
            communicator,
            (ref encoder, value) => encoder.EncodeSize(value),
            inputStream => inputStream.readSize());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an Ice string then decodes an IceRpc string.</summary>
    [TestCase("abcd")]
    [TestCase("")]
    [TestCase("국민경제의 발전을 위한 중요정책의 수립에 관하여 대통령의 자문에 응하기 위하여 국민경제자문회의를 둘 수 있다")]
    [TestCase("旅ロ京青利セムレ弱改フヨス波府かばぼ意送でぼ調掲察たス日西重ケアナ住橋ユムミク順待ふかんぼ人奨貯鏡すびそ")]
    [TestCase("😁😂😃😄😅😆😉😊😋😌😍😏😒😓😔😖")]
    public void Ice_string_to_icerpc_string(string value)
    {
        string decodedValue = value.IceToIceRpc(
            (outputStream, value) => outputStream.writeString(value),
            (ref decoder) => decoder.DecodeString());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an IceRpc string then decodes an Ice string.</summary>
    [TestCase("abcd")]
    [TestCase("")]
    [TestCase("국민경제의 발전을 위한 중요정책의 수립에 관하여 대통령의 자문에 응하기 위하여 국민경제자문회의를 둘 수 있다")]
    [TestCase("旅ロ京青利セムレ弱改フヨス波府かばぼ意送でぼ調掲察たス日西重ケアナ住橋ユムミク順待ふかんぼ人奨貯鏡すびそ")]
    [TestCase("😁😂😃😄😅😆😉😊😋😌😍😏😒😓😔😖")]
    public void IceRpc_string_to_ice_string(string value)
    {
        using var communicator = new Ice.Communicator();
        string decodedValue = value.IceRpcToIce(
            communicator,
            (ref encoder, value) => encoder.EncodeString(value),
            inputStream => inputStream.readString());

        Assert.That(decodedValue, Is.EqualTo(value));
    }
}
