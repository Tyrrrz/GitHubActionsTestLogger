using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHubActionsTestLogger.Reporting;

internal record TestRunEndInfo(
    TestRunStartInfo StartInfo,
    IReadOnlyList<TestResult> TestResults,
    TimeSpan Duration
)
{
    public int TotalTestCount => TestResults.Count;

    public int PassedTestCount => TestResults.Count(r => r.Outcome == TestOutcome.Passed);

    public int FailedTestCount => TestResults.Count(r => r.Outcome == TestOutcome.Failed);

    public int SkippedTestCount => TestResults.Count(r => r.Outcome == TestOutcome.Skipped);

    public TestOutcome OverallOutcome =>
        true switch
        {
            _ when FailedTestCount > 0 => TestOutcome.Failed,
            _ when PassedTestCount > 0 => TestOutcome.Passed,
            _ when SkippedTestCount > 0 => TestOutcome.Skipped,
            _ => TestOutcome.None,
        };
}
