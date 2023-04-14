// Copyright (c) ZeroC, Inc.

using IceRpc.Slice;
using NUnit.Framework;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
public class DictionaryTests
{
    public static IEnumerable<Dictionary<short, short>> DictionarySource
    {
        get
        {
            yield return new Dictionary<short, short>();
            yield return new Dictionary<short, short>()
            {
                [10] = 56,
                [30] = 3
            };
        }
    }

    [TestCaseSource(nameof(DictionarySource))]
    public void Ice_dictionary_to_Slice_dictionary(Dictionary<short, short> value)
    {
        Dictionary<short, short> decodedValue = value.IceToSlice(
            ShortShortDictHelper.write,
            (ref SliceDecoder decoder) => decoder.DecodeDictionary(
                count => new Dictionary<short, short>(count),
                (ref SliceDecoder decoder) => decoder.DecodeInt16(),
                (ref SliceDecoder decoder) => decoder.DecodeInt16()));

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    [TestCaseSource(nameof(DictionarySource))]
    public void Slice_dictionary_to_ice_dictionary(Dictionary<short, short> value)
    {
        Dictionary<short, short> decodedValue = value.SliceToIce(
            (ref SliceEncoder encoder, Dictionary<short, short> value) => encoder.EncodeDictionary(
                value,
                (ref SliceEncoder encoder, short value) => encoder.EncodeInt16(value),
                (ref SliceEncoder encoder, short value) => encoder.EncodeInt16(value)),
            ShortShortDictHelper.read);

        Assert.That(decodedValue, Is.EqualTo(value));
    }
}
