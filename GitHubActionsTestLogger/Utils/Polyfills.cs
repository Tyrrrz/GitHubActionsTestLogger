// ReSharper disable CheckNamespace

#if NETSTANDARD2_0 || NET451
using System;

internal static class PolyfillExtensions
{
    public static bool Contains(this string str, string sub,
        StringComparison comparison = StringComparison.Ordinal) =>
        str.IndexOf(sub, comparison) >= 0;
}

namespace System.Collections.Generic
{
    internal static class PolyfillExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key) =>
            dic.TryGetValue(key!, out var result) ? result! : default!;
    }
}
#endif