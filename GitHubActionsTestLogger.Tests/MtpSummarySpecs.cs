using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using GitHubActionsTestLogger.Tests.Mtp;
using GitHubActionsTestLogger.Tests.Utils;
using GitHubActionsTestLogger.Tests.Utils.Extensions;
using Microsoft.Testing.Platform.Builder;
using Xunit;
using Xunit.Abstractions;

namespace GitHubActionsTestLogger.Tests;

public class MtpSummarySpecs(ITestOutputHelper testOutput)
{
    [Fact]
    public async Task I_can_use_the_logger_to_produce_a_summary_that_includes_the_test_suite_name()
    {
        // Arrange
        using var testResultsDir = TempDir.Create();
        await using var summaryWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            testResultsDir.Path,
            "--report-github",
            "--report-github-summary-allow-empty",
        ]);

        builder.RegisterFakeTests();
        builder.AddGitHubActionsReporting(TextWriter.Null, summaryWriter);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = summaryWriter.ToString().Trim();

        output.Should().Contain(Assembly.GetEntryAssembly()?.GetName().Name);

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_use_the_logger_to_produce_a_summary_that_includes_the_list_of_failed_tests()
    {
        // Arrange
        using var testResultsDir = TempDir.Create();
        await using var summaryWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            testResultsDir.Path,
            "--report-github",
        ]);

        builder.RegisterFakeTests(
            new TestNodeBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage1")
                .Build(),
            new TestNodeBuilder()
                .SetDisplayName("Test2")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage2")
                .Build(),
            new TestNodeBuilder()
                .SetDisplayName("Test3")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage3")
                .Build(),
            new TestNodeBuilder().SetDisplayName("Test4").SetOutcome(TestOutcome.Passed).Build(),
            new TestNodeBuilder().SetDisplayName("Test5").SetOutcome(TestOutcome.Skipped).Build()
        );

        builder.AddGitHubActionsReporting(TextWriter.Null, summaryWriter);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

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
    public async Task I_can_use_the_logger_to_produce_a_summary_that_includes_the_list_of_passed_tests()
    {
        // Arrange
        using var testResultsDir = TempDir.Create();
        await using var summaryWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            testResultsDir.Path,
            "--report-github",
            "--report-github-summary-include-passed",
        ]);

        builder.RegisterFakeTests(
            new TestNodeBuilder().SetDisplayName("Test1").SetOutcome(TestOutcome.Passed).Build(),
            new TestNodeBuilder().SetDisplayName("Test2").SetOutcome(TestOutcome.Passed).Build(),
            new TestNodeBuilder().SetDisplayName("Test3").SetOutcome(TestOutcome.Passed).Build(),
            new TestNodeBuilder()
                .SetDisplayName("Test4")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage4")
                .Build()
        );

        builder.AddGitHubActionsReporting(TextWriter.Null, summaryWriter);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = summaryWriter.ToString().Trim();

        output.Should().Contain("Test1");
        output.Should().Contain("Test2");
        output.Should().Contain("Test3");
        output.Should().Contain("Test4");

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_use_the_logger_to_produce_a_summary_that_does_not_include_the_list_of_passed_tests()
    {
        // Arrange
        using var testResultsDir = TempDir.Create();
        await using var summaryWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            testResultsDir.Path,
            "--report-github",
            "--report-github-summary-include-passed",
            "false",
        ]);

        builder.RegisterFakeTests(
            new TestNodeBuilder().SetDisplayName("Test1").SetOutcome(TestOutcome.Passed).Build(),
            new TestNodeBuilder().SetDisplayName("Test2").SetOutcome(TestOutcome.Passed).Build(),
            new TestNodeBuilder().SetDisplayName("Test3").SetOutcome(TestOutcome.Passed).Build(),
            new TestNodeBuilder()
                .SetDisplayName("Test4")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage4")
                .Build()
        );

        builder.AddGitHubActionsReporting(TextWriter.Null, summaryWriter);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = summaryWriter.ToString().Trim();

        output.Should().NotContain("Test1");
        output.Should().NotContain("Test2");
        output.Should().NotContain("Test3");
        output.Should().Contain("Test4");

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_use_the_logger_to_produce_a_summary_that_includes_the_list_of_skipped_tests()
    {
        // Arrange
        using var testResultsDir = TempDir.Create();
        await using var summaryWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            testResultsDir.Path,
            "--report-github",
            "--report-github-summary-include-skipped",
        ]);

        builder.RegisterFakeTests(
            new TestNodeBuilder().SetDisplayName("Test1").SetOutcome(TestOutcome.Skipped).Build(),
            new TestNodeBuilder().SetDisplayName("Test2").SetOutcome(TestOutcome.Skipped).Build(),
            new TestNodeBuilder().SetDisplayName("Test3").SetOutcome(TestOutcome.Skipped).Build(),
            new TestNodeBuilder()
                .SetDisplayName("Test4")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage4")
                .Build()
        );

        builder.AddGitHubActionsReporting(TextWriter.Null, summaryWriter);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = summaryWriter.ToString().Trim();

        output.Should().Contain("Test1");
        output.Should().Contain("Test2");
        output.Should().Contain("Test3");
        output.Should().Contain("Test4");

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_use_the_logger_to_produce_a_summary_that_does_not_include_the_list_of_skipped_tests()
    {
        // Arrange
        using var testResultsDir = TempDir.Create();
        await using var summaryWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            testResultsDir.Path,
            "--report-github",
            "--report-github-summary-include-skipped",
            "false",
        ]);

        builder.RegisterFakeTests(
            new TestNodeBuilder().SetDisplayName("Test1").SetOutcome(TestOutcome.Skipped).Build(),
            new TestNodeBuilder().SetDisplayName("Test2").SetOutcome(TestOutcome.Skipped).Build(),
            new TestNodeBuilder().SetDisplayName("Test3").SetOutcome(TestOutcome.Skipped).Build(),
            new TestNodeBuilder()
                .SetDisplayName("Test4")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage4")
                .Build()
        );

        builder.AddGitHubActionsReporting(TextWriter.Null, summaryWriter);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = summaryWriter.ToString().Trim();

        output.Should().NotContain("Test1");
        output.Should().NotContain("Test2");
        output.Should().NotContain("Test3");
        output.Should().Contain("Test4");

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_use_the_logger_to_produce_a_summary_that_includes_empty_test_runs()
    {
        // Arrange
        using var testResultsDir = TempDir.Create();
        await using var summaryWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            testResultsDir.Path,
            "--report-github",
            "--report-github-summary-allow-empty",
        ]);

        builder.RegisterFakeTests();
        builder.AddGitHubActionsReporting(TextWriter.Null, summaryWriter);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = summaryWriter.ToString().Trim();
        output.Should().Contain("⚪️");

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_use_the_logger_to_produce_a_summary_that_does_not_include_empty_test_runs()
    {
        // Arrange
        using var testResultsDir = TempDir.Create();
        await using var summaryWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            testResultsDir.Path,
            "--report-github",
            "--report-github-summary-allow-empty",
            "false",
        ]);

        builder.RegisterFakeTests();
        builder.AddGitHubActionsReporting(TextWriter.Null, summaryWriter);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = summaryWriter.ToString().Trim();
        output.Should().BeNullOrEmpty();

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_try_to_use_the_logger_to_produce_a_summary_when_the_output_file_is_nearly_full_and_get_a_truncated_summary()
    {
        // Arrange
        using var testResultsDir = TempDir.Create();
        using var summaryFile = TempFile.Create();

        // Pre-fill the summary file to within ~1000 bytes of the 1 MiB limit.
        // This forces any summary larger than ~1000 bytes to be truncated.
        const int prefillSize = 1024 * 1024 - 1000;
        File.WriteAllZeroes(summaryFile.Path, prefillSize);

        using var commandWriter = new StringWriter();

        // Use a file-backed StreamWriter so that the file path is exposed internally
        await using var summaryFileStream = File.Open(
            summaryFile.Path,
            FileMode.Append,
            FileAccess.Write,
            FileShare.ReadWrite
        );
        await using var summaryWriter = new StreamWriter(summaryFileStream);

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            testResultsDir.Path,
            "--report-github",
            "--report-github-summary-include-passed",
        ]);

        builder.RegisterFakeTests(
            // Group A
            new TestNodeBuilder()
                .SetTypeName("GroupA")
                .SetDisplayName("TestGroupA_LongTestName_One")
                .SetOutcome(Mtp.TestOutcome.Passed)
                .Build(),
            new TestNodeBuilder()
                .SetTypeName("GroupA")
                .SetDisplayName("TestGroupA_LongTestName_Two")
                .SetOutcome(Mtp.TestOutcome.Failed)
                .SetErrorMessage("Expected: something, but got: something else (GroupA)")
                .Build(),
            // Group B
            new TestNodeBuilder()
                .SetTypeName("GroupB")
                .SetDisplayName("TestGroupB_LongTestName_One")
                .SetOutcome(Mtp.TestOutcome.Passed)
                .Build(),
            new TestNodeBuilder()
                .SetTypeName("GroupB")
                .SetDisplayName("TestGroupB_LongTestName_Two")
                .SetOutcome(Mtp.TestOutcome.Failed)
                .SetErrorMessage("Expected: something, but got: something else (GroupB)")
                .Build(),
            // Group C
            new TestNodeBuilder()
                .SetTypeName("GroupC")
                .SetDisplayName("TestGroupC_LongTestName_One")
                .SetOutcome(Mtp.TestOutcome.Passed)
                .Build(),
            new TestNodeBuilder()
                .SetTypeName("GroupC")
                .SetDisplayName("TestGroupC_LongTestName_Two")
                .SetOutcome(Mtp.TestOutcome.Failed)
                .SetErrorMessage("Expected: something, but got: something else (GroupC)")
                .Build()
        );

        builder.AddGitHubActionsReporting(commandWriter, summaryWriter);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        await summaryWriter.FlushAsync();

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
    public async Task I_can_try_to_use_the_logger_to_produce_a_summary_when_the_output_file_is_full_and_get_the_summary_omitted()
    {
        // Arrange
        using var testResultsDir = TempDir.Create();
        using var summaryFile = TempFile.Create();

        // Pre-fill the summary file to within 1 byte of the 1 MiB limit.
        // This leaves no room even for newlines, so the summary must be omitted.
        const int prefillSize = 1024 * 1024 - 1;
        File.WriteAllZeroes(summaryFile.Path, prefillSize);

        using var commandWriter = new StringWriter();

        // Use a file-backed StreamWriter so that the file path is exposed internally
        await using var summaryFileStream = File.Open(
            summaryFile.Path,
            FileMode.Append,
            FileAccess.Write,
            FileShare.ReadWrite
        );
        await using var summaryWriter = new StreamWriter(summaryFileStream);

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            testResultsDir.Path,
            "--report-github",
            "--report-github-summary-allow-empty",
        ]);

        builder.RegisterFakeTests();
        builder.AddGitHubActionsReporting(commandWriter, summaryWriter);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        await summaryWriter.FlushAsync();

        // Assert
        var commandOutput = commandWriter.ToString();
        var fileLength = new FileInfo(summaryFile.Path).Length;

        commandOutput.Should().ContainAll("::warning", "omitted");
        fileLength.Should().Be(prefillSize);

        testOutput.WriteLine("Command output:");
        testOutput.WriteLine(commandOutput);
    }
}
