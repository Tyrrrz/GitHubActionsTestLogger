namespace GitHubActionsTestLogger.Templates;

internal class TestSummaryContext
{
    public required TestLoggerOptions Options { get; init; }

    public required string TestSuite { get; init; }

    public required string TargetFramework { get; init; }

    public required TestRunResult TestRunResult { get; init; }
}