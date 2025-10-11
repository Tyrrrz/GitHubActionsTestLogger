using System.Threading.Tasks;

using Microsoft.Testing.Platform.Extensions;

namespace GitHubActionsTestLogger;

internal sealed class GitHubTestReporterExtension : IExtension
{
    // A unique identifier for the extension across all registered extensions.
    public string Uid => nameof(GitHubTestReporterExtension);

    // TODO: Decide how to get version (e.g. hardcoded, from some generated file, from assembly version...)
    public string Version => "1.0.0";

    public string DisplayName => "GitHub test reporter";

    public string Description => "Reports test run information to GitHub Actions";

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);
}
