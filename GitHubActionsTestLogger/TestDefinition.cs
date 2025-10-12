using System.Collections.Generic;

namespace GitHubActionsTestLogger;

internal record TestDefinition(
    string Id,
    string DisplayName,
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

    // TODO
    public string? SourceFilePath => null;

    // TODO
    public int? SourceFileLineNumber => null;
}
