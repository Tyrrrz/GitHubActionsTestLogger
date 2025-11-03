using System.Threading.Tasks;
using Microsoft.Testing.Platform.Builder;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class MtpInitializationSpecs
{
    [Fact]
    public async Task I_can_use_the_logger_with_the_default_configuration()
    {
        // Arrange
        var builder = await TestApplication.CreateBuilderAsync([]);

        // Act & assert
        builder.AddGitHubActionsReporting();

        // Can't perform a more meaningful assertion here without
        // accessing internal members of the reporter.
    }
}
