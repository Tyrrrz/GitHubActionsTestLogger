using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using GitHubActionsTestLogger.Utils.Extensions;

namespace GitHubActionsTestLogger;

internal class TestReporterContext(GitHubWorkflow github, TestReporterOptions options)
{
    private readonly Lock _lock = new();
    private readonly Stopwatch _stopwatch = new();

    private readonly string? _testSuiteName = Assembly.GetEntryAssembly()?.GetName().Name;

    private readonly string? _targetFrameworkName = Assembly
        .GetEntryAssembly()
        ?.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()
        ?.FrameworkName;

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
        {
            buffer.Replace($"@traits.{property.Key}", property.Value);
        }

        // Error message
        buffer.Replace("@error", testResult.Exception?.Message ?? "");

        // Error trace
        buffer.Replace("@trace", testResult.Exception?.StackTrace ?? "");

        // Target framework
        // TODO: Copy logic from platform: https://github.com/microsoft/testfx/blob/main/src/Platform/Microsoft.Testing.Platform/OutputDevice/BrowserOutputDevice.cs#L78
        // or ask for platform to expose it
        buffer.Replace("@framework", _targetFrameworkName ?? "");

        return buffer.Trim().ToString();
    }

    private string FormatAnnotationTitle(TestResult testResult) =>
        FormatAnnotation(Options.AnnotationTitleFormat, testResult);

    private string FormatAnnotationMessage(TestResult testResult) =>
        FormatAnnotation(Options.AnnotationMessageFormat, testResult);

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
                    args.Result.TryGetSourceFilePath(),
                    args.Result.TryGetSourceLine()
                );
            }

            // Record all test results to write them to the summary later
            _testResults.Add(testResult);
        }
    }

    public void HandleTestRunStart()
    {
        using (_lock.EnterScope())
        {
            _stopwatch.Start();
        }
    }

    public void HandleTestRunComplete()
    {
        using (_lock.EnterScope())
        {
            _stopwatch.Stop();

            var testRunStatistics = new TestRunStatistics(
                _testResults.Count(r => r.Outcome == TestOutcome.Passed),
                _testResults.Count(r => r.Outcome == TestOutcome.Failed),
                _testResults.Count(r => r.Outcome == TestOutcome.Skipped),
                _testResults.Count,
                _stopwatch.Elapsed
            );

            // Don't render empty summary for projects with no tests
            if (
                !Options.SummaryIncludeNotFoundTests
                && testRunStatistics.OverallOutcome == TestOutcome.None
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
                TestSuite = _testSuiteName ?? "Unknown Test Suite",
                TargetFramework = _targetFrameworkName ?? "Unknown Target Framework",
                TestRunStatistics = testRunStatistics,
                TestResults = testResults,
            };

            github.CreateSummary(template.Render());
        }
    }
}
