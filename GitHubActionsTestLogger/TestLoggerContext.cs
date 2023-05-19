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

    private string FormatAnnotation(string format, TestResult testResult)
    {
        var buffer = new StringBuilder(format);

        // New line token (don't include caret return for consistency across platforms)
        buffer.Replace("\\n", "\n");

        // Name token
        buffer.Replace("$test", testResult.TestCase.DisplayName ?? "");

        // Trait tokens
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

    private string FormatAnnotationTitle(TestResult testResult) =>
        FormatAnnotation(Options.AnnotationTitleFormat, testResult);

    private string FormatAnnotationMessage(TestResult testResult) =>
        FormatAnnotation(Options.AnnotationMessageFormat, testResult);

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
                    FormatAnnotationTitle(args.Result),
                    FormatAnnotationMessage(args.Result),
                    args.Result.TryGetSourceFilePath(),
                    args.Result.TryGetSourceLine()
                );
            }

            // Record all test results to write them to the summary later
            _testResults.Add(args.Result);
        }
    }

    public void HandleTestRunComplete(TestRunCompleteEventArgs args)
    {
        lock (_lock)
        {
            var template = new TestSummaryTemplate
            {
                TestSuite =
                    _testRunCriteria?.Sources?.FirstOrDefault()?.Pipe(Path.GetFileNameWithoutExtension) ??
                    "Unknown Test Suite",

                TargetFramework =
                    _testRunCriteria?.TryGetTargetFramework() ??
                    "Unknown Target Framework",

                TestRunStatistics = new TestRunStatistics(
                    (int?)args.TestRunStatistics?[TestOutcome.Passed] ??
                    _testResults.Count(r => r.Outcome == TestOutcome.Passed),

                    (int?)args.TestRunStatistics?[TestOutcome.Failed] ??
                    _testResults.Count(r => r.Outcome == TestOutcome.Failed),

                    (int?)args.TestRunStatistics?[TestOutcome.Skipped] ??
                    _testResults.Count(r => r.Outcome == TestOutcome.Skipped),

                    (int?)args.TestRunStatistics?.ExecutedTests ?? _testResults.Count,

                    args.ElapsedTimeInRunningTests
                ),

                TestResults = _testResults.Where(r =>
                    r.Outcome == TestOutcome.Failed ||
                    r.Outcome == TestOutcome.Passed && Options.SummaryIncludePassedTests ||
                    r.Outcome == TestOutcome.Skipped && Options.SummaryIncludeSkippedTests
                ).ToArray()
            };

            _github.CreateSummary(template.Render());
        }
    }
}