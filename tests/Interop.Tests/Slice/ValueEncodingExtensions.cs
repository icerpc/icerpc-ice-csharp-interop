// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc.Slice;
using System.Buffers;
using System.IO.Pipelines;

namespace Interop.Tests;

/// <summary>Provides extension methods to encode or decode a value with an Ice encoding or decoding function (possibly
/// generated from a .ice file) and decode or encode it with an IceRPC decoding or encoding function (possibly generated
/// from a .slice file).</summary>
public static class ValueEncodingExtensions
{
    /// <summary>Encodes a value with <see cref="SliceEncoder" /> and decodes it with <see cref="InputStream" />.
    /// Depending on the value type, the encoding or decoding functions can be a .ice or a .slice generated function.
    /// </summary>
    /// <param name="value">The value to encode with the Slice encoder.</param>
    /// <param name="encodeAction">The function to encode the value with the IceRPC encoder.</param>
    /// <param name="decodeFunc">The function to decode the value with the Ice decoder.</param>
    /// <returns>The decoded value.</returns>
    /// <remarks>This method should only be used for types that don't require an Ice communicator for the decoding.
    /// </remarks>
    public static T SliceToIce<T>(
        this T value,
        EncodeAction<T> encodeAction,
        Func<InputStream, T> decodeFunc)
    {
        var pipe = new Pipe();
        var encoder = new SliceEncoder(pipe.Writer, SliceEncoding.Slice1);
        encodeAction(ref encoder, value);
        pipe.Writer.Complete();
        pipe.Reader.TryRead(out ReadResult readResult);

        var inputStream = new InputStream(readResult.Buffer.ToArray());
        pipe.Reader.Complete();

        return decodeFunc(inputStream);
    }

    /// <summary>Encodes a value with <see cref="OutputStream" /> and decodes it with <see cref="SliceDecoder" />.
    /// Depending on the value type, the encoding or decoding functions can be a .ice or a .slice generated function.
    /// </summary>
    /// <param name="value">The value to encode with the Slice encoder.</param>
    /// <param name="encodeAction">The function to encode the value with the Ice encoder.</param>
    /// <param name="decodeFunc">The function to decode the value with the IceRpc decoder.</param>
    /// <returns>The decoded value.</returns>
    /// <remarks>This method should only be used for types that don't require an Ice communicator for the encoding.
    /// </remarks>
    public static T IceToSlice<T>(
        this T value,
        Action<OutputStream, T> encodeAction,
        DecodeFunc<T> decodeFunc)
    {
        var outputStream = new OutputStream();
        encodeAction(outputStream, value);
        byte[] buffer = outputStream.finished();

        var decoder = new SliceDecoder(buffer, SliceEncoding.Slice1);
        return decodeFunc(ref decoder);
    }
}
