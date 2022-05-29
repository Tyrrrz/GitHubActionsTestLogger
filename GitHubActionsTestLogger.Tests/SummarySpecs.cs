using System.IO;
using FluentAssertions;
using GitHubActionsTestLogger.Tests.Utils;
using GitHubActionsTestLogger.Tests.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class SummarySpecs
{
    [Fact]
    public void Test_summary_contains_test_suite_name()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var context = new TestLoggerContext(
            new GitHubWorkflow(
                TextWriter.Null,
                summaryWriter
            ),
            TestLoggerOptions.Default
        );

        // Act
        context.SimulateTestRun("TestProject.dll");

        // Assert
        var output = summaryWriter.ToString().Trim();

        output.Should().Contain("TestProject");
    }

    [Fact]
    public void Test_summary_contains_names_of_failed_tests()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var context = new TestLoggerContext(
            new GitHubWorkflow(
                TextWriter.Null,
                summaryWriter
            ),
            TestLoggerOptions.Default
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetFullyQualifiedName("TestProject.SomeTests.Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage1")
                .Build(),
            new TestResultBuilder()
                .SetDisplayName("Test2")
                .SetFullyQualifiedName("TestProject.SomeTests.Test2")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage2")
                .Build(),
            new TestResultBuilder()
                .SetDisplayName("Test3")
                .SetFullyQualifiedName("TestProject.SomeTests.Test3")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage3")
                .Build()
        );

        // Assert
        var output = summaryWriter.ToString().Trim();

        output.Should().Contain("Test1");
        output.Should().Contain("Test2");
        output.Should().Contain("Test3");
    }

    [Fact]
    public void Test_summary_contains_error_messages_of_failed_tests()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var context = new TestLoggerContext(
            new GitHubWorkflow(
                TextWriter.Null,
                summaryWriter
            ),
            TestLoggerOptions.Default
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetFullyQualifiedName("TestProject.SomeTests.Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage1")
                .Build(),
            new TestResultBuilder()
                .SetDisplayName("Test2")
                .SetFullyQualifiedName("TestProject.SomeTests.Test2")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage2")
                .Build(),
            new TestResultBuilder()
                .SetDisplayName("Test3")
                .SetFullyQualifiedName("TestProject.SomeTests.Test3")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage3")
                .Build()
        );

        // Assert
        var output = summaryWriter.ToString().Trim();

        output.Should().Contain("ErrorMessage1");
        output.Should().Contain("ErrorMessage2");
        output.Should().Contain("ErrorMessage3");
    }
}