// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc.Ice.Codec;
using System.Buffers;
using System.IO.Pipelines;

namespace Interop.Tests.Generator;

/// <summary>Provides extension methods to encode or decode a value using Ice APIs and decode or encode the same value
/// with IceRPC APIs.</summary>
internal static class ValueEncodingExtensions
{
    /// <summary>Encodes a value with <see cref="IceEncoder" /> and decodes it with <see cref="InputStream" />.
    /// </summary>
    /// <param name="value">The value to encode with IceRPC's encoder.</param>
    /// <param name="communicator">The Ice communicator used to decode the value.</param>
    /// <param name="encodeAction">The function to encode the value with the IceRPC encoder.</param>
    /// <param name="decodeFunc">The function to decode the value with the Ice decoder.</param>
    /// <returns>The decoded value.</returns>
    internal static T IceRpcToIce<T>(
        this T value,
        Communicator communicator,
        EncodeAction<T> encodeAction,
        Func<InputStream, T> decodeFunc)
    {
        var pipe = new Pipe();
        var encoder = new IceEncoder(pipe.Writer);
        encodeAction(ref encoder, value);
        pipe.Writer.Complete();
        pipe.Reader.TryRead(out ReadResult readResult);

        var inputStream = new InputStream(communicator, readResult.Buffer.ToArray());
        pipe.Reader.Complete();

        return decodeFunc(inputStream);
    }

    /// <summary>Encodes a value with <see cref="OutputStream" /> and decodes it with <see cref="IceDecoder" />.
    /// </summary>
    /// <param name="value">The value to encode with IceRPC's encoder.</param>
    /// <param name="encodeAction">The function to encode the value with the Ice encoder.</param>
    /// <param name="decodeFunc">The function to decode the value with the IceRPC decoder.</param>
    /// <returns>The decoded value.</returns>
    internal static T IceToIceRpc<T>(
        this T value,
        Action<OutputStream, T> encodeAction,
        DecodeFunc<T> decodeFunc)
    {
        var outputStream = new OutputStream();
        encodeAction(outputStream, value);
        byte[] buffer = outputStream.finished();

        var decoder = new IceDecoder(buffer);
        return decodeFunc(ref decoder);
    }
}
