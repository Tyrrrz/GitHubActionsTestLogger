using System.Collections.Generic;

namespace GitHubActionsTestLogger;

internal record TestDefinition(
    string Id,
    string DisplayName,
    IReadOnlyDictionary<string, string> Properties
);
