using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using GitHubActionsTestLogger.GitHub;
using GitHubActionsTestLogger.Utils.Extensions;

namespace GitHubActionsTestLogger.Reporting;

internal class TestReportingContext(GitHubWorkflow github, TestReportingOptions options)
{
    private readonly Lock _lock = new();
    private readonly Stopwatch _stopwatch = new();

    private TestRunStartInfo? _testRunStartInfo;

    public TestReportingOptions Options { get; } = options;

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

    public void HandleTestRunStart(TestRunStartInfo info)
    {
        using (_lock.EnterScope())
        {
            _stopwatch.Start();
            _testRunStartInfo = info;
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
        }
    }

    public void HandleTestRunEnd(TestRunEndInfo info)
    {
        using (_lock.EnterScope())
        {
            _stopwatch.Stop();

            // Don't render empty summary for projects with no tests
            if (!Options.SummaryAllowEmpty && info.OverallOutcome == TestOutcome.None)
                return;

            var filteredTestResults = info
                .TestResults.Where(r =>
                    r.Outcome == TestOutcome.Failed
                    || r.Outcome == TestOutcome.Passed && Options.SummaryIncludePassedTests
                    || r.Outcome == TestOutcome.Skipped && Options.SummaryIncludeSkippedTests
                )
                .ToArray();

            var template = new TestSummaryTemplate
            {
                TestSuite = info.StartInfo.SuiteName ?? "Unknown Test Suite",
                TargetFramework = info.StartInfo.FrameworkName ?? "Unknown Target Framework",
                TestRunEndInfo = info,
                TestResults = filteredTestResults,
            };

            // Report the test run to the job summary
            github.CreateSummary(template.Render());
        }
    }
}
