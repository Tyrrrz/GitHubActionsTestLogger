// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming

#if NETSTANDARD2_0 || NET462
using System;
using System.Collections.Generic;
using System.Text;

internal static class StringPolyfills
{
    public static bool Contains(this string str, string sub,
        StringComparison comparison = StringComparison.Ordinal) =>
        str.IndexOf(sub, comparison) >= 0;
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