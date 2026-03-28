using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using GitHubActionsTestLogger.Tests.Utils;
using GitHubActionsTestLogger.Tests.Utils.Extensions;
using GitHubActionsTestLogger.Tests.VsTest;
using Xunit;
using Xunit.Abstractions;
using TestOutcome = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome;

namespace GitHubActionsTestLogger.Tests;

public class VsTestSummarySpecs(ITestOutputHelper testOutput)
{
    [Fact]
    public void I_can_use_the_logger_to_produce_a_summary_that_includes_the_test_suite_name()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new Dictionary<string, string?> { ["summary-allow-empty"] = "true" },
            TextWriter.Null,
            summaryWriter
        );

        // Act
        events.SimulateTestRun("TestProject.dll");

        // Assert
        var output = summaryWriter.ToString().Trim();

        output.Should().Contain("TestProject");

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_a_summary_that_includes_the_list_of_failed_tests()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(events, [], TextWriter.Null, summaryWriter);

        // Act
        events.SimulateTestRun(
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

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_a_summary_that_includes_the_list_of_passed_tests()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new Dictionary<string, string?> { ["summary-include-passed"] = "true" },
            TextWriter.Null,
            summaryWriter
        );

        // Act
        events.SimulateTestRun(
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
    public void I_can_use_the_logger_to_produce_a_summary_that_does_not_include_the_list_of_passed_tests()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new Dictionary<string, string?> { ["summary-include-passed"] = "false" },
            TextWriter.Null,
            summaryWriter
        );

        // Act
        events.SimulateTestRun(
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

        output.Should().NotContain("Test1");
        output.Should().NotContain("Test2");
        output.Should().NotContain("Test3");
        output.Should().Contain("Test4");

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_a_summary_that_includes_the_list_of_skipped_tests()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new Dictionary<string, string?> { ["summary-include-skipped"] = "true" },
            TextWriter.Null,
            summaryWriter
        );

        // Act
        events.SimulateTestRun(
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
    public void I_can_use_the_logger_to_produce_a_summary_that_does_not_include_the_list_of_skipped_tests()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new Dictionary<string, string?> { ["summary-include-skipped"] = "false" },
            TextWriter.Null,
            summaryWriter
        );

        // Act
        events.SimulateTestRun(
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

        output.Should().NotContain("Test1");
        output.Should().NotContain("Test2");
        output.Should().NotContain("Test3");
        output.Should().Contain("Test4");

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_a_summary_that_includes_empty_test_runs()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new Dictionary<string, string?> { ["summary-allow-empty"] = "true" },
            TextWriter.Null,
            summaryWriter
        );

        // Act
        events.SimulateTestRun();

        // Assert
        var output = summaryWriter.ToString().Trim();
        output.Should().Contain("⚪️");

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_a_summary_that_does_not_include_empty_test_runs()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new Dictionary<string, string?> { ["summary-allow-empty"] = "false" },
            TextWriter.Null,
            summaryWriter
        );

        // Act
        events.SimulateTestRun();

        // Assert
        var output = summaryWriter.ToString().Trim();
        output.Should().BeNullOrEmpty();

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_try_to_use_the_logger_to_produce_a_summary_when_the_output_file_is_nearly_full_and_get_a_truncated_summary()
    {
        // Arrange
        using var summaryFile = TempFile.Create();

        // Pre-fill the summary file to within ~1000 bytes of the 1 MiB limit.
        // This forces any summary larger than ~1000 bytes to be truncated.
        const int prefillSize = 1024 * 1024 - 1000;
        File.WriteAllZeroes(summaryFile.Path, prefillSize);

        using var commandWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        // Use a file-backed StreamWriter so that the file path is exposed internally
        using var summaryFileStream = File.Open(
            summaryFile.Path,
            FileMode.Append,
            FileAccess.Write,
            FileShare.ReadWrite
        );
        using (var summaryWriter = new StreamWriter(summaryFileStream))
        {
            logger.Initialize(
                events,
                new Dictionary<string, string?> { ["summary-include-passed"] = "true" },
                commandWriter,
                summaryWriter
            );

            // Act — simulate a run with multiple test groups to produce a multi-group summary
            events.SimulateTestRun(
                // Group A
                new TestResultBuilder()
                    .SetDisplayName("TestGroupA_LongTestName_One")
                    .SetFullyQualifiedName("TestProject.GroupA.TestGroupA_LongTestName_One")
                    .SetOutcome(TestOutcome.Passed)
                    .Build(),
                new TestResultBuilder()
                    .SetDisplayName("TestGroupA_LongTestName_Two")
                    .SetFullyQualifiedName("TestProject.GroupA.TestGroupA_LongTestName_Two")
                    .SetOutcome(TestOutcome.Failed)
                    .SetErrorMessage("Expected: something, but got: something else (GroupA)")
                    .Build(),
                // Group B
                new TestResultBuilder()
                    .SetDisplayName("TestGroupB_LongTestName_One")
                    .SetFullyQualifiedName("TestProject.GroupB.TestGroupB_LongTestName_One")
                    .SetOutcome(TestOutcome.Passed)
                    .Build(),
                new TestResultBuilder()
                    .SetDisplayName("TestGroupB_LongTestName_Two")
                    .SetFullyQualifiedName("TestProject.GroupB.TestGroupB_LongTestName_Two")
                    .SetOutcome(TestOutcome.Failed)
                    .SetErrorMessage("Expected: something, but got: something else (GroupB)")
                    .Build(),
                // Group C
                new TestResultBuilder()
                    .SetDisplayName("TestGroupC_LongTestName_One")
                    .SetFullyQualifiedName("TestProject.GroupC.TestGroupC_LongTestName_One")
                    .SetOutcome(TestOutcome.Passed)
                    .Build(),
                new TestResultBuilder()
                    .SetDisplayName("TestGroupC_LongTestName_Two")
                    .SetFullyQualifiedName("TestProject.GroupC.TestGroupC_LongTestName_Two")
                    .SetOutcome(TestOutcome.Failed)
                    .SetErrorMessage("Expected: something, but got: something else (GroupC)")
                    .Build()
            );
        } // Dispose writer before reading to ensure content is fully flushed

        // Assert
        var commandOutput = commandWriter.ToString();

        var summaryOutput = Encoding.UTF8.GetString(
            File.ReadAllBytes(summaryFile.Path, prefillSize)
        );

        commandOutput.Should().ContainAll("::warning", "truncated");
        summaryOutput.Should().NotBeNullOrWhiteSpace();

        testOutput.WriteLine("Command output:");
        testOutput.WriteLine(commandOutput);
        testOutput.WriteLine("Summary output:");
        testOutput.WriteLine(summaryOutput);
    }

    [Fact]
    public void I_can_try_to_use_the_logger_to_produce_a_summary_when_the_output_file_is_full_and_get_the_summary_omitted()
    {
        // Arrange
        using var summaryFile = TempFile.Create();

        // Pre-fill the summary file to within 1 byte of the 1 MiB limit.
        // This leaves no room even for newlines, so the summary must be omitted.
        const int prefillSize = 1024 * 1024 - 1;
        File.WriteAllZeroes(summaryFile.Path, prefillSize);

        using var commandWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        // Use a file-backed StreamWriter so that the file path is exposed internally
        using var summaryFileStream = File.Open(
            summaryFile.Path,
            FileMode.Append,
            FileAccess.Write,
            FileShare.ReadWrite
        );
        using (var summaryWriter = new StreamWriter(summaryFileStream))
        {
            logger.Initialize(
                events,
                new Dictionary<string, string?> { ["summary-allow-empty"] = "true" },
                commandWriter,
                summaryWriter
            );

            // Act
            events.SimulateTestRun("TestProject.dll");
        } // Dispose writer before reading to ensure content is fully flushed

        // Assert
        var commandOutput = commandWriter.ToString();
        var fileLength = new FileInfo(summaryFile.Path).Length;

        commandOutput.Should().ContainAll("::warning", "omitted");
        fileLength.Should().Be(prefillSize);

        testOutput.WriteLine("Command output:");
        testOutput.WriteLine(commandOutput);
    }
}
