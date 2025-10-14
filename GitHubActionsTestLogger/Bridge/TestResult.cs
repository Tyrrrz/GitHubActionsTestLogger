namespace GitHubActionsTestLogger.Bridge;

internal record TestResult(
    TestDefinition Definition,
    TestOutcome Outcome,
    string? ErrorMessage,
    string? ErrorStackTrace
);
