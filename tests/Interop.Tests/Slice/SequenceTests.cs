// Copyright (c) ZeroC, Inc.

using IceRpc.Slice;
using NUnit.Framework;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
public class SequenceTests
{
    public static IEnumerable<short[]> SequenceSource
    {
        get
        {
            yield return Array.Empty<short>();
            yield return new short[] { 10, 56 };
        }
    }

    [TestCaseSource(nameof(SequenceSource))]
    public void Sequence_ice_encode_and_icerpc_decode(short[] value)
    {
        short[] decodedValue = value.IceEncodeAndIceRpcDecode(
            ShortSeqHelper.write,
            (ref SliceDecoder decoder) => decoder.DecodeSequence<short>());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    [TestCaseSource(nameof(SequenceSource))]
    public void Sequence_icerpc_encode_and_ice_decode(short[] value)
    {
        short[] decodedValue = value.IceRpcEncodeAndIceDecode(
            (ref SliceEncoder encoder, short[] value) => encoder.EncodeSequence(value),
            ShortSeqHelper.read);

        Assert.That(decodedValue, Is.EqualTo(value));
    }
}
