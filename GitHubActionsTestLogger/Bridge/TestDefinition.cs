using System.Collections.Generic;

namespace GitHubActionsTestLogger.Bridge;

internal record TestDefinition(
    string Id,
    string DisplayName,
    string? SourceFilePath,
    int? SourceFileLineNumber,
    IReadOnlyDictionary<string, string> Properties
)
{
    // TODO
    public string TypeFullyQualifiedName => Id;

    // TODO
    public string TypeMinimallyQualifiedName => DisplayName;

    // TODO
    public string FullyQualifiedName => DisplayName;

    // TODO
    public string MinimallyQualifiedName => DisplayName;
}
