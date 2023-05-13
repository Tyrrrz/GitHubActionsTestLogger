using System;
using System.Globalization;
using System.Text;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class StringExtensions
{
    public static string SubstringUntil(
        this string str,
        string sub,
        StringComparison comparison = StringComparison.Ordinal)
    {
        var index = str.IndexOf(sub, comparison);
        return index < 0
            ? str
            : str[..index];
    }

    public static string SubstringUntilLast(
        this string str,
        string sub,
        StringComparison comparison = StringComparison.Ordinal)
    {
        var index = str.LastIndexOf(sub, comparison);
        return index < 0
            ? str
            : str[..index];
    }

    public static string SubstringAfter(
        this string str,
        string sub,
        StringComparison comparison = StringComparison.Ordinal)
    {
        var index = str.IndexOf(sub, comparison);
        return index >= 0
            ? str.Substring(index + sub.Length, str.Length - index - sub.Length)
            : "";
    }

    public static string SubstringAfterLast(
        this string str,
        string sub,
        StringComparison comparison = StringComparison.Ordinal)
    {
        var index = str.LastIndexOf(sub, comparison);
        return index >= 0
            ? str.Substring(index + sub.Length, str.Length - index - sub.Length)
            : "";
    }

    public static string Indent(this string str, int spaces)
    {
        var indentUnit = new string(' ', spaces);
        return indentUnit + str.Replace("\n", "\n" + new string(' ', spaces));
    }

    public static int? TryParseInt(this string? str) =>
        int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;

    public static StringBuilder Trim(this StringBuilder builder)
    {
        while (builder.Length > 0 && char.IsWhiteSpace(builder[0]))
            builder.Remove(0, 1);

        while (builder.Length > 0 && char.IsWhiteSpace(builder[^1]))
            builder.Remove(builder.Length - 1, 1);

        return builder;
    }
}