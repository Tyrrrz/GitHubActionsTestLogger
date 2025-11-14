using System.IO;
using System.Threading.Tasks;
using GitHubActionsTestLogger.Tests.Mtp;
using Microsoft.Testing.Platform.Builder;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class MtpInitializationSpecs
{
    [Fact]
    public async Task I_can_use_the_logger_with_the_default_configuration()
    {
        // Arrange
        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            Path.Combine(Directory.GetCurrentDirectory(), "FakeTestResults"),
            "--report-github",
        ]);

        builder.RegisterFakeTests();
        builder.AddGitHubActionsReporting();

        // Act & assert
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Can't perform a more meaningful assertion here without
        // accessing internal members of the reporter.
    }
}
