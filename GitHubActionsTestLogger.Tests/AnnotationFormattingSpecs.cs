using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class AnnotationFormattingSpecs
{
    [Fact]
    public void Custom_format_can_be_used_for_annotation_title()
    {
        // Arrange
        using var writer = new StringWriter();

        using var context = new TestLoggerContext(
            writer,
            null,
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
        context.HandleTestResult(new TestResultEventArgs(testResult));

        // Assert
        var output = writer.ToString().Trim();
        output.Should().Contain("<Test1>");
    }

    [Fact]
    public void Custom_format_can_be_used_for_annotation_message()
    {
        // Arrange
        using var writer = new StringWriter();

        using var context = new TestLoggerContext(
            writer,
            null,
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
        context.HandleTestResult(new TestResultEventArgs(testResult));

        // Assert
        var output = writer.ToString().Trim();
        output.Should().Contain("ErrorMessage\nErrorStackTrace");
    }

    [Fact]
    public void Custom_format_can_reference_test_traits()
    {
        // Arrange
        using var writer = new StringWriter();

        using var context = new TestLoggerContext(
            writer,
            null,
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
        context.HandleTestResult(new TestResultEventArgs(testResult));

        // Assert
        var output = writer.ToString().Trim();
        output.Should().Contain("[UI Test] Test1");
    }
}