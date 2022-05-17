using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class AnnotationFormatSpecs
{
    [Fact]
    public void Custom_format_can_be_used_for_annotation_title()
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
                AnnotationTitleFormat = "<$test>"
            }
        );

        var testResult = new TestResult(new TestCase
        {
            DisplayName = "Test1"
        })
        {
            Outcome = TestOutcome.Failed,
            ErrorMessage = "ErrorMessage",
            ErrorStackTrace = "ErrorStackTrace"
        };

        // Act
        context.HandleTestResult(testResult);

        // Assert
        var output = commandWriter.ToString().Trim();
        output.Should().Contain("<Test1>");
    }

    [Fact]
    public void Custom_format_can_be_used_for_annotation_message()
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
                AnnotationMessageFormat = "$error\\n$trace"
            }
        );

        var testResult = new TestResult(new TestCase
        {
            DisplayName = "Test1"
        })
        {
            Outcome = TestOutcome.Failed,
            ErrorMessage = "ErrorMessage",
            ErrorStackTrace = "ErrorStackTrace"
        };

        // Act
        context.HandleTestResult(testResult);

        // Assert
        var output = commandWriter.ToString().Trim();
        output.Should().Contain("ErrorMessage%0D%0AErrorStackTrace");
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
                AnnotationTitleFormat = "[$traits.Category] $test"
            }
        );

        var testResult = new TestResult(new TestCase
        {
            DisplayName = "Test1",
            Traits =
            {
                {"Category", "UI Test"},
                {"Document", "SS01"}
            }
        })
        {
            Outcome = TestOutcome.Failed,
            ErrorMessage = "ErrorMessage"
        };

        // Act
        context.HandleTestResult(testResult);

        // Assert
        var output = commandWriter.ToString().Trim();
        output.Should().Contain("[UI Test] Test1");
    }
}