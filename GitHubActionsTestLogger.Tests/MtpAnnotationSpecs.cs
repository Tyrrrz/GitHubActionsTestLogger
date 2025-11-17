using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using FluentAssertions;
using GitHubActionsTestLogger.Tests.Mtp;
using Microsoft.Testing.Platform.Builder;
using Xunit;
using Xunit.Abstractions;

namespace GitHubActionsTestLogger.Tests;

public class MtpAnnotationSpecs(ITestOutputHelper testOutput)
{
    [Fact]
    public async Task I_can_use_the_logger_to_produce_annotations_for_failed_tests()
    {
        // Arrange
        await using var commandWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            Path.Combine(Directory.GetCurrentDirectory(), "FakeTestResults"),
            "--report-github",
        ]);

        builder.RegisterFakeTests(
            new TestNodeBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage")
                .Build(),
            new TestNodeBuilder().SetDisplayName("Test2").SetOutcome(TestOutcome.Passed).Build(),
            new TestNodeBuilder().SetDisplayName("Test3").SetOutcome(TestOutcome.Skipped).Build()
        );

        builder.AddGitHubActionsReporting(commandWriter, TextWriter.Null);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().StartWith("::error ");
        output.Should().Contain("Test1");
        output.Should().Contain("ErrorMessage");

        output.Should().NotContain("Test2");
        output.Should().NotContain("Test3");

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_use_the_logger_to_produce_annotations_that_include_source_information_extracted_from_exceptions()
    {
        // Arrange
        await using var commandWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            Path.Combine(Directory.GetCurrentDirectory(), "FakeTestResults"),
            "--report-github",
        ]);

        builder.RegisterFakeTests(
            new TestNodeBuilder()
                .SetDisplayName("I can execute a command with buffering and cancel it immediately")
                .SetNamespace("CliWrap.Tests")
                .SetTypeName("CancellationSpecs")
                .SetMethodName("I_can_execute_a_command_with_buffering_and_cancel_it_immediately")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage")
                .SetErrorStackTrace(
                    """
                    at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
                    at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
                    at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
                    at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
                    at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
                    at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
                    at FluentAssertions.Primitives.BooleanAssertions.BeFalse(String because, Object[] becauseArgs)
                    at CliWrap.Tests.CancellationSpecs.I_can_execute_a_command_with_buffering_and_cancel_it_immediately() in /dir/CliWrap.Tests/CancellationSpecs.cs:line 75
                    """
                )
                .Build()
        );

        builder.AddGitHubActionsReporting(commandWriter, TextWriter.Null);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().StartWith("::error ");
        output.Should().Contain("file=/dir/CliWrap.Tests/CancellationSpecs.cs");
        output.Should().Contain("line=75");
        output.Should().Contain("I can execute a command with buffering and cancel it immediately");
        output.Should().Contain("ErrorMessage");

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_use_the_logger_to_produce_annotations_that_include_source_information_extracted_from_async_exceptions()
    {
        // Arrange
        await using var commandWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            Path.Combine(Directory.GetCurrentDirectory(), "FakeTestResults"),
            "--report-github",
        ]);

        builder.RegisterFakeTests(
            new TestNodeBuilder()
                .SetDisplayName("SendEnvelopeAsync_ItemRateLimit_DropsItem")
                .SetNamespace("Sentry.Tests.Internals.Http")
                .SetTypeName("HttpTransportTests")
                .SetMethodName("SendEnvelopeAsync_ItemRateLimit_DropsItem")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage")
                .SetErrorStackTrace(
                    """
                    at System.Net.Http.HttpContent.CheckDisposed()
                    at System.Net.Http.HttpContent.ReadAsStringAsync()
                    at Sentry.Tests.Internals.Http.HttpTransportTests.<SendEnvelopeAsync_ItemRateLimit_DropsItem>d__3.MoveNext() in /dir/Sentry.Tests/Internals/Http/HttpTransportTests.cs:line 187
                    --- End of stack trace from previous location where exception was thrown ---
                    at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
                    at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                    --- End of stack trace from previous location where exception was thrown ---
                    at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
                    at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                    --- End of stack trace from previous location where exception was thrown ---
                    at System.Runtime.CompilerServices.TaskAwaiter.ThrowForNonSuccess(Task task)
                    at System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
                    """
                )
                .Build()
        );

        builder.AddGitHubActionsReporting(commandWriter, TextWriter.Null);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().StartWith("::error ");
        output.Should().Contain("file=/dir/Sentry.Tests/Internals/Http/HttpTransportTests.cs");
        output.Should().Contain("line=187");
        output.Should().Contain("SendEnvelopeAsync_ItemRateLimit_DropsItem");
        output.Should().Contain("ErrorMessage");

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_use_the_logger_to_produce_annotations_that_include_the_test_name()
    {
        // Arrange
        await using var commandWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            Path.Combine(Directory.GetCurrentDirectory(), "FakeTestResults"),
            "--report-github",
            "--report-github-annotations-title",
            "<@test>",
            "--report-github-annotations-message",
            "[@test]",
        ]);

        builder.RegisterFakeTests(
            new TestNodeBuilder().SetDisplayName("Test1").SetOutcome(TestOutcome.Failed).Build()
        );

        builder.AddGitHubActionsReporting(commandWriter, TextWriter.Null);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<Test1>");
        output.Should().Contain("[Test1]");

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_use_the_logger_to_produce_annotations_that_include_test_traits()
    {
        // Arrange
        await using var commandWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            Path.Combine(Directory.GetCurrentDirectory(), "FakeTestResults"),
            "--report-github",
            "--report-github-annotations-title",
            "<@traits.Category -> @test>",
            "--report-github-annotations-message",
            "[@traits.Category -> @test]",
        ]);

        builder.RegisterFakeTests(
            new TestNodeBuilder()
                .SetDisplayName("Test1")
                .SetTrait("Category", "UI Test")
                .SetTrait("Document", "SS01")
                .SetOutcome(TestOutcome.Failed)
                .Build()
        );

        builder.AddGitHubActionsReporting(commandWriter, TextWriter.Null);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<UI Test -> Test1>");
        output.Should().Contain("[UI Test -> Test1]");

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_use_the_logger_to_produce_annotations_that_include_the_error_message()
    {
        // Arrange
        await using var commandWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            Path.Combine(Directory.GetCurrentDirectory(), "FakeTestResults"),
            "--report-github",
            "--report-github-annotations-title",
            "<@test: @error>",
            "--report-github-annotations-message",
            "[@test: @error]",
        ]);

        builder.RegisterFakeTests(
            new TestNodeBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage")
                .Build()
        );

        builder.AddGitHubActionsReporting(commandWriter, TextWriter.Null);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<Test1: ErrorMessage>");
        output.Should().Contain("[Test1: ErrorMessage]");

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_use_the_logger_to_produce_annotations_that_include_the_error_stacktrace()
    {
        // Arrange
        await using var commandWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            Path.Combine(Directory.GetCurrentDirectory(), "FakeTestResults"),
            "--report-github",
            "--report-github-annotations-title",
            "<@test: @trace>",
            "--report-github-annotations-message",
            "[@test: @trace]",
        ]);

        builder.RegisterFakeTests(
            new TestNodeBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorStackTrace("ErrorStackTrace")
                .Build()
        );

        builder.AddGitHubActionsReporting(commandWriter, TextWriter.Null);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = commandWriter.ToString().Trim();

        // .NET adds additional text to the end of the injected stack traces, so only check the beginning
        output.Should().Contain("<Test1: ErrorStackTrace");
        output.Should().Contain("[Test1: ErrorStackTrace");

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_use_the_logger_to_produce_annotations_that_include_the_target_framework_version()
    {
        // Arrange
        await using var commandWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            Path.Combine(Directory.GetCurrentDirectory(), "FakeTestResults"),
            "--report-github",
            "--report-github-annotations-title",
            "<@test (@framework)>",
            "--report-github-annotations-message",
            "[@test (@framework)]",
        ]);

        // Can't inject custom framework with MTP since it's not passed as metadata
        var framework = AppContext.TargetFrameworkName ?? "UnknownFramework";

        builder.RegisterFakeTests(
            new TestNodeBuilder().SetDisplayName("Test1").SetOutcome(TestOutcome.Failed).Build()
        );

        builder.AddGitHubActionsReporting(commandWriter, TextWriter.Null);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain($"<Test1 ({framework})>");
        output.Should().Contain($"[Test1 ({framework})]");

        testOutput.WriteLine(output);
    }

    [Fact]
    public async Task I_can_use_the_logger_to_produce_annotations_that_include_line_breaks()
    {
        // Arrange
        await using var commandWriter = new StringWriter();

        var builder = await TestApplication.CreateBuilderAsync([
            "--results-directory",
            Path.Combine(Directory.GetCurrentDirectory(), "FakeTestResults"),
            "--report-github",
            "--report-github-annotations-message",
            "foo\\nbar",
        ]);

        builder.RegisterFakeTests(
            new TestNodeBuilder().SetDisplayName("Test1").SetOutcome(TestOutcome.Failed).Build()
        );

        builder.AddGitHubActionsReporting(commandWriter, TextWriter.Null);

        // Act
        var app = await builder.BuildAsync();
        await app.RunAsync();

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("foo%0Abar");

        testOutput.WriteLine(output);
    }
}
