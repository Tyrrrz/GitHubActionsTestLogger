using System.Reflection;
using System.Threading.Tasks;
using GitHubActionsTestLogger.GitHub;
using Microsoft.Testing.Platform.Extensions;

namespace GitHubActionsTestLogger;

internal class MtpLoggerExtension : IExtension
{
    private static readonly Assembly Assembly = typeof(MtpLoggerExtension).Assembly;

    public string Uid => "GitHubActionsTestLoggerExtension";

    public string Version { get; } =
        Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? Assembly.GetName().Version?.ToString()
        // This should never happen, but throwing here seems like an overreaction
        ?? "1.0.0";

    public string DisplayName => "GitHub Actions Test Logger";

    public string Description => "Reports test run information to GitHub Actions";

    public Task<bool> IsEnabledAsync() => Task.FromResult(GitHubEnvironment.IsRunningInActions);
}
