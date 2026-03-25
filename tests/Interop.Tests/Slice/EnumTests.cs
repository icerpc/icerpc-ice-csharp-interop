// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc.Ice.Codec;
using NUnit.Framework;
using System.Buffers;
using System.IO.Pipelines;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
internal class EnumTests
{
    [Test]
    public void Ice_my_enum_to_slice_my_enum()
    {
        MyEnumTwin decodedValue = IceToSlice(
            MyEnum.Enum1,
            MyEnumHelper.write,
            MyEnumTwinIceDecoderExtensions.DecodeMyEnumTwin);

        Assert.That(decodedValue, Is.EqualTo(MyEnumTwin.Enum1));
    }

    [Test]
    public void Slice_my_enum_to_ice_my_enum()
    {
        MyEnum decodedValue = SliceToIce(
            MyEnumTwin.Enum1,
            MyEnumTwinIceEncoderExtensions.EncodeMyEnumTwin,
            MyEnumHelper.read);

        Assert.That(decodedValue, Is.EqualTo(MyEnum.Enum1));
    }

    private static TSliceEnum IceToSlice<TIceEnum, TSliceEnum>(
        TIceEnum value,
        Action<OutputStream, TIceEnum> encodeAction,
        DecodeFunc<TSliceEnum> decodeFunc)
    {
        using var communicator = new Communicator();
        var outputStream = new OutputStream(communicator);
        encodeAction(outputStream, value);
        byte[] buffer = outputStream.finished();

        var decoder = new IceDecoder(buffer);
        return decodeFunc(ref decoder);
    }

    private static TIceEnum SliceToIce<TSliceEnum, TIceEnum>(
        TSliceEnum value,
        EncodeAction<TSliceEnum> encodeAction,
        Func<InputStream, TIceEnum> decodeFunc)
    {
        var pipe = new Pipe();
        var encoder = new IceEncoder(pipe.Writer);
        encodeAction(ref encoder, value);
        pipe.Writer.Complete();
        pipe.Reader.TryRead(out ReadResult readResult);

        using var communicator = new Communicator();
        var inputStream = new InputStream(communicator, readResult.Buffer.ToArray());
        pipe.Reader.Complete();

        return decodeFunc(inputStream);
    }
}
