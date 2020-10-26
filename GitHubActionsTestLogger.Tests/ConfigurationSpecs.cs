using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using GitHubActionsTestLogger.Tests.Fakes;
using Xunit;

namespace GitHubActionsTestLogger.Tests
{
    public class ConfigurationSpecs
    {
        [Fact]
        public void Logger_can_be_used_with_default_configuration()
        {
            // Arrange
            var logger = new TestLogger();
            var events = new FakeTestLoggerEvents();

            // Act
            logger.Initialize(events, Directory.GetCurrentDirectory());

            // Assert
            logger.Context.Should().NotBeNull();
            logger.Context!.Options.Should().BeEquivalentTo(TestLoggerOptions.Default);
        }

        [Fact]
        public void Logger_can_be_used_with_custom_configuration()
        {
            // Arrange
            var logger = new TestLogger();
            var events = new FakeTestLoggerEvents();

            var parameters = new Dictionary<string, string>
            {
                ["report-warnings"] = "false"
            };

            // Act
            logger.Initialize(events, parameters);

            // Assert
            logger.Context.Should().NotBeNull();
            logger.Context!.Options.Should().BeEquivalentTo(new TestLoggerOptions(false));
        }
    }
}