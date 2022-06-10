using System;

namespace GitHubActionsTestLogger;

internal record TestRunStatistics(
    long PassedTestCount,
    long FailedTestCount,
    long SkippedTestCount,
    long TotalTestCount,
    TimeSpan ElapsedDuration
);