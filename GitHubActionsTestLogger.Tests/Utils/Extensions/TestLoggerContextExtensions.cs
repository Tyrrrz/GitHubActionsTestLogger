using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using TestOutcome = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome;

namespace GitHubActionsTestLogger.Tests.Utils.Extensions;

internal static class TestLoggerContextExtensions
{
    public static void SimulateTestRun(
        this VSTestTestLoggerContext context,
        string testSuiteFilePath,
        string targetFrameworkName,
        params IReadOnlyList<TestResult> testResults
    )
    {
        context.HandleTestRunStart(
            new TestRunStartEventArgs(
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
            )
        );

        foreach (var testResult in testResults)
            context.HandleTestResult(new TestResultEventArgs(testResult));

        context.HandleTestRunComplete(
            new TestRunCompleteEventArgs(
                new TestRunStatistics(
                    new Dictionary<
                        Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome,
                        long
                    >
                    {
                        [Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Passed] =
                            testResults.Count(r =>
                                r.Outcome
                                == Microsoft
                                    .VisualStudio
                                    .TestPlatform
                                    .ObjectModel
                                    .TestOutcome
                                    .Passed
                            ),
                        [Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Failed] =
                            testResults.Count(r =>
                                r.Outcome
                                == Microsoft
                                    .VisualStudio
                                    .TestPlatform
                                    .ObjectModel
                                    .TestOutcome
                                    .Failed
                            ),
                        [Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Skipped] =
                            testResults.Count(r =>
                                r.Outcome
                                == Microsoft
                                    .VisualStudio
                                    .TestPlatform
                                    .ObjectModel
                                    .TestOutcome
                                    .Skipped
                            ),
                        [Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.None] =
                            testResults.Count(r =>
                                r.Outcome
                                == Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.None
                            ),
                    }
                ),
                false,
                false,
                null,
                new Collection<AttachmentSet>(),
                TimeSpan.FromSeconds(1.234)
            )
        );
    }

    public static void SimulateTestRun(
        this VSTestTestLoggerContext context,
        string testSuiteName,
        params IReadOnlyList<TestResult> testResults
    ) => context.SimulateTestRun(testSuiteName, "FakeTargetFramework", testResults);

    public static void SimulateTestRun(
        this VSTestTestLoggerContext context,
        params IReadOnlyList<TestResult> testResults
    ) => context.SimulateTestRun("FakeTests.dll", testResults);
}
