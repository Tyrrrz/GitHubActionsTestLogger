using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GitHubActionsTestLogger.GitHub;
using GitHubActionsTestLogger.Utils.Extensions;

namespace GitHubActionsTestLogger.Reporting;

internal class TestReportingContext(GitHubWorkflow github, TestReportingOptions options)
{
    private readonly SemaphoreSlim _lock = new(1, 1);
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

    public async Task HandleTestRunStartAsync(
        TestRunStartInfo info,
        CancellationToken cancellationToken = default
    )
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            _stopwatch.Start();
            _testRunStartInfo = info;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task HandleTestResultAsync(
        TestResult testResult,
        CancellationToken cancellationToken = default
    )
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            // Report failed test results to job annotations
            if (testResult.Outcome == TestOutcome.Failed)
            {
                await github.CreateErrorAnnotationAsync(
                    FormatAnnotationTitle(testResult),
                    FormatAnnotationMessage(testResult),
                    testResult.Definition.SourceFilePath,
                    testResult.Definition.SourceFileLineNumber
                );
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task HandleTestRunEndAsync(
        TestRunEndInfo info,
        CancellationToken cancellationToken = default
    )
    {
        await _lock.WaitAsync(cancellationToken);

        try
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
            await github.CreateSummaryAsync(await template.RenderAsync(cancellationToken));
        }
        finally
        {
            _lock.Release();
        }
    }
}
