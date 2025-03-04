using System;
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

public interface ITestRunCriteria
{
    string? TargetFramework { get; }
    IEnumerable<string>? Sources { get; }
}

public sealed class VSTestTestRunCriteria : ITestRunCriteria
{
    public string? TargetFramework { get; private set; }
    public IEnumerable<string>? Sources { get; private set; }

    public static VSTestTestRunCriteria Convert(TestRunCriteria testRunCriteria)
    {
        return new VSTestTestRunCriteria
        {
            TargetFramework = testRunCriteria.TryGetTargetFramework(),
            Sources = testRunCriteria.Sources,
        };
    }
}

public interface ITestRunComplete
{
    long? PassedTests { get; }
    long? FailedTests { get; }
    long? SkippedTests { get; }
    long? ExecutedTests { get; }
    TimeSpan ElapsedTimeInRunningTests { get; }
}

public enum TestOutcome
{
    None = 0,
    Passed = 1,
    Failed = 2,
    Skipped = 3,
    NotFound = 4,
}

public interface ITestResult
{
    public string? DisplayName { get; set; }
    public string FullyQualifiedName { get; set; }
    Dictionary<string, string> Traits { get; set; }
    string? ErrorMessage { get; set; }
    string? ErrorStackTrace { get; set; }
    TestOutcome Outcome { get; set; }

    string? SourceFilePath { get; set; }
    int? SourceFileLine { get; set; }
    string MinimallyQualifiedName { get; set; }
}

public sealed class VSTestTestResult : ITestResult
{
    public string? DisplayName { get; set; }
    public required Dictionary<string, string> Traits { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorStackTrace { get; set; }
    public TestOutcome Outcome { get; set; }
    public string? SourceFilePath { get; set; }
    public int? SourceFileLine { get; set; }
    public required string FullyQualifiedName { get; set; }
    public required string MinimallyQualifiedName { get; set; }

    public static VSTestTestResult Convert(TestResult testResult)
    {
        return new VSTestTestResult
        {
            FullyQualifiedName = testResult.TestCase.GetTypeFullyQualifiedName(),
            MinimallyQualifiedName = testResult.TestCase.GetMinimallyQualifiedName(),
            DisplayName = testResult.TestCase.DisplayName,
            Traits = testResult.Traits.ToDictionary(t => t.Name, t => t.Value) ?? new(),
            ErrorMessage = testResult.ErrorMessage,
            ErrorStackTrace = testResult.ErrorStackTrace,
            Outcome = (TestOutcome)testResult.Outcome,
            SourceFilePath = testResult.TryGetSourceFilePath(),
            SourceFileLine = testResult.TryGetSourceLine(),
        };
    }
}

public class TestLoggerContext(GitHubWorkflow github, TestLoggerOptions options)
{
    private readonly Lock _lock = new();
    private ITestRunCriteria? _testRunCriteria;
    private readonly List<ITestResult> _testResults = [];

    public TestLoggerOptions Options { get; } = options;

    private string FormatAnnotation(string format, ITestResult testResult)
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

    private string FormatAnnotationTitle(ITestResult testResult) =>
        FormatAnnotation(Options.AnnotationTitleFormat, testResult);

    private string FormatAnnotationMessage(ITestResult testResult) =>
        FormatAnnotation(Options.AnnotationMessageFormat, testResult);

    public void HandleTestRunStart(ITestRunCriteria testRunCriteria)
    {
        using (_lock.EnterScope())
        {
            _testRunCriteria = testRunCriteria;
        }
    }

    public void HandleTestResult(ITestResult testResult)
    {
        using (_lock.EnterScope())
        {
            // Report failed test results to job annotations
            if (testResult.Outcome == TestOutcome.Failed)
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

    public void HandleTestRunComplete(ITestRunComplete args)
    {
        using (_lock.EnterScope())
        {
            var testSuite =
                _testRunCriteria?.Sources?.FirstOrDefault()?.Pipe(Path.GetFileNameWithoutExtension)
                ?? "Unknown Test Suite";

            var targetFramework = _testRunCriteria?.TargetFramework ?? "Unknown Target Framework";

            var testRunStatistics = new TestRunStatistics(
                (int?)args.PassedTests ?? _testResults.Count(r => r.Outcome == TestOutcome.Passed),
                (int?)args.FailedTests ?? _testResults.Count(r => r.Outcome == TestOutcome.Failed),
                (int?)args.SkippedTests
                    ?? _testResults.Count(r => r.Outcome == TestOutcome.Skipped),
                (int?)args.ExecutedTests ?? _testResults.Count,
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
                TestResults = testResults,
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
