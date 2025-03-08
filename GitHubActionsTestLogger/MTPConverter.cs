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
        TestNodeStateProperty nodeState =
            testNodeUpdateMessage.TestNode.Properties.Single<TestNodeStateProperty>();
        var testOutcome = LoggerTestOutcome.None;
        switch (nodeState)
        {
            case PassedTestNodeStateProperty:
                testOutcome = LoggerTestOutcome.Passed;
                break;
            case FailedTestNodeStateProperty
            or ErrorTestNodeStateProperty
            or TimeoutTestNodeStateProperty
            or CancelledTestNodeStateProperty:
                testOutcome = LoggerTestOutcome.Failed;
                break;
            case SkippedTestNodeStateProperty:
                testOutcome = LoggerTestOutcome.Skipped;
                break;
        }

        var location = testNodeUpdateMessage.Properties.SingleOrDefault<FileLocationProperty>();
        var traits = testNodeUpdateMessage
            .TestNode.Properties.OfType<TestMetadataProperty>()
            .ToDictionary(i => i.Key, i => i.Value);
        var error =
            testNodeUpdateMessage.TestNode.Properties.SingleOrDefault<ErrorTestNodeStateProperty>();

        return new LoggerTestResult(
            testNodeUpdateMessage.TestNode.DisplayName,
            testNodeUpdateMessage.TestNode.Uid, // to minimally qualified name
            testNodeUpdateMessage.TestNode.Uid,
            traits,
            location?.FilePath,
            location?.LineSpan.Start.Line ?? 0,
            testOutcome,
            error?.Exception?.Message,
            error?.Exception?.StackTrace
        );
    }
}
