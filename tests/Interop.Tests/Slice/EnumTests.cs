// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc.Slice;
using NUnit.Framework;
using System.Buffers;
using System.IO.Pipelines;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
public class EnumTests
{
    [TestCase(OneByteEnum.value1, OneByteEnumTwin.Value1)]
    [TestCase(OneByteEnum.value2, OneByteEnumTwin.Value2)]
    public void Ice_one_byte_enum_to_slice_one_byte_enum(OneByteEnum value, OneByteEnumTwin expectedValue)
    {
        OneByteEnumTwin decodedValue = IceToSlice(
            value,
            OneByteEnumHelper.write,
            OneByteEnumTwinSliceDecoderExtensions.DecodeOneByteEnumTwin);

        Assert.That(decodedValue, Is.EqualTo(expectedValue));
    }

    [TestCase(OneByteEnumTwin.Value1, OneByteEnum.value1)]
    [TestCase(OneByteEnumTwin.Value2, OneByteEnum.value2)]
    public void Slice_one_byte_enum_to_ice_one_byte_enum(OneByteEnumTwin value, OneByteEnum expectedValue)
    {
        OneByteEnum decodedValue = SliceToIce(
            value,
            OneByteEnumTwinSliceEncoderExtensions.EncodeOneByteEnumTwin,
            OneByteEnumHelper.read);

        Assert.That(decodedValue, Is.EqualTo(expectedValue));
    }

    [TestCase(FiveBytesEnum.value1, FiveBytesEnumTwin.Value1)]
    [TestCase(FiveBytesEnum.value2, FiveBytesEnumTwin.Value2)]
    [TestCase(FiveBytesEnum.value3, FiveBytesEnumTwin.Value3)]
    public void Ice_five_bytes_enum_to_slice_five_bytes_enum(FiveBytesEnum value, FiveBytesEnumTwin expectedValue)
    {
        FiveBytesEnumTwin decodedValue = IceToSlice(
            value,
            FiveBytesEnumHelper.write,
            FiveBytesEnumTwinSliceDecoderExtensions.DecodeFiveBytesEnumTwin);

        Assert.That(decodedValue, Is.EqualTo(expectedValue));
    }

    [TestCase(FiveBytesEnumTwin.Value1, FiveBytesEnum.value1)]
    [TestCase(FiveBytesEnumTwin.Value2, FiveBytesEnum.value2)]
    [TestCase(FiveBytesEnumTwin.Value3, FiveBytesEnum.value3)]
    public void Slice_five_bytes_enum_to_ice_five_bytes_enum(FiveBytesEnumTwin value, FiveBytesEnum expectedValue)
    {
        FiveBytesEnum decodedValue = SliceToIce(
            value,
            FiveBytesEnumTwinSliceEncoderExtensions.EncodeFiveBytesEnumTwin,
            FiveBytesEnumHelper.read);

        Assert.That(decodedValue, Is.EqualTo(expectedValue));
    }

    private static TSliceEnum IceToSlice<TIceEnum,TSliceEnum>(
        TIceEnum value,
        Action<OutputStream, TIceEnum> encodeAction,
        DecodeFunc<TSliceEnum> decodeFunc)
    {
        using Communicator communicator = Util.initialize();
        var outputStream = new OutputStream(communicator);
        encodeAction(outputStream, value);
        byte[] buffer = outputStream.finished();

        var decoder = new SliceDecoder(buffer, SliceEncoding.Slice1);
        return decodeFunc(ref decoder);
    }

    private static TIceEnum SliceToIce<TSliceEnum, TIceEnum>(
        TSliceEnum value,
        EncodeAction<TSliceEnum> encodeAction,
        Func<InputStream, TIceEnum> decodeFunc)
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
