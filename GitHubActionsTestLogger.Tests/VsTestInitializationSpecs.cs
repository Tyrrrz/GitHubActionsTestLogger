using System.Collections.Generic;
using System.IO;
using GitHubActionsTestLogger.Tests.VsTest;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class VsTestInitializationSpecs
{
    [Fact]
    public void I_can_use_the_logger_with_the_default_configuration()
    {
        // Arrange
        var logger = new VsTestLogger();
        var events = new FakeTestLoggerEvents();

        // Act & assert
        logger.Initialize(events, Directory.GetCurrentDirectory());

        // Can't perform a more meaningful assertion here without
        // accessing internal members of the logger.
    }

    [Fact]
    public void I_can_use_the_logger_with_an_empty_configuration()
    {
        // Arrange
        var logger = new VsTestLogger();
        var events = new FakeTestLoggerEvents();

        // Act & assert
        logger.Initialize(events, new Dictionary<string, string?>());

        // Can't perform a more meaningful assertion here without
        // accessing internal members of the logger.
    }
}
