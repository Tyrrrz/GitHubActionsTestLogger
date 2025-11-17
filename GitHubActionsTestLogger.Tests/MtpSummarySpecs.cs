using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using GitHubActionsTestLogger.Tests.Mtp;
using GitHubActionsTestLogger.Tests.Utils;
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
}
