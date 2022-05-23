﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace GitHubActionsTestLogger.Tests.Utils.Extensions;

internal static class TestLoggerContextExtensions
{
    public static void SimulateTestRun(
        this TestLoggerContext context,
        string suiteFilePath,
        params TestResult[] testResults)
    {
        context.HandleTestRunStart(
            new TestRunStartEventArgs(
                new TestRunCriteria(new[] { suiteFilePath }, 1)
            )
        );

        foreach (var testResult in testResults)
            context.HandleTestResult(new TestResultEventArgs(testResult));

        context.HandleTestRunComplete(
            new TestRunCompleteEventArgs(
                new TestRunStatistics(new Dictionary<TestOutcome, long>
                {
                    [TestOutcome.Passed] = testResults.Count(r => r.Outcome == TestOutcome.Passed),
                    [TestOutcome.Failed] = testResults.Count(r => r.Outcome == TestOutcome.Failed),
                    [TestOutcome.Skipped] = testResults.Count(r => r.Outcome == TestOutcome.Skipped),
                    [TestOutcome.None] = testResults.Count(r => r.Outcome == TestOutcome.None)
                }),
                false,
                false,
                null,
                new Collection<AttachmentSet>(),
                TimeSpan.FromSeconds(1.234)
            )
        );
    }

    public static void SimulateTestRun(this TestLoggerContext context, params TestResult[] testResults) =>
        context.SimulateTestRun("FakeTests.dll", testResults);
}