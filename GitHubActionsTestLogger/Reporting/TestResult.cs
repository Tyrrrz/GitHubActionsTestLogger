namespace GitHubActionsTestLogger.Reporting;

internal record TestResult(
    TestDefinition Definition,
    TestOutcome Outcome,
    string? ErrorMessage,
    string? ErrorStackTrace
);
