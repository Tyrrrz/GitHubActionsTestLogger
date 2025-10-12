using System;

namespace GitHubActionsTestLogger;

internal record TestResult(
    TestDefinition Definition,
    TestOutcome Outcome,
    Exception? Exception,
    string? FailExplanation
);
