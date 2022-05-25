// ReSharper disable CheckNamespace

#if NETSTANDARD2_0 || NET451
using System;
using System.Collections.Generic;
using System.Text;

internal static class StringPolyfills
{
    public static bool Contains(this string str, string sub,
        StringComparison comparison = StringComparison.Ordinal) =>
        str.IndexOf(sub, comparison) >= 0;
}

internal static class StringBuilderPolyfills
{
    public static StringBuilder AppendJoin<T>(this StringBuilder builder, string separator, IEnumerable<T> source) =>
        builder.Append(string.Join(separator, source));
}

namespace System.Collections.Generic
{
    internal static class CollectionPolyfills
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dic, TKey key) =>
            dic.TryGetValue(key!, out var result) ? result! : default!;
    }
}
#endif