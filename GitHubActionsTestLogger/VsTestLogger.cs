using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitHubActionsTestLogger.GitHub;
using GitHubActionsTestLogger.Reporting;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using TestOutcome = GitHubActionsTestLogger.Reporting.TestOutcome;
using TestResult = GitHubActionsTestLogger.Reporting.TestResult;
using TestRunStatistics = GitHubActionsTestLogger.Reporting.TestRunStatistics;

namespace GitHubActionsTestLogger;

[FriendlyName("GitHubActions")]
[ExtensionUri("logger://tyrrrz/ghactions/v2")]
public class VsTestLogger : ITestLoggerWithParameters
{
    // VSTest may theoretically not produce test run statistics at the end of the test session, so we build it
    // manually by collecting all test results.
    private List<TestResult> _testResults = [];

    internal TestReportingContext? Context { get; private set; }

    internal void Initialize(TestLoggerEvents events, TestReportingContext context)
    {
        Context = context;

        events.TestRunStart += (_, args) => OnTestRunStart(args);
        events.TestResult += (_, args) => OnTestResult(args);
        events.TestRunComplete += (_, args) => OnTestRunComplete(args);
    }

    internal void Initialize(TestLoggerEvents events, TestReportingOptions options) =>
        Initialize(events, new TestReportingContext(GitHubWorkflow.Default, options));

    public void Initialize(TestLoggerEvents events, string testRunDirectory) =>
        Initialize(events, TestReportingOptions.Default);

    public void Initialize(TestLoggerEvents events, Dictionary<string, string?> parameters) =>
        Initialize(events, TestReportingOptions.Resolve(parameters));

    private void OnTestRunStart(TestRunStartEventArgs args)
    {
        if (Context is null)
            throw new InvalidOperationException("The logger is not initialized.");

        _testResults = [];

        Context.HandleTestRunStart(
            new TestRunStartInfo(
                args.TestRunCriteria.TestSessionInfo?.Id.ToString() ?? Guid.NewGuid().ToString(),
                args.TestRunCriteria.Sources?.FirstOrDefault()
                    ?.Pipe(Path.GetFileNameWithoutExtension),
                args.TestRunCriteria.TryGetTargetFramework()
            )
        );
    }

    private void OnTestResult(TestResultEventArgs args)
    {
        if (Context is null)
            throw new InvalidOperationException("The logger is not initialized.");

        var testDefinition = new TestDefinition(
            args.Result.TestCase.Id.ToString(),
            args.Result.TestCase.DisplayName,
            args.Result.TryGetSourceFilePath(),
            args.Result.TryGetSourceLine(),
            args.Result.TestCase.Traits.ToDictionary(t => t.Name, t => t.Value)
        );

        var testResult = new TestResult(
            testDefinition,
            args.Result.Outcome switch
            {
                Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Passed =>
                    TestOutcome.Passed,
                Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Failed =>
                    TestOutcome.Failed,
                Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Skipped =>
                    TestOutcome.Skipped,
                _ => TestOutcome.None,
            },
            args.Result.ErrorMessage,
            args.Result.ErrorStackTrace
        );

        Context.HandleTestResult(testResult);
        _testResults.Add(testResult);
    }

    private void OnTestRunComplete(TestRunCompleteEventArgs args)
    {
        if (Context is null)
            throw new InvalidOperationException("The logger is not initialized.");

        Context.HandleTestRunEnd(
            new TestRunStatistics(
                (int?)
                    args.TestRunStatistics?[
                        Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Passed
                    ] ?? _testResults.Count(r => r.Outcome == TestOutcome.Passed),
                (int?)
                    args.TestRunStatistics?[
                        Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Failed
                    ] ?? _testResults.Count(r => r.Outcome == TestOutcome.Failed),
                (int?)
                    args.TestRunStatistics?[
                        Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome.Skipped
                    ] ?? _testResults.Count(r => r.Outcome == TestOutcome.Skipped),
                (int?)args.TestRunStatistics?.ExecutedTests ?? _testResults.Count,
                args.ElapsedTimeInRunningTests
            )
        );
    }
}
