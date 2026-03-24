// Copyright (c) ZeroC, Inc.

using NUnit.Framework;
using IceRpc.Ice.Codec;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
internal class PrimitiveTypeTests
{
    /// <summary>Encodes an ice bool then decodes a slice bool.</summary>
    [Test]
    public void Ice_bool_to_slice_bool([Values] bool value)
    {
        bool decodedValue = value.IceToSlice(
            (outputStream, value) => outputStream.writeBool(value),
            (ref decoder) => decoder.DecodeBool());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes a slice bool then decodes an ice bool.</summary>
    [Test]
    public void Slice_bool_to_ice_bool([Values] bool value)
    {
        using var communicator = new Ice.Communicator();
        bool decodedValue = value.SliceToIce(
            communicator,
            (ref encoder, value) => encoder.EncodeBool(value),
            inputStream => inputStream.readBool());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an ice short then decodes a slice int16.</summary>
    [Test]
    public void Ice_short_to_slice_int16([Values(-30_000, 30_000)] short value)
    {
        short decodedValue = value.IceToSlice(
            (outputStream, value) => outputStream.writeShort(value),
            (ref decoder) => decoder.DecodeShort());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes a slice int16 then decodes an ice short.</summary>
    [Test]
    public void Slice_int16_to_ice_short([Values(-30_000, 30_000)] short value)
    {
        using var communicator = new Ice.Communicator();
        short decodedValue = value.SliceToIce(
            communicator,
            (ref encoder, value) => encoder.EncodeShort(value),
            inputStream => inputStream.readShort());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an ice double then decodes a slice float64.</summary>
    [TestCase(-3.141592653589793238462643383279502884197)]
    [TestCase(0.0)]
    [TestCase(2.71828182845904523536028747135266249775724709369995957)]
    public void Ice_double_to_slice_float64(double value)
    {
        double decodedValue = value.IceToSlice(
            (outputStream, value) => outputStream.writeDouble(value),
            (ref decoder) => decoder.DecodeDouble());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes a slice float64 then decodes an ice double.</summary>
    [TestCase(-3.141592653589793238462643383279502884197)]
    [TestCase(0.0)]
    [TestCase(2.71828182845904523536028747135266249775724709369995957)]
    public void Slice_float64_to_ice_double(double value)
    {
        using var communicator = new Ice.Communicator();
        double decodedValue = value.SliceToIce(
            communicator,
            (ref encoder, value) => encoder.EncodeDouble(value),
            inputStream => inputStream.readDouble());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an ice size then decodes a slice size.</summary>
    [Test]
    public void Ice_size_to_slice_size([Values(0, 7, 254, 350)] int value)
    {
        int decodedValue = value.IceToSlice(
            (outputStream, value) => outputStream.writeSize(value),
            (ref decoder) => decoder.DecodeSize());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes a slice size then decodes an ice size.</summary>
    [Test]
    public void Slice_size_to_ice_size([Values(0, 7, 254, 350)] int value)
    {
        using var communicator = new Ice.Communicator();
        int decodedValue = value.SliceToIce(
            communicator,
            (ref encoder, value) => encoder.EncodeSize(value),
            inputStream => inputStream.readSize());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an ice string then decodes a slice string.</summary>
    [TestCase("abcd")]
    [TestCase("")]
    [TestCase("국민경제의 발전을 위한 중요정책의 수립에 관하여 대통령의 자문에 응하기 위하여 국민경제자문회의를 둘 수 있다")]
    [TestCase("旅ロ京青利セムレ弱改フヨス波府かばぼ意送でぼ調掲察たス日西重ケアナ住橋ユムミク順待ふかんぼ人奨貯鏡すびそ")]
    [TestCase("😁😂😃😄😅😆😉😊😋😌😍😏😒😓😔😖")]
    public void Ice_string_to_slice_string(string value)
    {
        string decodedValue = value.IceToSlice(
            (outputStream, value) => outputStream.writeString(value),
            (ref decoder) => decoder.DecodeString());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes a slice string then decodes an ice string.</summary>
    [TestCase("abcd")]
    [TestCase("")]
    [TestCase("국민경제의 발전을 위한 중요정책의 수립에 관하여 대통령의 자문에 응하기 위하여 국민경제자문회의를 둘 수 있다")]
    [TestCase("旅ロ京青利セムレ弱改フヨス波府かばぼ意送でぼ調掲察たス日西重ケアナ住橋ユムミク順待ふかんぼ人奨貯鏡すびそ")]
    [TestCase("😁😂😃😄😅😆😉😊😋😌😍😏😒😓😔😖")]
    public void Slice_string_to_ice_string(string value)
    {
        using var communicator = new Ice.Communicator();
        string decodedValue = value.SliceToIce(
            communicator,
            (ref encoder, value) => encoder.EncodeString(value),
            inputStream => inputStream.readString());

        Assert.That(decodedValue, Is.EqualTo(value));
    }
}
