using System.Collections.Generic;

namespace GitHubActionsTestLogger;

public record LoggerTestRunCriteria(string? TargetFramework, IEnumerable<string>? Sources);
