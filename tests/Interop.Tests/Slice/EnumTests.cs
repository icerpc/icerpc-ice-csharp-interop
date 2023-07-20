// Copyright (c) ZeroC, Inc.

using Ice;
using NUnit.Framework;
using System.Buffers;
using System.IO.Pipelines;
using ZeroC.Slice;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
public class EnumTests
{
    [Test]
    public void Ice_my_enum_to_slice_my_enum()
    {
        MyEnumTwin decodedValue = IceToSlice(
            MyEnum.Enum1,
            MyEnumHelper.write,
            MyEnumTwinSliceDecoderExtensions.DecodeMyEnumTwin);

        Assert.That(decodedValue, Is.EqualTo(MyEnumTwin.Enum1));
    }

    [Test]
    public void Slice_my_enum_to_ice_my_enum()
    {
        MyEnum decodedValue = SliceToIce(
            MyEnumTwin.Enum1,
            MyEnumTwinSliceEncoderExtensions.EncodeMyEnumTwin,
            MyEnumHelper.read);

        Assert.That(decodedValue, Is.EqualTo(MyEnum.Enum1));
    }

    private static TSliceEnum IceToSlice<TIceEnum, TSliceEnum>(
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
