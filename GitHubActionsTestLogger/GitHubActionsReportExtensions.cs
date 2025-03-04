using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Extensions;

namespace GitHubActionsTestLogger;

/// <summary>
/// Provides extension methods for adding GitHubActions report generation to a test application.
/// </summary>
public static class GitHubActionsReportExtensions
{
    /// <summary>
    /// Adds GitHubActions report generation to a test application.
    /// </summary>
    /// <param name="builder">The test application builder.</param>
    public static void AddGitHubActionsReporter(this ITestApplicationBuilder builder)
    {
        CompositeExtensionFactory<GitHubActionsReporterDataConsumer> compositeExtensionFactory =
            new(serviceProvider => new GitHubActionsReporterDataConsumer(serviceProvider));
        builder.TestHost.AddDataConsumer(compositeExtensionFactory);
    }
}
