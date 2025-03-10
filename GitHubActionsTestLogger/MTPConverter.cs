using System;
using System.Linq;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace GitHubActionsTestLogger;

public static class MTPConverter
{
    public static LoggerTestResult ConvertTestNodeUpdateMessage(
        TestNodeUpdateMessage testNodeUpdateMessage
    )
    {
        var testNode = testNodeUpdateMessage.TestNode;
        var properties = testNode.Properties;
        TestNodeStateProperty nodeState = properties.Single<TestNodeStateProperty>();
        var testOutcome = LoggerTestOutcome.None;
        string? errorMessage = null;
        string? errorStackTrace = null;
        switch (nodeState)
        {
            case PassedTestNodeStateProperty:
                testOutcome = LoggerTestOutcome.Passed;
                break;

            case FailedTestNodeStateProperty failed:
                testOutcome = LoggerTestOutcome.Failed;
                errorMessage = failed.Explanation;
                errorStackTrace = failed.Exception?.StackTrace;
                break;

            case ErrorTestNodeStateProperty failed:
                testOutcome = LoggerTestOutcome.Failed;
                errorMessage = failed.Explanation;
                errorStackTrace = failed.Exception?.StackTrace;
                break;

            case TimeoutTestNodeStateProperty failed:
                testOutcome = LoggerTestOutcome.Failed;
                errorMessage = failed.Explanation;
                errorStackTrace = failed.Exception?.StackTrace;
                break;

            case CancelledTestNodeStateProperty failed:
                testOutcome = LoggerTestOutcome.Failed;
                errorMessage = failed.Explanation;
                errorStackTrace = failed.Exception?.StackTrace;
                break;

            case SkippedTestNodeStateProperty:
                testOutcome = LoggerTestOutcome.Skipped;
                break;
        }

        var location = properties.SingleOrDefault<FileLocationProperty>();
        var traits = properties
            .OfType<TestMetadataProperty>()
            .ToDictionary(i => i.Key, i => i.Value);
        var error = properties.SingleOrDefault<ErrorTestNodeStateProperty>();

        return new LoggerTestResult(
            testNode.DisplayName,
            testNode.Uid, // to minimally qualified name
            testNode.Uid,
            traits,
            location?.FilePath,
            location?.LineSpan.Start.Line ?? 0,
            testOutcome,
            errorMessage,
            errorStackTrace
        );
    }
}
