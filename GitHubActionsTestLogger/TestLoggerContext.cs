using System;
using System.Collections.Generic;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

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

    public void HandleTestRunStart(TestRunCriteria testRunCriteria) =>
        _testRunCriteria = testRunCriteria;

    public void HandleTestResult(TestResult testResult)
    {
        _testResults.Add(testResult);

        if (testResult.Outcome == TestOutcome.Failed)
        {
            _github.ReportError(
                TestResultFormat.Apply(Options.AnnotationTitleFormat, testResult),
                TestResultFormat.Apply(Options.AnnotationMessageFormat, testResult),
                testResult.TryGetSourceFilePath(),
                testResult.TryGetSourceLine()
            );
        }
    }

    public void HandleTestRunComplete(ITestRunStatistics testRunStatistics, TimeSpan testRunElapsedTime)
    {
        if (_testRunCriteria is null)
            return;

        _github.ReportSummary(
            TestSummary.Generate(
                _testRunCriteria,
                testRunStatistics,
                testRunElapsedTime,
                _testResults
            )
        );
    }
}