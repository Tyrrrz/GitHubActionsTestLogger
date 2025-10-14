using System.Collections.Generic;

namespace GitHubActionsTestLogger.Reporting;

internal record TestDefinition(
    string Id,
    string DisplayName,
    SymbolReference Symbol,
    SymbolReference TypeSymbol,
    string? SourceFilePath,
    int? SourceFileLineNumber,
    IReadOnlyDictionary<string, string> Properties
);
