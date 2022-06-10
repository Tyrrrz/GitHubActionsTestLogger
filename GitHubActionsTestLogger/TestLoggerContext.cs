using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

    private string ApplyAnnotationFormat(string format, TestResult testResult)
    {
        var buffer = new StringBuilder(format);

        // New line token (don't include caret return for consistency across platforms)
        buffer.Replace("\\n", "\n");

        // Name token
        buffer.Replace("$test", testResult.TestCase.DisplayName ?? "");

        // Traits tokens
        foreach (var trait in testResult.Traits.Union(testResult.TestCase.Traits))
            buffer.Replace($"$traits.{trait.Name}", trait.Value);

        // Error message
        buffer.Replace("$error", testResult.ErrorMessage ?? "");

        // Error trace
        buffer.Replace("$trace", testResult.ErrorStackTrace ?? "");

        // Target framework
        buffer.Replace("$framework", _testRunCriteria?.TryGetTargetFramework() ?? "");

        return buffer.Trim().ToString();
    }

    private string ApplyAnnotationTitleFormat(TestResult testResult) =>
        ApplyAnnotationFormat(Options.AnnotationTitleFormat, testResult);

    private string ApplyAnnotationMessageFormat(TestResult testResult) =>
        ApplyAnnotationFormat(Options.AnnotationMessageFormat, testResult);

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
                _github.CreateErrorAnnotation(
                    ApplyAnnotationTitleFormat(args.Result),
                    ApplyAnnotationMessageFormat(args.Result),
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
            // Note: it might be an issue only on this repo, when using coverlet with the logger
            // https://twitter.com/Tyrrrz/status/1530141770788610048

            var testSuiteName =
                _testRunCriteria?.Sources.FirstOrDefault()?.Pipe(Path.GetFileNameWithoutExtension) ??
                "Unknown Test Suite";

            var targetFrameworkName =
                _testRunCriteria?.TryGetTargetFramework() ??
                "Unknown Target Framework";

            var testRunStatistics = new TestRunStatistics(
                args.TestRunStatistics[TestOutcome.Passed],
                args.TestRunStatistics[TestOutcome.Failed],
                args.TestRunStatistics[TestOutcome.Skipped],
                args.TestRunStatistics.ExecutedTests,
                args.ElapsedTimeInRunningTests
            );

            _github.CreateSummary(
                TestSummary.Generate(
                    testSuiteName,
                    targetFrameworkName,
                    testRunStatistics,
                    _testResults
                )
            );
        }
    }
}