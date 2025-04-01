using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using GitHubActionsTestLogger.Utils.Extensions;

using Microsoft.Testing.Platform.Extensions.Messages;

namespace GitHubActionsTestLogger;

public class TestReporterContext(GitHubWorkflow github, TestReporterOptions options)
{
    private readonly Lock _lock = new();
    private readonly List<TestNode> _testResults = [];
    private readonly Stopwatch _stopwatch = new();

    public TestReporterOptions Options { get; } = options;

    private string FormatAnnotation(string format, TestNode testNode, TestNodeStateProperty testNodeStateProperty)
    {
        var buffer = new StringBuilder(format);

        // Escaped new line token (backwards compat)
        buffer.Replace("\\n", "\n");

        // Name token
        buffer
            .Replace("@test", testNode.DisplayName)
            // Backwards compat
            .Replace("$test", testNode.DisplayName);

        // Trait tokens
        // TODO: this is not enough, we would also need to handle properties from VSTest bridge
        foreach (var metadataProperty in testNode.Properties.OfType<TestMetadataProperty>())
        {
            buffer
                .Replace($"@traits.{metadataProperty.Key}", metadataProperty.Value)
                // Backwards compat
                .Replace($"$traits.{metadataProperty.Key}", metadataProperty.Value);
        }

        Exception? exception = testNodeStateProperty switch
        {
            FailedTestNodeStateProperty failed => failed.Exception,
            ErrorTestNodeStateProperty error => error.Exception,
            _ => null,
        };

        // Error message
        buffer
            .Replace("@error", exception?.Message ?? "")
            // Backwards compat
            .Replace("$error", exception?.Message ?? "");

        // Error trace
        buffer
            .Replace("@trace", exception?.StackTrace ?? "")
            // Backwards compat
            .Replace("$trace", exception?.StackTrace ?? "");

        // Target framework
        // TODO: Copy logic from platform: https://github.com/microsoft/testfx/blob/main/src/Platform/Microsoft.Testing.Platform/OutputDevice/BrowserOutputDevice.cs#L78
        // or ask for platform to expose it
        buffer
            .Replace("@framework", _testRunCriteria?.TryGetTargetFramework() ?? "")
            // Backwards compat
            .Replace("$framework", _testRunCriteria?.TryGetTargetFramework() ?? "");

        return buffer.Trim().ToString();
    }

    private string FormatAnnotationTitle(TestNode testNode, TestNodeStateProperty testNodeStateProperty) =>
        FormatAnnotation(Options.AnnotationTitleFormat, testNode, testNodeStateProperty);

    private string FormatAnnotationMessage(TestNode testNode, TestNodeStateProperty testNodeStateProperty) =>
        FormatAnnotation(Options.AnnotationMessageFormat, testNode, testNodeStateProperty);

    public void HandleTestResult(TestNodeUpdateMessage testNodeUpdateMessage)
    {
        using (_lock.EnterScope())
        {
            var testNodeState = testNodeUpdateMessage.TestNode.Properties.Single<TestNodeStateProperty>();
            // Report failed test results to job annotations
            if (testNodeState is FailedTestNodeStateProperty or ErrorTestNodeStateProperty)
            {
                github.CreateErrorAnnotation(
                    FormatAnnotationTitle(testNodeUpdateMessage.TestNode, testNodeState),
                    FormatAnnotationMessage(testNodeUpdateMessage.TestNode, testNodeState),
                    args.Result.TryGetSourceFilePath(),
                    args.Result.TryGetSourceLine()
                );
            }

            // Record all test results to write them to the summary later
            _testResults.Add(testNodeUpdateMessage.TestNode);
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

            var testSuite = Assembly.GetEntryAssembly()?.GetName().Name
                ?? "Unknown Test Suite";

            var targetFramework =
                // See line 65
                _testRunCriteria?.TryGetTargetFramework() ?? "Unknown Target Framework";

            var testRunStatistics = new TestRunStatistics(
                (int?)args.TestRunStatistics?[TestOutcome.Passed]
                    ?? _testResults.Count(r => r.Outcome == TestOutcome.Passed),
                (int?)args.TestRunStatistics?[TestOutcome.Failed]
                    ?? _testResults.Count(r => r.Outcome == TestOutcome.Failed),
                (int?)args.TestRunStatistics?[TestOutcome.Skipped]
                    ?? _testResults.Count(r => r.Outcome == TestOutcome.Skipped),
                (int?)args.TestRunStatistics?.ExecutedTests ?? _testResults.Count,
                _stopwatch.Elapsed
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
