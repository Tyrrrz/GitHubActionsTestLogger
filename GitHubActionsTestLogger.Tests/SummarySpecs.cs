using System.IO;
using FluentAssertions;
using GitHubActionsTestLogger.Tests.Utils;
using GitHubActionsTestLogger.Tests.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;
using Xunit.Abstractions;

namespace GitHubActionsTestLogger.Tests;

public class SummarySpecs(ITestOutputHelper testOutput)
{
    [Fact]
    public void I_can_use_the_logger_to_produce_a_summary_that_includes_the_test_suite_name()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var context = new TestReporterContext(
            new GitHubWorkflow(TextWriter.Null, summaryWriter),
            TestReporterOptions.Default
        );

        // Act
        context.SimulateTestRun("TestProject.dll");

        // Assert
        var output = summaryWriter.ToString().Trim();

        output.Should().Contain("TestProject");

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_a_summary_that_includes_the_list_of_passed_tests()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var context = new TestReporterContext(
            new GitHubWorkflow(TextWriter.Null, summaryWriter),
            new TestReporterOptions { SummaryIncludePassedTests = true }
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetFullyQualifiedName("TestProject.SomeTests.Test1")
                .SetOutcome(TestOutcome.Passed)
                .Build(),
            new TestResultBuilder()
                .SetDisplayName("Test2")
                .SetFullyQualifiedName("TestProject.SomeTests.Test2")
                .SetOutcome(TestOutcome.Passed)
                .Build(),
            new TestResultBuilder()
                .SetDisplayName("Test3")
                .SetFullyQualifiedName("TestProject.SomeTests.Test3")
                .SetOutcome(TestOutcome.Passed)
                .Build(),
            new TestResultBuilder()
                .SetDisplayName("Test4")
                .SetFullyQualifiedName("TestProject.SomeTests.Test4")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage4")
                .Build()
        );

        // Assert
        var output = summaryWriter.ToString().Trim();

        output.Should().Contain("Test1");
        output.Should().Contain("Test2");
        output.Should().Contain("Test3");
        output.Should().Contain("Test4");

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_a_summary_that_includes_the_list_of_failed_tests()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var context = new TestReporterContext(
            new GitHubWorkflow(TextWriter.Null, summaryWriter),
            TestReporterOptions.Default
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
                .Build(),
            new TestResultBuilder()
                .SetDisplayName("Test4")
                .SetFullyQualifiedName("TestProject.SomeTests.Test4")
                .SetOutcome(TestOutcome.Passed)
                .Build(),
            new TestResultBuilder()
                .SetDisplayName("Test5")
                .SetFullyQualifiedName("TestProject.SomeTests.Test5")
                .SetOutcome(TestOutcome.Skipped)
                .Build()
        );

        // Assert
        var output = summaryWriter.ToString().Trim();

        output.Should().Contain("Test1");
        output.Should().Contain("ErrorMessage1");
        output.Should().Contain("Test2");
        output.Should().Contain("ErrorMessage2");
        output.Should().Contain("Test3");
        output.Should().Contain("ErrorMessage3");

        output.Should().NotContain("Test4");
        output.Should().NotContain("Test5");

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_a_summary_that_includes_the_list_of_skipped_tests()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var context = new TestReporterContext(
            new GitHubWorkflow(TextWriter.Null, summaryWriter),
            new TestReporterOptions { SummaryIncludeSkippedTests = true }
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetFullyQualifiedName("TestProject.SomeTests.Test1")
                .SetOutcome(TestOutcome.Skipped)
                .Build(),
            new TestResultBuilder()
                .SetDisplayName("Test2")
                .SetFullyQualifiedName("TestProject.SomeTests.Test2")
                .SetOutcome(TestOutcome.Skipped)
                .Build(),
            new TestResultBuilder()
                .SetDisplayName("Test3")
                .SetFullyQualifiedName("TestProject.SomeTests.Test3")
                .SetOutcome(TestOutcome.Skipped)
                .Build(),
            new TestResultBuilder()
                .SetDisplayName("Test4")
                .SetFullyQualifiedName("TestProject.SomeTests.Test4")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage4")
                .Build()
        );

        // Assert
        var output = summaryWriter.ToString().Trim();

        output.Should().Contain("Test1");
        output.Should().Contain("Test2");
        output.Should().Contain("Test3");
        output.Should().Contain("Test4");

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_a_summary_that_includes_empty_test_suites()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var context = new TestReporterContext(
            new GitHubWorkflow(TextWriter.Null, summaryWriter),
            TestReporterOptions.Default
        );

        // Act
        context.SimulateTestRun();

        // Assert
        var output = summaryWriter.ToString().Trim();
        output.Should().Contain("⚪️ FakeTests");

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_a_summary_that_does_not_include_empty_test_suites()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var context = new TestReporterContext(
            new GitHubWorkflow(TextWriter.Null, summaryWriter),
            new TestReporterOptions { SummaryIncludeNotFoundTests = false }
        );

        // Act
        context.SimulateTestRun();

        // Assert
        var output = summaryWriter.ToString().Trim();
        output.Should().BeNullOrEmpty();

        testOutput.WriteLine(output);
    }
}
