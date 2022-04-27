using System;
using System.Collections.Generic;
using System.IO;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace GitHubActionsTestLogger;

public class TestLoggerContext : IDisposable
{
    private readonly GitHubWorkflow _workflow;
    private readonly TestSummaryWriter? _summaryWriter;

    private TestRunCriteria? _testRunCriteria;
    private readonly List<TestResult> _testResults = new();

    public TestLoggerOptions Options { get; }

    public TestLoggerContext(TextWriter output, string? summaryFilePath, TestLoggerOptions options)
    {
        _workflow = new GitHubWorkflow(output);

        if (!string.IsNullOrWhiteSpace(summaryFilePath))
            _summaryWriter = new TestSummaryWriter(summaryFilePath);

        Options = options;
    }

    public void HandleTestRunStart(TestRunStartEventArgs args)
    {
        _testRunCriteria = args.TestRunCriteria;
    }

    public void HandleTestResult(TestResultEventArgs args)
    {
        var testResult = args.Result;

        _testResults.Add(testResult);

        if (testResult.Outcome == TestOutcome.Failed)
        {
            _workflow.ReportError(
                TestResultFormat.Apply(Options.AnnotationTitleFormat, testResult),
                TestResultFormat.Apply(Options.AnnotationMessageFormat, testResult),
                testResult.TryGetSourceFilePath(),
                testResult.TryGetSourceLine()
            );
        }

        // We're updating summary on every test result because there is no reliable
        // way to know when the test run has completed.
        if (_testRunCriteria is not null)
            _summaryWriter?.Update(_testRunCriteria, _testResults);
    }

    public void Dispose()
    {
        _summaryWriter?.Dispose();
    }
}