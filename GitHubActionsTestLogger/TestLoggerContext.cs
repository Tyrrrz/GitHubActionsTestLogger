using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GitHubActionsTestLogger.Utils.Extensions;

namespace GitHubActionsTestLogger;

public class TestLoggerContext(GitHubWorkflow github, TestLoggerOptions options)
{
    private readonly Lock _lock = new();
    private LoggerTestRunCriteria? _testRunCriteria;
    private readonly List<LoggerTestResult> _testResults = [];

    public TestLoggerOptions Options { get; } = options;

    private string FormatAnnotation(string format, LoggerTestResult testResult)
    {
        var buffer = new StringBuilder(format);

        // Escaped new line token (backwards compat)
        buffer.Replace("\\n", "\n");

        // Name token
        buffer
            .Replace("@test", testResult.DisplayName)
            // Backwards compat
            .Replace("$test", testResult.DisplayName);

        // Trait tokens
        foreach (var trait in testResult.Traits.Union(testResult.Traits))
        {
            buffer
                .Replace($"@traits.{trait.Key}", trait.Value)
                // Backwards compat
                .Replace($"$traits.{trait.Key}", trait.Value);
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
            .Replace("@framework", _testRunCriteria?.TargetFramework ?? "")
            // Backwards compat
            .Replace("$framework", _testRunCriteria?.TargetFramework ?? "");

        return buffer.Trim().ToString();
    }

    private string FormatAnnotationTitle(LoggerTestResult testResult) =>
        FormatAnnotation(Options.AnnotationTitleFormat, testResult);

    private string FormatAnnotationMessage(LoggerTestResult testResult) =>
        FormatAnnotation(Options.AnnotationMessageFormat, testResult);

    public void HandleTestRunStart(LoggerTestRunCriteria testRunCriteria)
    {
        using (_lock.EnterScope())
        {
            _testRunCriteria = testRunCriteria;
        }
    }

    public void HandleTestResult(LoggerTestResult testResult)
    {
        using (_lock.EnterScope())
        {
            // Report failed test results to job annotations
            if (testResult.Outcome == LoggerTestOutcome.Failed)
            {
                github.CreateErrorAnnotation(
                    FormatAnnotationTitle(testResult),
                    FormatAnnotationMessage(testResult),
                    testResult.SourceFilePath,
                    testResult.SourceFileLine
                );
            }

            // Record all test results to write them to the summary later
            _testResults.Add(testResult);
        }
    }

    public void HandleTestRunComplete(LoggerTestRunComplete args)
    {
        using (_lock.EnterScope())
        {
            var testSuite =
                _testRunCriteria?.Sources?.FirstOrDefault()?.Pipe(Path.GetFileNameWithoutExtension)
                ?? "Unknown Test Suite";

            var targetFramework = _testRunCriteria?.TargetFramework ?? "Unknown Target Framework";

            var testRunStatistics = new LoggerTestRunStatistics(
                (int?)args.PassedTests
                    ?? _testResults.Count(r => r.Outcome == LoggerTestOutcome.Passed),
                (int?)args.FailedTests
                    ?? _testResults.Count(r => r.Outcome == LoggerTestOutcome.Failed),
                (int?)args.SkippedTests
                    ?? _testResults.Count(r => r.Outcome == LoggerTestOutcome.Skipped),
                (int?)args.ExecutedTests ?? _testResults.Count,
                args.ElapsedTimeInRunningTests
            );

            var testResults = _testResults
                .Where(r =>
                    r.Outcome == LoggerTestOutcome.Failed
                    || r.Outcome == LoggerTestOutcome.Passed && Options.SummaryIncludePassedTests
                    || r.Outcome == LoggerTestOutcome.Skipped && Options.SummaryIncludeSkippedTests
                )
                .ToArray();

            var template = new TestSummaryTemplate
            {
                TestSuite = testSuite,
                TargetFramework = targetFramework,
                TestRunStatistics = testRunStatistics,
                TestResults = testResults,
            };

            if (
                !Options.SummaryIncludeNotFoundTests
                && testRunStatistics.OverallOutcome == LoggerTestOutcome.NotFound
            )
            {
                return;
            }

            github.CreateSummary(template.Render());
        }
    }
}
