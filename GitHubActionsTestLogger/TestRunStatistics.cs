using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger;

internal class TestRunStatistics
{
    public required long PassedTestCount { get; init; }
    public required long FailedTestCount { get; init; }
    public required long SkippedTestCount { get; init; }
    public required long TotalTestCount { get; init; }
    public required TimeSpan ElapsedDuration { get; init; }

    public TestOutcome OverallOutcome
    {
        get
        {
            if (FailedTestCount > 0)
                return TestOutcome.Failed;

            if (PassedTestCount > 0)
                return TestOutcome.Passed;

            if (SkippedTestCount > 0)
                return TestOutcome.Skipped;

            return TestOutcome.None;
        }
    }
}