using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace GitHubActionsTestLogger;

public class TestLoggerContext
{
    private readonly GitHubWorkflow _github;

    private readonly object _lock = new();
    private TestRunCriteria? _testRunCriteria;
    private readonly List<TestResult> _testResults = new();

    public TestLoggerOptions Options { get; }

    public TestLoggerContext(GitHubWorkflow github, TestLoggerOptions options)
    {
        _github = github;
        Options = options;
    }

    public void HandleTestRunStart(TestRunStartEventArgs args)
    {
        lock (_lock)
        {
            _testRunCriteria = args.TestRunCriteria;
        }
    }

    public void HandleTestResult(TestResultEventArgs args)
    {
        lock (_lock)
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
    }

    public void HandleTestRunComplete(TestRunCompleteEventArgs args)
    {
        lock (_lock)
        {
            // TestRunStart event sometimes doesn't fire, which means _testRunCriteria may be null
            // https://github.com/microsoft/vstest/issues/3121

            var suiteName =
                _testRunCriteria?.Sources.FirstOrDefault()?.Pipe(Path.GetFileNameWithoutExtension) ??
                "Unknown Test Suite";

            var frameworkName =
                _testRunCriteria?.TryGetTargetFramework() ??
                "Unknown Target Framework";

            _github.ReportSummary(
                TestSummary.Generate(suiteName, frameworkName, _testResults)
            );
        }
    }
}