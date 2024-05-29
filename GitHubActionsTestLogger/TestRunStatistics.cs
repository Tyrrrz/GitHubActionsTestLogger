using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger;

internal record TestRunStatistics(
    int PassedTestCount,
    int FailedTestCount,
    int SkippedTestCount,
    int TotalTestCount,
    TimeSpan OverallDuration
)
{
    public TestOutcome OverallOutcome { get; } =
        true switch
        {
            _ when FailedTestCount > 0 => TestOutcome.Failed,
            _ when PassedTestCount > 0 => TestOutcome.Passed,
            _ when SkippedTestCount > 0 => TestOutcome.Skipped,
            _ when TotalTestCount == 0 => TestOutcome.NotFound,
            _ => TestOutcome.None
        };
}
