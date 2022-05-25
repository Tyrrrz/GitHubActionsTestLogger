using System;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class TimeSpanExtensions
{
    public static string ToHumanString(this TimeSpan timeSpan)
    {
        if (timeSpan.TotalSeconds <= 1)
            return timeSpan.Milliseconds + "ms";

        if (timeSpan.TotalMinutes <= 1)
            return timeSpan.Seconds + "s";

        if (timeSpan.TotalHours <= 1)
            return timeSpan.Minutes + "m " + timeSpan.Seconds + "s";

        return timeSpan.Hours + "h " + timeSpan.Minutes + "m";
    }
}