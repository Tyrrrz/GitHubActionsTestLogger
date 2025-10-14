using Microsoft.Testing.Platform.Builder;

namespace GitHubActionsTestLogger;

/// <summary>
/// This class is used by Microsoft.Testing.Platform.MSBuild to hook into the Testing Platform Builder
/// to add GitHub Actions reporting support.
/// </summary>
public static class TestingPlatformBuilderHook
{
    /// <summary>
    /// Adds GitHub Actions reporting support to the Testing Platform Builder.
    /// </summary>
    public static void AddExtensions(ITestApplicationBuilder testApplicationBuilder, string[] _) =>
        testApplicationBuilder.AddGitHubActionsReporting();
}
