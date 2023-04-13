// Copyright (c) ZeroC, Inc.

using Ice;
using IceRpc.Slice;
using System.Buffers;
using System.IO.Pipelines;

namespace Interop.Tests;

/// <summary>Provides extension methods to encode a value with Ice encoder and decode it with the IceRPC decoder or
/// vice-versa.</summary>
public static class ValueEncodingExtensions
{
    /// <summary>Encodes a value with <see cref="SliceEncoder" /> and decodes it with <see cref="InputStream" />.
    /// </summary>
    /// <param name="value">The value to encode with the Slice encoder.</param>
    /// <param name="encodeAction">The function to encode the value with the IceRPC encoder.</param>
    /// <param name="decodeFunc">The function to decode the value with the Ice decoder.</param>
    /// <returns>The decoded value.</returns>
    public static T IceRpcEncodeAndIceDecode<T>(
        this T value,
        EncodeAction<T> encodeAction,
        Func<InputStream, T> decodeFunc)
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

    /// <summary>Encodes a value with <see cref="OutputStream" /> and decodes it with <see cref="SliceDecoder" />.
    /// </summary>
    /// <param name="value">The value to encode with the Slice encoder.</param>
    /// <param name="encodeAction">The function to encode the value with the IceRPC encoder.</param>
    /// <param name="decodeFunc">The function to decode the value with the Ice decoder.</param>
    /// <returns>The decoded value.</returns>
    public static T IceEncodeAndIceRpcDecode<T>(
        this T value,
        Action<OutputStream, T> encodeAction,
        DecodeFunc<T> decodeFunc)
    {
        using Communicator communicator = Util.initialize();
        var outputStream = new OutputStream(communicator);
        encodeAction(outputStream, value);
        byte[] buffer = outputStream.finished();

        var decoder = new SliceDecoder(buffer, SliceEncoding.Slice1);
        return decodeFunc(ref decoder);
    }
}
