using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace GitHubActionsTestLogger;

public class TestLoggerContext(GitHubWorkflow github, TestLoggerOptions options)
{
    private readonly Lock _lock = new();
    private TestRunCriteria? _testRunCriteria;
    private readonly List<TestResult> _testResults = [];

    public TestLoggerOptions Options { get; } = options;

    private string FormatAnnotation(string format, TestResult testResult)
    {
        var buffer = new StringBuilder(format);

        // Escaped new line token (backwards compat)
        buffer.Replace("\\n", "\n");

        // Name token
        buffer
            .Replace("@test", testResult.TestCase.DisplayName)
            // Backwards compat
            .Replace("$test", testResult.TestCase.DisplayName);

        // Trait tokens
        foreach (var trait in testResult.Traits.Union(testResult.TestCase.Traits))
        {
            buffer
                .Replace($"@traits.{trait.Name}", trait.Value)
                // Backwards compat
                .Replace($"$traits.{trait.Name}", trait.Value);
        }

        // Error message
        buffer
            .Replace("@error", testResult.ErrorMessage ?? "")
            // Backwards compat
            .Replace("$error", testResult.ErrorMessage ?? "");

        // Error trace
        buffer
            .Replace("@trace", testResult.ErrorStackTrace ?? "")
            // Backwards compat
            .Replace("$trace", testResult.ErrorStackTrace ?? "");

        // Target framework
        buffer
            .Replace("@framework", _testRunCriteria?.TryGetTargetFramework() ?? "")
            // Backwards compat
            .Replace("$framework", _testRunCriteria?.TryGetTargetFramework() ?? "");

        return buffer.Trim().ToString();
    }

    private string FormatAnnotationTitle(TestResult testResult) =>
        FormatAnnotation(Options.AnnotationTitleFormat, testResult);

    private string FormatAnnotationMessage(TestResult testResult) =>
        FormatAnnotation(Options.AnnotationMessageFormat, testResult);

    public void HandleTestRunStart(TestRunStartEventArgs args)
    {
        using (_lock.EnterScope())
        {
            _testRunCriteria = args.TestRunCriteria;
        }
    }

    public void HandleTestResult(TestResultEventArgs args)
    {
        using (_lock.EnterScope())
        {
            // Report failed test results to job annotations
            if (args.Result.Outcome == TestOutcome.Failed)
            {
                github.CreateErrorAnnotation(
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
        using (_lock.EnterScope())
        {
            var testSuite =
                _testRunCriteria?.Sources?.FirstOrDefault()?.Pipe(Path.GetFileNameWithoutExtension)
                ?? "Unknown Test Suite";

            var targetFramework =
                _testRunCriteria?.TryGetTargetFramework() ?? "Unknown Target Framework";

            var testRunStatistics = new TestRunStatistics(
                (int?)args.TestRunStatistics?[TestOutcome.Passed]
                    ?? _testResults.Count(r => r.Outcome == TestOutcome.Passed),
                (int?)args.TestRunStatistics?[TestOutcome.Failed]
                    ?? _testResults.Count(r => r.Outcome == TestOutcome.Failed),
                (int?)args.TestRunStatistics?[TestOutcome.Skipped]
                    ?? _testResults.Count(r => r.Outcome == TestOutcome.Skipped),
                (int?)args.TestRunStatistics?.ExecutedTests ?? _testResults.Count,
                args.ElapsedTimeInRunningTests
            );

            var testResults = _testResults
                .Where(r =>
                    r.Outcome == TestOutcome.Failed
                    || r.Outcome == TestOutcome.Passed && Options.SummaryIncludePassedTests
                    || r.Outcome == TestOutcome.Skipped && Options.SummaryIncludeSkippedTests
                )
                .ToArray();

            var template = new TestSummaryTemplate
            {
                TestSuite = testSuite,
                TargetFramework = targetFramework,
                TestRunStatistics = testRunStatistics,
                TestResults = testResults
            };

            if (
                !Options.SummaryIncludeNotFoundTests
                && testRunStatistics.OverallOutcome == TestOutcome.NotFound
            )
            {
                return;
            }

            github.CreateSummary(template.Render());
        }
    }
}
