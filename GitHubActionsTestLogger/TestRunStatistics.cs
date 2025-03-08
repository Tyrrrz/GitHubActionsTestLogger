using System;

namespace GitHubActionsTestLogger;

public record LoggerTestRunStatistics(
    int PassedTestCount,
    int FailedTestCount,
    int SkippedTestCount,
    int TotalTestCount,
    TimeSpan OverallDuration
)
{
    public LoggerTestOutcome OverallOutcome { get; } =
        true switch
        {
            _ when FailedTestCount > 0 => LoggerTestOutcome.Failed,
            _ when PassedTestCount > 0 => LoggerTestOutcome.Passed,
            _ when SkippedTestCount > 0 => LoggerTestOutcome.Skipped,
            _ when TotalTestCount == 0 => LoggerTestOutcome.NotFound,
            _ => LoggerTestOutcome.None,
        };
}
