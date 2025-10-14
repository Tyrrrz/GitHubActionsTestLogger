using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using GitHubActionsTestLogger.Bridge;
using GitHubActionsTestLogger.Utils.Extensions;

namespace GitHubActionsTestLogger;

internal class TestReporterContext(GitHubWorkflow github, TestReporterOptions options)
{
    private readonly Lock _lock = new();
    private readonly Stopwatch _stopwatch = new();

    private TestRunStartInfo? _testRunStartInfo;
    private readonly List<TestResult> _testResults = [];

    public TestReporterOptions Options { get; } = options;

    private string FormatAnnotation(string format, TestResult testResult)
    {
        var buffer = new StringBuilder(format);

        // Escaped new line token
        buffer.Replace("\\n", "\n");

        // Name
        buffer.Replace("@test", testResult.Definition.DisplayName);

        // Properties
        foreach (var property in testResult.Definition.Properties)
            buffer.Replace($"@traits.{property.Key}", property.Value);

        // Error message
        buffer.Replace("@error", testResult.ErrorMessage ?? "");

        // Error trace
        buffer.Replace("@trace", testResult.ErrorStackTrace ?? "");

        // Target framework
        buffer.Replace("@framework", _testRunStartInfo?.FrameworkName ?? "");

        return buffer.Trim().ToString();
    }

    private string FormatAnnotationTitle(TestResult testResult) =>
        FormatAnnotation(Options.AnnotationTitleFormat, testResult);

    private string FormatAnnotationMessage(TestResult testResult) =>
        FormatAnnotation(Options.AnnotationMessageFormat, testResult);

    public void HandleTestRunStart(TestRunStartInfo startInfo)
    {
        using (_lock.EnterScope())
        {
            _stopwatch.Start();
            _testRunStartInfo = startInfo;
        }
    }

    public void HandleTestResult(TestResult testResult)
    {
        using (_lock.EnterScope())
        {
            // Report failed test results to job annotations
            if (testResult.Outcome == TestOutcome.Failed)
            {
                github.CreateErrorAnnotation(
                    FormatAnnotationTitle(testResult),
                    FormatAnnotationMessage(testResult),
                    testResult.Definition.SourceFilePath,
                    testResult.Definition.SourceFileLineNumber
                );
            }

            // Record all test results to write them to the summary later
            _testResults.Add(testResult);
        }
    }

    public void HandleTestRunEnd(TestRunStatistics statistics)
    {
        using (_lock.EnterScope())
        {
            _stopwatch.Stop();

            // Don't render empty summary for projects with no tests
            if (
                !Options.SummaryIncludeNotFoundTests
                && statistics.OverallOutcome == TestOutcome.None
            )
            {
                return;
            }

            var testResults = _testResults
                .Where(r =>
                    r.Outcome == TestOutcome.Failed
                    || r.Outcome == TestOutcome.Passed && Options.SummaryIncludePassedTests
                    || r.Outcome == TestOutcome.Skipped && Options.SummaryIncludeSkippedTests
                )
                .ToArray();

            var template = new TestSummaryTemplate
            {
                TestSuite = _testRunStartInfo?.SuiteName ?? "Unknown Test Suite",
                TargetFramework = _testRunStartInfo?.FrameworkName ?? "Unknown Target Framework",
                TestRunStatistics = statistics,
                TestResults = testResults,
            };

            github.CreateSummary(template.Render());
        }
    }
}
