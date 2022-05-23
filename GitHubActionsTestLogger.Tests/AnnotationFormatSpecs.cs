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

        // Act
        context.SimulateTestRun(
            "FakeTests.dll",
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage")
                .SetErrorStackTrace("ErrorStackTrace")
                .Build()
        );

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

        // Act
        context.SimulateTestRun(
            "FakeTests.dll",
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetTrait("Category", "UI Test")
                .SetTrait("Document", "SS01")
                .SetOutcome(TestOutcome.Failed)
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();
        output.Should().Contain("[UI Test] Test1");
    }
}