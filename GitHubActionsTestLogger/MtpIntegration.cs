using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Services;

namespace GitHubActionsTestLogger;

public static class MtpIntegration
{
    /// <summary>
    /// Adds GitHub Actions reporting support to the Testing Platform Builder.
    /// </summary>
    public static void AddGitHubActionsReporting(
        this ITestApplicationBuilder testApplicationBuilder
    )
    {
        var compositeExtension = new CompositeExtensionFactory<MtpLogger>(
            serviceProvider => new MtpLogger(serviceProvider.GetCommandLineOptions())
        );

        testApplicationBuilder.TestHost.AddDataConsumer(compositeExtension);
        testApplicationBuilder.TestHost.AddTestSessionLifetimeHandle(compositeExtension);

        testApplicationBuilder.CommandLine.AddProvider(() => new MtpLoggerOptionsProvider());
    }
}
