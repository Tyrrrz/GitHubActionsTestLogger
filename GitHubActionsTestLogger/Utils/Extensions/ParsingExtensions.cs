using System.Globalization;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class ParsingExtensions
{
    extension(int)
    {
        public static int? ParseOrNull(string? str) =>
            int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
                ? result
                : null;
    }
}
