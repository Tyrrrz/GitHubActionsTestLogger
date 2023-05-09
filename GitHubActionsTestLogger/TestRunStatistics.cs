using System;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger;

internal record TestRunStatistics(
    long PassedTestCount,
    long FailedTestCount,
    long SkippedTestCount,
    long TotalTestCount,
    TimeSpan ElapsedDuration)
{
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