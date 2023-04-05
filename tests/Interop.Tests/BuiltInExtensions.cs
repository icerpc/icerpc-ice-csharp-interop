

using Ice;

internal static class BuiltInExtensions
{
    internal static byte[] CreateEncapsulation(this byte[] payload)
    {
        var outputStream = new OutputStream();
        outputStream.startEncapsulation();
        outputStream.writeBlob(payload);
        outputStream.endEncapsulation();
        return outputStream.finished();
    }
}
