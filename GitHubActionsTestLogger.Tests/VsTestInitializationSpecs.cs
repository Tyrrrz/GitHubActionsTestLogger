using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using GitHubActionsTestLogger.Reporting;
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

        // Act
        logger.Initialize(events, Directory.GetCurrentDirectory());

        // Assert
        logger.Context.Should().NotBeNull();
        logger.Context?.Options.Should().BeEquivalentTo(TestReportingOptions.Default);
    }

    [Fact]
    public void I_can_use_the_logger_with_an_empty_configuration()
    {
        // Arrange
        var logger = new VsTestLogger();
        var events = new FakeTestLoggerEvents();

        // Act
        logger.Initialize(events, new Dictionary<string, string?>());

        // Assert
        logger.Context.Should().NotBeNull();
        logger.Context?.Options.Should().BeEquivalentTo(TestReportingOptions.Default);
    }

    [Fact]
    public void I_can_use_the_logger_with_a_custom_configuration()
    {
        // Arrange
        var logger = new VsTestLogger();

        var events = new FakeTestLoggerEvents();
        var parameters = new Dictionary<string, string?>
        {
            ["annotations-title"] = "TitleFormat",
            ["annotations-message"] = "MessageFormat",
            ["summary-allow-empty"] = "true",
            ["summary-include-passed"] = "true",
            ["summary-include-skipped"] = "true",
        };

        // Act
        logger.Initialize(events, parameters);

        // Assert
        logger.Context.Should().NotBeNull();
        logger.Context?.Options.AnnotationTitleFormat.Should().Be("TitleFormat");
        logger.Context?.Options.AnnotationMessageFormat.Should().Be("MessageFormat");
        logger.Context?.Options.SummaryAllowEmpty.Should().BeTrue();
        logger.Context?.Options.SummaryIncludePassedTests.Should().BeTrue();
        logger.Context?.Options.SummaryIncludeSkippedTests.Should().BeTrue();
    }
}
