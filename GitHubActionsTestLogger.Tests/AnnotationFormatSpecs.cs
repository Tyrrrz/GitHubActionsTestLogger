using System.IO;
using FluentAssertions;
using GitHubActionsTestLogger.Tests.Utils;
using GitHubActionsTestLogger.Tests.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class AnnotationFormatSpecs
{
    [Fact]
    public void Custom_format_can_reference_test_name()
    {
        // Arrange
        using var commandWriter = new StringWriter();

        var context = new TestLoggerContext(
            new GitHubWorkflow(
                commandWriter,
                TextWriter.Null
            ),
            new TestLoggerOptions
            {
                AnnotationTitleFormat = "<$test>",
                AnnotationMessageFormat = "[$test]"
            }
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<Test1>");
        output.Should().Contain("[Test1]");
    }

    [Fact]
    public void Custom_format_can_reference_test_traits()
    {
        // Arrange
        using var commandWriter = new StringWriter();

        var context = new TestLoggerContext(
            new GitHubWorkflow(
                commandWriter,
                TextWriter.Null
            ),
            new TestLoggerOptions
            {
                AnnotationTitleFormat = "<$traits.Category -> $test>",
                AnnotationMessageFormat = "[$traits.Category -> $test]"
            }
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetTrait("Category", "UI Test")
                .SetTrait("Document", "SS01")
                .SetOutcome(TestOutcome.Failed)
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<UI Test -> Test1>");
        output.Should().Contain("[UI Test -> Test1]");
    }

    [Fact]
    public void Custom_format_can_reference_test_error_message()
    {
        // Arrange
        using var commandWriter = new StringWriter();

        var context = new TestLoggerContext(
            new GitHubWorkflow(
                commandWriter,
                TextWriter.Null
            ),
            new TestLoggerOptions
            {
                AnnotationTitleFormat = "<$test: $error>",
                AnnotationMessageFormat = "[$test: $error]"
            }
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage")
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<Test1: ErrorMessage>");
        output.Should().Contain("[Test1: ErrorMessage]");
    }

    [Fact]
    public void Custom_format_can_reference_test_error_stacktrace()
    {
        // Arrange
        using var commandWriter = new StringWriter();

        var context = new TestLoggerContext(
            new GitHubWorkflow(
                commandWriter,
                TextWriter.Null
            ),
            new TestLoggerOptions
            {
                AnnotationTitleFormat = "<$test: $trace>",
                AnnotationMessageFormat = "[$test: $trace]"
            }
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorStackTrace("ErrorStackTrace")
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<Test1: ErrorStackTrace>");
        output.Should().Contain("[Test1: ErrorStackTrace]");
    }

    [Fact]
    public void Custom_format_can_contain_newlines()
    {
        // Arrange
        using var commandWriter = new StringWriter();

        var context = new TestLoggerContext(
            new GitHubWorkflow(
                commandWriter,
                TextWriter.Null
            ),
            new TestLoggerOptions
            {
                AnnotationMessageFormat = "foo\\nbar"
            }
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();
        output.Should().Contain("foo%0Abar");
    }
}