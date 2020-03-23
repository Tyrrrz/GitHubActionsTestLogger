using System;
using System.Globalization;

namespace GitHubActionsTestLogger.Internal
{
    internal static class Extensions
    {
        public static string SubstringUntil(this string str, string sub,
            StringComparison comparison = StringComparison.Ordinal)
        {
            var index = str.IndexOf(sub, comparison);
            return index < 0 ? str : str.Substring(0, index);
        }

        public static int? ParseNullableIntOrDefault(this string? str) =>
            int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
                ? result
                : (int?) null;
    }
}