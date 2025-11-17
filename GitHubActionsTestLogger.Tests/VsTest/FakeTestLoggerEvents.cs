using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace GitHubActionsTestLogger.Tests.VsTest;

internal class FakeTestLoggerEvents : TestLoggerEvents
{
    // Not all of these events need to be raised, but they all need to be provided for the interface
#pragma warning disable CS0067
    public override event EventHandler<TestRunMessageEventArgs>? TestRunMessage;
    public override event EventHandler<TestRunStartEventArgs>? TestRunStart;
    public override event EventHandler<TestResultEventArgs>? TestResult;
    public override event EventHandler<TestRunCompleteEventArgs>? TestRunComplete;
    public override event EventHandler<DiscoveryStartEventArgs>? DiscoveryStart;
    public override event EventHandler<TestRunMessageEventArgs>? DiscoveryMessage;
    public override event EventHandler<DiscoveredTestsEventArgs>? DiscoveredTests;
    public override event EventHandler<DiscoveryCompleteEventArgs>? DiscoveryComplete;
#pragma warning restore CS0067

    public void RaiseTestRunStart(TestRunStartEventArgs args) => TestRunStart?.Invoke(this, args);

    public void RaiseTestResult(TestResultEventArgs args) => TestResult?.Invoke(this, args);

    public void RaiseTestRunComplete(TestRunCompleteEventArgs args) =>
        TestRunComplete?.Invoke(this, args);

    public void SimulateTestRun(
        string testSuiteFilePath,
        string targetFrameworkName,
        params IReadOnlyList<TestResult> testResults
    )
    {
        RaiseTestRunStart(
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
            RaiseTestResult(new TestResultEventArgs(testResult));

        RaiseTestRunComplete(
            new TestRunCompleteEventArgs(
                new TestRunStatistics(
                    new Dictionary<TestOutcome, long>
                    {
                        [TestOutcome.Passed] = testResults.Count(r =>
                            r.Outcome == TestOutcome.Passed
                        ),
                        [TestOutcome.Failed] = testResults.Count(r =>
                            r.Outcome == TestOutcome.Failed
                        ),
                        [TestOutcome.Skipped] = testResults.Count(r =>
                            r.Outcome == TestOutcome.Skipped
                        ),
                        [TestOutcome.None] = testResults.Count(r => r.Outcome == TestOutcome.None),
                    }
                ),
                false,
                false,
                null,
                [],
                TimeSpan.FromSeconds(1.234)
            )
        );
    }

    public void SimulateTestRun(
        string testSuiteName,
        params IReadOnlyList<TestResult> testResults
    ) => SimulateTestRun(testSuiteName, "FakeTargetFramework", testResults);

    public void SimulateTestRun(params IReadOnlyList<TestResult> testResults) =>
        SimulateTestRun("FakeTests.dll", testResults);
}
