// Copyright (c) ZeroC, Inc.

using Ice;

namespace Interop.Tests.Slice;

/// <summary>Provides extension methods for <see cref="Nullable{T}" />.</summary>
public static class NullableExtensions
{
    public static Optional<T> ToOptionalValue<T>(this T? nullable) where T : struct =>
        nullable.HasValue ? nullable.Value : Util.None;

    public static Optional<T> ToOptionalReference<T>(this T? nullable) where T : class =>
        nullable is null ? Util.None : new Optional<T>(nullable);
}
