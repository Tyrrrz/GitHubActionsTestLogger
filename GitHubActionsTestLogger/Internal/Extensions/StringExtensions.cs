using System;
using System.Globalization;

namespace GitHubActionsTestLogger.Internal.Extensions
{
    internal static class StringExtensions
    {
        public static bool Contains(this string str, string sub,
            StringComparison comparison = StringComparison.Ordinal) =>
            str.IndexOf(sub, comparison) >= 0;

        public static string SubstringUntil(this string str, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = str.IndexOf(sub, comparison);
            return index < 0 ? str : str.Substring(0, index);
        }

        public static string SubstringAfterLast(this string str, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = str.LastIndexOf(sub, comparison);
            return index >= 0 ? str.Substring(index + sub.Length, str.Length - index - sub.Length) : "";
        }

        public static int? ParseNullableIntOrDefault(this string? str) =>
            int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
                ? result
                : (int?) null;
    }
}