// Copyright (c) ZeroC, Inc.

using Ice;

namespace Interop.Tests.Slice;

/// <summary>Provides extension methods for <see cref="Optional{T}" />.</summary>
public static class OptionalExtensions
{
    public static T? ToNullableValue<T>(this Optional<T> optional) where T : struct =>
        optional.HasValue ? optional.Value : null;

    public static T? ToNullableReference<T>(this Optional<T> optional) where T : class =>
        optional.HasValue ? optional.Value : null;
}
