// Copyright (c) ZeroC, Inc.

using NUnit.Framework;
using ZeroC.Slice;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
internal class SequenceTests
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
    public void Ice_sequence_to_slice_sequence(short[] value)
    {
        short[] decodedValue = value.IceToSlice(
            ShortSeqHelper.write,
            (ref decoder) => decoder.DecodeSequence<short>());

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    [TestCaseSource(nameof(SequenceSource))]
    public void Slice_sequence_to_ice_sequence(short[] value)
    {
        using var communicator = new Ice.Communicator();
        short[] decodedValue = value.SliceToIce(
            communicator,
            (ref encoder, value) => encoder.EncodeSequence(value),
            ShortSeqHelper.read);

        Assert.That(decodedValue, Is.EqualTo(value));
    }
}
