using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Services;

namespace GitHubActionsTestLogger;

public static class GitHubReportExtensions
{
    /// <summary>
    /// Adds GitHub Actions reporting support to the Testing Platform Builder.
    /// </summary>
    public static void AddGitHubReportProvider(this ITestApplicationBuilder testApplicationBuilder)
    {
        var extension = new TestReporterExtension();

        var compositeExtension = new CompositeExtensionFactory<TestReporter>(
            serviceProvider => new TestReporter(extension, serviceProvider.GetCommandLineOptions())
        );
        testApplicationBuilder.TestHost.AddDataConsumer(compositeExtension);
        testApplicationBuilder.TestHost.AddTestSessionLifetimeHandle(compositeExtension);

        testApplicationBuilder.CommandLine.AddProvider(() =>
            new TestReporterOptionsProvider(extension)
        );
    }
}
