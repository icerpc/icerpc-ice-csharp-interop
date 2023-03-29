// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc.Slice;
using NUnit.Framework;
using System.Buffers;
using System.IO.Pipelines;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
public class PrimitiveTypeTests
{
    /// <summary>Encodes an ice bool then decodes a slice bool.</summary>
    [TestCase(true)]
    [TestCase(false)]
    public void Ice_bool_to_slice_bool(bool value)
    {
        bool decodedValue = IceToSlice(
            value,
            (outputStream, value) => outputStream.writeBool(value),
            (ref SliceDecoder decoder) => decoder.DecodeBool());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes a slice bool then decodes an ice bool.</summary>
    [TestCase(true)]
    [TestCase(false)]
    public void Slice_bool_to_ice_bool(bool value)
    {
        bool decodedValue = SliceToIce(
            value,
            (ref SliceEncoder encoder, bool value) => encoder.EncodeBool(value),
            inputStream => inputStream.readBool());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an ice short then decodes a slice int16.</summary>
    [TestCase(-30_000)]
    [TestCase(30_000)]
    public void Ice_short_to_slice_int16(short value)
    {
        short decodedValue = IceToSlice(
            value,
            (outputStream, value) => outputStream.writeShort(value),
            (ref SliceDecoder decoder) => decoder.DecodeInt16());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes a slice int16 then decodes an ice short.</summary>
    [TestCase(-30_000)]
    [TestCase(30_000)]
    public void Slice_int16_to_ice_short(short value)
    {
        short decodedValue = SliceToIce(
            value,
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
        double decodedValue = IceToSlice(
            value,
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
        double decodedValue = SliceToIce(
            value,
            (ref SliceEncoder encoder, double value) => encoder.EncodeFloat64(value),
            inputStream => inputStream.readDouble());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes an ice string then decodes a slice string.</summary>
    [TestCase("abcd")]
    [TestCase("")]
    [TestCase("국민경제의 발전을 위한 중요정책의 수립에 관하여 대통령의 자문에 응하기 위하여 국민경제자문회의를 둘 수 있다")] // Korean
    [TestCase("旅ロ京青利セムレ弱改フヨス波府かばぼ意送でぼ調掲察たス日西重ケアナ住橋ユムミク順待ふかんぼ人奨貯鏡すびそ")]  // Japanese
    [TestCase("😁😂😃😄😅😆😉😊😋😌😍😏😒😓😔😖")]
    public void Ice_string_to_slice_string(string value)
    {
        string decodedValue = IceToSlice(
            value,
            (outputStream, value) => outputStream.writeString(value),
            (ref SliceDecoder decoder) => decoder.DecodeString());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    /// <summary>Encodes a slice string then decodes an ice string.</summary>
    [TestCase("abcd")]
    [TestCase("")]
    [TestCase("국민경제의 발전을 위한 중요정책의 수립에 관하여 대통령의 자문에 응하기 위하여 국민경제자문회의를 둘 수 있다")] // Korean
    [TestCase("旅ロ京青利セムレ弱改フヨス波府かばぼ意送でぼ調掲察たス日西重ケアナ住橋ユムミク順待ふかんぼ人奨貯鏡すびそ")]  // Japanese
    [TestCase("😁😂😃😄😅😆😉😊😋😌😍😏😒😓😔😖")]
    public void Slice_string_to_ice_string(string value)
    {
        string decodedValue = SliceToIce(
            value,
            (ref SliceEncoder encoder, string value) => encoder.EncodeString(value),
            inputStream => inputStream.readString());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    private static T IceToSlice<T>(T value, Action<OutputStream, T> encodeAction, DecodeFunc<T> decodeFunc)
    {
        using Communicator communicator = Util.initialize();
        var outputStream = new OutputStream(communicator);
        encodeAction(outputStream, value);
        byte[] buffer = outputStream.finished();

        var decoder = new SliceDecoder(buffer, SliceEncoding.Slice1);
        return decodeFunc(ref decoder);
    }

    private static T SliceToIce<T>(T value, EncodeAction<T> encodeAction, Func<InputStream, T> decodeFunc)
    {
        var pipe = new Pipe();
        var encoder = new SliceEncoder(pipe.Writer, SliceEncoding.Slice1);
        encodeAction(ref encoder, value);
        pipe.Writer.Complete();
        pipe.Reader.TryRead(out ReadResult readResult);

        using Communicator communicator = Util.initialize();
        var inputStream = new InputStream(communicator, readResult.Buffer.ToArray());
        pipe.Reader.Complete();

        return decodeFunc(inputStream);
    }
}
