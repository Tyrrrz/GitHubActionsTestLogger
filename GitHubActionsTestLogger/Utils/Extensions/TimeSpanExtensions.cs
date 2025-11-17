using System;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class TimeSpanExtensions
{
    extension(TimeSpan timeSpan)
    {
        public string ToHumanString() =>
            timeSpan switch
            {
                { TotalSeconds: <= 1 } => timeSpan.Milliseconds + "ms",
                { TotalMinutes: <= 1 } => timeSpan.Seconds + "s",
                { TotalHours: <= 1 } => timeSpan.Minutes + "m" + timeSpan.Seconds + "s",
                _ => timeSpan.Hours + "h" + timeSpan.Minutes + "m",
            };
    }
}
