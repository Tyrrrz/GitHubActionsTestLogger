using Microsoft.Testing.Platform.Extensions.Messages;

namespace GitHubActionsTestLogger;

internal enum TestOutcome
{
    None,
    Passed,
    Failed,
    Skipped,
}

internal static class TestOutcomeExtensions
{
    public static TestOutcome ToTestOutcome(this TestNodeStateProperty stateProperty) =>
        stateProperty switch
        {
            PassedTestNodeStateProperty => TestOutcome.Passed,
            FailedTestNodeStateProperty => TestOutcome.Failed,
            ErrorTestNodeStateProperty => TestOutcome.Failed,
            TimeoutTestNodeStateProperty => TestOutcome.Failed,
            SkippedTestNodeStateProperty => TestOutcome.Skipped,
            CancelledTestNodeStateProperty => TestOutcome.Skipped,
            _ => TestOutcome.None,
        };
}
