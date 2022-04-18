using System;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class TimeSpanExtensions
{
    public static string ToHumanReadableString(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalSeconds <= 1)
            return $"{timeSpan:s\\.ff} seconds";

        if (timeSpan.TotalMinutes <= 1)
            return $"{timeSpan:%s} seconds";

        if (timeSpan.TotalHours <= 1)
            return $"{timeSpan:%m} minutes";

        return $"{timeSpan:%h} hours";
    }
}