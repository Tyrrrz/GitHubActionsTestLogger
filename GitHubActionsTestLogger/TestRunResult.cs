using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger;

internal record TestRunResult(IReadOnlyList<TestResult> TestResults, TimeSpan OverallDuration)
{
    public int PassedTestCount => TestResults.Count(r => r.Outcome == TestOutcome.Passed);

    public int FailedTestCount => TestResults.Count(r => r.Outcome == TestOutcome.Failed);

    public int SkippedTestCount => TestResults.Count(r => r.Outcome == TestOutcome.Skipped);

    public int TotalTestCount => TestResults.Count;

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