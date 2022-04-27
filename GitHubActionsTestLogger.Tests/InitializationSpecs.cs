using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using GitHubActionsTestLogger.Tests.Fakes;
using GitHubActionsTestLogger.Tests.Utils;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class InitializationSpecs
{
    [Fact]
    public void Logger_can_be_used_with_default_configuration()
    {
        // Arrange
        using var logger = new TestLogger();
        var events = new FakeTestLoggerEvents();

        // Act
        var context = logger.InitializeAndGetContext(events, Directory.GetCurrentDirectory());

        // Assert
        context.Options.Should().Be(TestLoggerOptions.Default);
    }

    [Fact]
    public void Logger_can_be_used_with_custom_configuration()
    {
        // Arrange
        using var logger = new TestLogger();

        var events = new FakeTestLoggerEvents();
        var parameters = new Dictionary<string, string>
        {
            ["annotations.titleFormat"] = "TitleFormat",
            ["annotations.messageFormat"] = "MessageFormat"
        };

        // Act
        var context = logger.InitializeAndGetContext(events, parameters);

        // Assert
        context.Options.AnnotationTitleFormat.Should().Be("TitleFormat");
        context.Options.AnnotationMessageFormat.Should().Be("MessageFormat");
    }
}