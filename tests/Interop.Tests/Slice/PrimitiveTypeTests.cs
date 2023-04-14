// Copyright (c) ZeroC, Inc.

using IceRpc.Slice;
using NUnit.Framework;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
public class PrimitiveTypeTests
{
    /// <summary>Encodes an ice bool then decodes a slice bool.</summary>
    [TestCase(true)]
    [TestCase(false)]
    public void Ice_bool_to_slice_bool(bool value)
    {
        bool decodedValue = value.IceToSlice(
            (outputStream, value) => outputStream.writeBool(value),
            (ref SliceDecoder decoder) => decoder.DecodeBool());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes a slice bool then decodes an ice bool.</summary>
    [TestCase(true)]
    [TestCase(false)]
    public void Slice_bool_to_ice_bool(bool value)
    {
        bool decodedValue = value.SliceToIce(
            (ref SliceEncoder encoder, bool value) => encoder.EncodeBool(value),
            inputStream => inputStream.readBool());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an ice short then decodes a slice int16.</summary>
    [TestCase(-30_000)]
    [TestCase(30_000)]
    public void Ice_short_to_slice_int16(short value)
    {
        short decodedValue = value.IceToSlice(
            (outputStream, value) => outputStream.writeShort(value),
            (ref SliceDecoder decoder) => decoder.DecodeInt16());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes a slice int16 then decodes an ice short.</summary>
    [TestCase(-30_000)]
    [TestCase(30_000)]
    public void Slice_int16_to_ice_short(short value)
    {
        short decodedValue = value.SliceToIce(
            (ref SliceEncoder encoder, short value) => encoder.EncodeInt16(value),
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
            (ref SliceDecoder decoder) => decoder.DecodeFloat64());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes a slice float64 then decodes an ice double.</summary>
    [TestCase(-3.141592653589793238462643383279502884197)]
    [TestCase(0.0)]
    [TestCase(2.71828182845904523536028747135266249775724709369995957)]
    public void Slice_float64_to_ice_double(double value)
    {
        double decodedValue = value.SliceToIce(
            (ref SliceEncoder encoder, double value) => encoder.EncodeFloat64(value),
            inputStream => inputStream.readDouble());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an ice size then decodes a slice size.</summary>
    [TestCase(0)]
    [TestCase(7)]
    [TestCase(254)]
    [TestCase(350)]
    public void Ice_size_to_slice_size(int value)
    {
        int decodedValue = value.IceToSlice(
            (outputStream, value) => outputStream.writeSize(value),
            (ref SliceDecoder decoder) => decoder.DecodeSize());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes a slice size then decodes an ice size.</summary>
    [TestCase(0)]
    [TestCase(7)]
    [TestCase(254)]
    [TestCase(350)]
    public void Slice_size_to_ice_size(int value)
    {
        int decodedValue = value.SliceToIce(
            (ref SliceEncoder encoder, int value) => encoder.EncodeSize(value),
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
            (ref SliceDecoder decoder) => decoder.DecodeString());

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
        string decodedValue = value.SliceToIce(
            (ref SliceEncoder encoder, string value) => encoder.EncodeString(value),
            inputStream => inputStream.readString());

        Assert.That(decodedValue, Is.EqualTo(value));
    }
}
