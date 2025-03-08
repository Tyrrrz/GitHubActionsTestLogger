using System.Collections.Generic;

namespace GitHubActionsTestLogger;

public record LoggerTestResult(
    string? DisplayName,
    string MinimallyQualifiedName,
    string FullyQualifiedName,
    Dictionary<string, string> Traits,
    string? SourceFilePath,
    int? SourceFileLine,
    LoggerTestOutcome Outcome,
    string? ErrorMessage,
    string? ErrorStackTrace
);
