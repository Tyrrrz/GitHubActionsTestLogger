using System.IO;
using GitHubActionsTestLogger.GitHub;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Services;

namespace GitHubActionsTestLogger;

/// <summary>
/// Extensions that integrate GitHub Actions test reporting into the Microsoft Testing Platform.
/// </summary>
public static class MtpIntegration
{
    /// <inheritdoc cref="MtpIntegration" />
    extension(ITestApplicationBuilder testApplicationBuilder)
    {
        /// <summary>
        /// Adds GitHub Actions reporting support to the Testing Platform Builder.
        /// </summary>
        /// <remarks>
        /// This overload is useful for testing purposes, as it allows providing custom
        /// writers for GitHub's command and summary outputs.
        /// </remarks>
        public void AddGitHubActionsReporting(TextWriter commandWriter, TextWriter summaryWriter)
        {
            var compositeExtension = new CompositeExtensionFactory<MtpLogger>(
                serviceProvider => new MtpLogger(
                    new GitHubWorkflow(commandWriter, summaryWriter),
                    serviceProvider.GetCommandLineOptions()
                )
            );

            testApplicationBuilder.TestHost.AddDataConsumer(compositeExtension);
            testApplicationBuilder.TestHost.AddTestSessionLifetimeHandle(compositeExtension);

            testApplicationBuilder.CommandLine.AddProvider(() => new MtpLoggerOptionsProvider());
        }

        /// <summary>
        /// Adds GitHub Actions reporting support to the Testing Platform Builder.
        /// </summary>
        public void AddGitHubActionsReporting() =>
            testApplicationBuilder.AddGitHubActionsReporting(
                GitHubWorkflow.DefaultCommandWriter,
                GitHubWorkflow.DefaultSummaryWriter
            );
    }

    /// <summary>
    /// Adds GitHub Actions reporting support to the Testing Platform Builder.
    /// </summary>
    // This method is called implicitly by MTP to automatically register this extension.
    // This method does not need to be an extension method, and it's better that it isn't to avoid confusion.
    public static void AddExtensions(ITestApplicationBuilder testApplicationBuilder, string[] _) =>
        testApplicationBuilder.AddGitHubActionsReporting();
}
