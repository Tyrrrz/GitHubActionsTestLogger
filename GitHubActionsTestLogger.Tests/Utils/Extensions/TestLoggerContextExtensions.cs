using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using TestOutcome = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome;

namespace GitHubActionsTestLogger.Tests.Utils.Extensions;

internal static class TestLoggerContextExtensions
{
    public static void SimulateTestRun(
        this TestLoggerContext context,
        string testSuiteFilePath,
        string targetFrameworkName,
        params IReadOnlyList<LoggerTestResult> testResults
    )
    {
        var testRunCriteria = VSTestConverter.ConvertTestRunCriteria(
            new TestRunCriteria(
                [testSuiteFilePath],
                1,
                true,
                // lang=xml
                $"""
                <RunSettings>
                    <RunConfiguration>
                        <TargetFrameworkVersion>{targetFrameworkName}</TargetFrameworkVersion>
                    </RunConfiguration>
                </RunSettings>
                """
            )
        );
        context.HandleTestRunStart(testRunCriteria);

        foreach (var testResult in testResults)
            context.HandleTestResult(testResult);

        context.HandleTestRunComplete(
            VSTestConverter.ConvertTestRunComplete(
                new TestRunCompleteEventArgs(
                    new TestRunStatistics(
                        new Dictionary<TestOutcome, long>
                        {
                            [TestOutcome.Passed] = testResults.Count(r =>
                                r.Outcome == LoggerTestOutcome.Passed
                            ),
                            [TestOutcome.Failed] = testResults.Count(r =>
                                r.Outcome == LoggerTestOutcome.Failed
                            ),
                            [TestOutcome.Skipped] = testResults.Count(r =>
                                r.Outcome == LoggerTestOutcome.Skipped
                            ),
                            [TestOutcome.None] = testResults.Count(r =>
                                r.Outcome == LoggerTestOutcome.None
                            ),
                        }
                    ),
                    false,
                    false,
                    null,
                    new Collection<AttachmentSet>(),
                    TimeSpan.FromSeconds(1.234)
                )
            )
        );
    }

    public static void SimulateTestRun(
        this TestLoggerContext context,
        string testSuiteName,
        params IReadOnlyList<LoggerTestResult> testResults
    ) => context.SimulateTestRun(testSuiteName, "FakeTargetFramework", testResults);

    public static void SimulateTestRun(
        this TestLoggerContext context,
        params IReadOnlyList<LoggerTestResult> testResults
    ) => context.SimulateTestRun("FakeTests.dll", testResults);
}
