using System;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class TimeSpanExtensions
{
    public static string ToHumanString(this TimeSpan timeSpan) => timeSpan switch
    {
        { TotalSeconds: <= 1 } t => t.Milliseconds + "ms",
        { TotalMinutes: <= 1 } t => t.Seconds + "s",
        { TotalHours: <= 1 } t => t.Minutes + "m" + t.Seconds + "s",
        var t => t.Hours + "h " + t.Minutes + "m"
    };
}