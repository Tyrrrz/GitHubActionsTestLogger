using Microsoft.Testing.Platform.Builder;

namespace GitHubActionsTestLogger;

/// <summary>
/// This class is used by Microsoft.Testing.Platform.MSBuild to hook into the Testing Platform Builder to add GitHubActionsLogger support.
/// </summary>
public static class TestingPlatformBuilderHook
{
    /// <summary>
    /// Adds TrxReport support to the Testing Platform Builder.
    /// </summary>
    /// <param name="testApplicationBuilder">The test application builder.</param>
    /// <param name="_">The command line arguments.</param>
    public static void AddExtensions(ITestApplicationBuilder testApplicationBuilder, string[] _) =>
        testApplicationBuilder.AddGitHubActionsReporter();
}
