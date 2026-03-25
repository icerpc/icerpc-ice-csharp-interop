// Copyright (c) ZeroC, Inc.

using IceRpc.Ice.Codec;
using NUnit.Framework;

namespace Interop.Tests.Slice;

[Parallelizable(scope: ParallelScope.All)]
internal class DictionaryTests
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
            (ref decoder) => decoder.DecodeDictionary(
                count => new Dictionary<short, short>(count),
                (ref decoder) => decoder.DecodeShort(),
                (ref decoder) => decoder.DecodeShort()));

        Assert.That(decodedValue, Is.EqualTo(value));
    }

    [TestCaseSource(nameof(DictionarySource))]
    public void Slice_dictionary_to_ice_dictionary(Dictionary<short, short> value)
    {
        using var communicator = new Ice.Communicator();
        Dictionary<short, short> decodedValue = value.SliceToIce(
            communicator,
            (ref encoder, value) => encoder.EncodeDictionary(
                value,
                (ref encoder, value) => encoder.EncodeShort(value),
                (ref encoder, value) => encoder.EncodeShort(value)),
            ShortShortDictHelper.read);

        Assert.That(decodedValue, Is.EqualTo(value));
    }
}
