// Copyright (c) ZeroC, Inc.

using Ice;

namespace Interop.Tests;

/// <summary>Provide extension methods for byte buffers.</summary>
public static class BufferExtensions
{
    /// <summary>Wraps a buffer inside a "1.1" encapsulation.</summary>
    public static byte[] ToEncapsulation(this byte[] payload)
    {
        var outputStream = new OutputStream();
        outputStream.startEncapsulation();
        outputStream.writeBlob(payload);
        outputStream.endEncapsulation();
        return outputStream.finished();
    }
}
