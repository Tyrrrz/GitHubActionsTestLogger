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

namespace GitHubActionsTestLogger;

[FriendlyName("GitHubActions")]
[ExtensionUri("logger://tyrrrz/ghactions/v3")]
public class VsTestLogger : ITestLoggerWithParameters
{
    private TestReportingContext? _context;
    private TestRunStartInfo? _testRunStartInfo;
    private List<TestResult> _testResults = [];

    private void Initialize(TestLoggerEvents events, TestReportingContext context)
    {
        _context = context;

        events.TestRunStart += (_, args) => OnTestRunStart(args);
        events.TestResult += (_, args) => OnTestResult(args);
        events.TestRunComplete += (_, args) => OnTestRunComplete(args);
    }

    public void Initialize(
        TestLoggerEvents events,
        Dictionary<string, string?> parameters,
        TextWriter commandWriter,
        TextWriter summaryWriter
    )
    {
        var options = new TestReportingOptions
        {
            AnnotationTitleFormat =
                parameters.GetValueOrDefault("annotations-title")
                ?? TestReportingOptions.Default.AnnotationTitleFormat,
            AnnotationMessageFormat =
                parameters.GetValueOrDefault("annotations-message")
                ?? TestReportingOptions.Default.AnnotationMessageFormat,
            SummaryAllowEmpty =
                parameters.GetValueOrDefault("summary-allow-empty")?.Pipe(bool.Parse)
                ?? TestReportingOptions.Default.SummaryAllowEmpty,
            SummaryIncludePassedTests =
                parameters.GetValueOrDefault("summary-include-passed")?.Pipe(bool.Parse)
                ?? TestReportingOptions.Default.SummaryIncludePassedTests,
            SummaryIncludeSkippedTests =
                parameters.GetValueOrDefault("summary-include-skipped")?.Pipe(bool.Parse)
                ?? TestReportingOptions.Default.SummaryIncludeSkippedTests,
        };

        var context = new TestReportingContext(
            new GitHubWorkflow(commandWriter, summaryWriter),
            options
        );

        Initialize(events, context);
    }

    public void Initialize(TestLoggerEvents events, Dictionary<string, string?> parameters) =>
        Initialize(
            events,
            parameters,
            GitHubWorkflow.DefaultCommandWriter,
            GitHubWorkflow.DefaultSummaryWriter
        );

    public void Initialize(TestLoggerEvents events, string testRunDirectory) =>
        Initialize(events, []);

    private void OnTestRunStart(TestRunStartEventArgs args)
    {
        if (_context is null)
            throw new InvalidOperationException("Logger is not initialized.");

        var testRunStartInfo = new TestRunStartInfo(
            args.TestRunCriteria.TestSessionInfo?.Id.ToString() ?? Guid.NewGuid().ToString(),
            args.TestRunCriteria.Sources?.FirstOrDefault()?.Pipe(Path.GetFileNameWithoutExtension),
            args.TestRunCriteria.TryGetTargetFramework()
        );

        _testRunStartInfo = testRunStartInfo;
        _testResults = [];

        _context.HandleTestRunStartAsync(testRunStartInfo).GetAwaiter().GetResult();
    }

    private void OnTestResult(TestResultEventArgs args)
    {
        if (_context is null)
            throw new InvalidOperationException("Logger is not initialized.");

        var testDefinition = new TestDefinition(
            args.Result.TestCase.Id.ToString(),
            args.Result.TestCase.DisplayName,
            new SymbolReference(
                args.Result.TestCase.GetMinimallyQualifiedName(),
                args.Result.TestCase.FullyQualifiedName
            ),
            new SymbolReference(
                args.Result.TestCase.GetTypeMinimallyQualifiedName(),
                args.Result.TestCase.GetTypeFullyQualifiedName()
            ),
            args.Result.TryGetSourceFilePath(),
            args.Result.TryGetSourceLine()
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

        _testResults.Add(testResult);
        _context.HandleTestResultAsync(testResult).GetAwaiter().GetResult();
    }

    private void OnTestRunComplete(TestRunCompleteEventArgs args)
    {
        if (_context is null)
            throw new InvalidOperationException("Logger is not initialized.");

        if (_testRunStartInfo is null)
            throw new InvalidOperationException("Test run has not been started.");

        var testRunEndInfo = new TestRunEndInfo(
            _testRunStartInfo,
            _testResults,
            args.ElapsedTimeInRunningTests
        );

        _context.HandleTestRunEndAsync(testRunEndInfo).GetAwaiter().GetResult();
    }
}
