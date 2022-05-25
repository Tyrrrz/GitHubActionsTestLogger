using System;
using System.Collections.Generic;
using System.Linq;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace GitHubActionsTestLogger;

public class TestLoggerContext
{
    private readonly GitHubWorkflow _github;

    private TestRunCriteria? _testRunCriteria;
    private readonly List<TestResult> _testResults = new();

    public TestLoggerOptions Options { get; }

    public TestLoggerContext(GitHubWorkflow github, TestLoggerOptions options)
    {
        _github = github;
        Options = options;
    }

    public void HandleTestRunStart(TestRunStartEventArgs args) =>
        _testRunCriteria = args.TestRunCriteria;

    public void HandleTestResult(TestResultEventArgs args)
    {
        // Report failed test results to job annotations
        if (args.Result.Outcome == TestOutcome.Failed)
        {
            _github.ReportError(
                TestResultFormat.Apply(Options.AnnotationTitleFormat, args.Result),
                TestResultFormat.Apply(Options.AnnotationMessageFormat, args.Result),
                args.Result.TryGetSourceFilePath(),
                args.Result.TryGetSourceLine()
            );
        }

        // Record all test results to write them to summary later
        _testResults.Add(args.Result);
    }

    public void HandleTestRunComplete(TestRunCompleteEventArgs args)
    {
        if (!_testRunCriteria!.Sources.Any(s => s.Contains("Fake")) && !_testRunCriteria.Sources.Any(s => s.Contains("Project")))
            Console.WriteLine("test");

        // This is expected to have been set when the test run started
        if (_testRunCriteria is null)
            return;

        _github.ReportSummary(
            TestSummary.Generate(
                _testRunCriteria,
                args.TestRunStatistics,
                args.ElapsedTimeInRunningTests,
                _testResults
            )
        );
    }
}