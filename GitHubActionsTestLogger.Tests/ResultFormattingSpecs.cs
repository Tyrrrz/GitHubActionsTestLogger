using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class ResultFormattingSpecs
{
    [Fact]
    public void Custom_format_can_be_used_when_writing_test_results()
    {
        // Arrange
        using var writer = new StringWriter();
        var context = new TestLoggerContext(
            writer,
            new TestLoggerOptions(
                new TestResultMessageFormat("<$test> -> $outcome"),
                TestLoggerOptions.Default.ReportWarnings
            )
        );

        var testResult = new TestResult(new TestCase
        {
            DisplayName = "Test1"
        })
        {
            Outcome = TestOutcome.Failed,
            ErrorMessage = "ErrorMessage"
        };

        // Act
        context.ProcessTestResult(testResult);

        var output = writer.ToString().Trim();

        // Assert
        output.Should().Contain(
            "<Test1> -> ErrorMessage"
        );
    }

    [Fact]
    public void Custom_format_can_reference_test_traits()
    {
        // Arrange
        using var writer = new StringWriter();
        var context = new TestLoggerContext(
            writer,
            new TestLoggerOptions(
                new TestResultMessageFormat("[$traits.Category] <$test> -> $outcome"),
                TestLoggerOptions.Default.ReportWarnings
            )
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
        context.ProcessTestResult(testResult);

        var output = writer.ToString().Trim();

        // Assert
        output.Should().Contain(
            "[UI Test] <Test1> -> ErrorMessage"
        );
    }
}