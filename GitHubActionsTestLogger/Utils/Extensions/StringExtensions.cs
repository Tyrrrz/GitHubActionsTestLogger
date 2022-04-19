using System;
using System.Collections.Generic;
using System.Globalization;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class StringExtensions
{
    public static string SubstringUntil(this string str, string sub,
        StringComparison comparison = StringComparison.Ordinal)
    {
        var index = str.IndexOf(sub, comparison);
        return index < 0
            ? str
            : str.Substring(0, index);
    }

    public static string SubstringUntilLast(this string str, string sub,
        StringComparison comparison = StringComparison.Ordinal)
    {
        var index = str.LastIndexOf(sub, comparison);
        return index < 0
            ? str
            : str.Substring(0, index);
    }

    public static string SubstringAfterLast(this string str, string sub,
        StringComparison comparison = StringComparison.Ordinal)
    {
        var index = str.LastIndexOf(sub, comparison);
        return index >= 0
            ? str.Substring(index + sub.Length, str.Length - index - sub.Length)
            : "";
    }

    public static bool? TryParseBool(this string? str) =>
        bool.TryParse(str, out var result)
            ? result
            : null;

    public static int? TryParseInt(this string? str) =>
        int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;

    public static string GetLongestCommonPrefix(
        this IEnumerable<string> strings,
        StringComparison comparison = StringComparison.Ordinal)
    {
        var isFirst = true;
        var prefix = "";

        foreach (var str in strings)
        {
            if (isFirst)
            {
                isFirst = false;
                prefix = str;
            }
            else
            {
                while (prefix.Length > 0 && str.StartsWith(prefix, comparison))
                    prefix = prefix.Substring(0, prefix.Length - 1);
            }
        }

        return prefix;
    }
}