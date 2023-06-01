using System.IO;
using FluentAssertions;
using GitHubActionsTestLogger.Tests.Utils;
using GitHubActionsTestLogger.Tests.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;
using Xunit.Abstractions;

namespace GitHubActionsTestLogger.Tests;

public class AnnotationSpecs
{
    private readonly ITestOutputHelper _testOutput;

    public AnnotationSpecs(ITestOutputHelper testOutput) =>
        _testOutput = testOutput;

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_for_failed_tests()
    {
        // Arrange
        using var commandWriter = new StringWriter();

        var context = new TestLoggerContext(
            new GitHubWorkflow(
                commandWriter,
                TextWriter.Null
            ),
            TestLoggerOptions.Default
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage")
                .Build(),
            new TestResultBuilder()
                .SetDisplayName("Test2")
                .SetOutcome(TestOutcome.Passed)
                .Build(),
            new TestResultBuilder()
                .SetDisplayName("Test3")
                .SetOutcome(TestOutcome.Skipped)
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().StartWith("::error ");
        output.Should().Contain("Test1");
        output.Should().Contain("ErrorMessage");

        output.Should().NotContain("Test2");
        output.Should().NotContain("Test3");

        _testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_source_information()
    {
        // .NET test platform never sends source information, so we can only
        // rely on exception stack traces to get it.

        // Arrange
        using var commandWriter = new StringWriter();

        var context = new TestLoggerContext(
            new GitHubWorkflow(
                commandWriter,
                TextWriter.Null
            ),
            TestLoggerOptions.Default
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("I can execute a command with buffering and cancel it immediately")
                .SetFullyQualifiedName(
                    "CliWrap.Tests.CancellationSpecs.I_can_execute_a_command_with_buffering_and_cancel_it_immediately()"
                )
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

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().StartWith("::error ");
        output.Should().Contain("file=/dir/CliWrap.Tests/CancellationSpecs.cs");
        output.Should().Contain("line=75");
        output.Should().Contain("I can execute a command with buffering and cancel it immediately");
        output.Should().Contain("ErrorMessage");

        _testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_source_information_for_async_tests()
    {
        // .NET test platform never sends source information, so we can only
        // rely on exception stack traces to get it.

        // Arrange
        using var commandWriter = new StringWriter();

        var context = new TestLoggerContext(
            new GitHubWorkflow(
                commandWriter,
                TextWriter.Null
            ),
            TestLoggerOptions.Default
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("SendEnvelopeAsync_ItemRateLimit_DropsItem")
                .SetFullyQualifiedName(
                    "Sentry.Tests.Internals.Http.HttpTransportTests.SendEnvelopeAsync_ItemRateLimit_DropsItem()"
                )
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

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().StartWith("::error ");
        output.Should().Contain("file=/dir/Sentry.Tests/Internals/Http/HttpTransportTests.cs");
        output.Should().Contain("line=187");
        output.Should().Contain("SendEnvelopeAsync_ItemRateLimit_DropsItem");
        output.Should().Contain("ErrorMessage");

        _testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_the_test_name()
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
                AnnotationTitleFormat = "<@test>",
                AnnotationMessageFormat = "[@test]"
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
        output.Should().Contain("[Test1]");

        _testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_test_traits()
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
                AnnotationTitleFormat = "<@traits.Category -> @test>",
                AnnotationMessageFormat = "[@traits.Category -> @test]"
            }
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetTrait("Category", "UI Test")
                .SetTrait("Document", "SS01")
                .SetOutcome(TestOutcome.Failed)
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<UI Test -> Test1>");
        output.Should().Contain("[UI Test -> Test1]");

        _testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_the_error_message()
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
                AnnotationTitleFormat = "<@test: @error>",
                AnnotationMessageFormat = "[@test: @error]"
            }
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage")
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<Test1: ErrorMessage>");
        output.Should().Contain("[Test1: ErrorMessage]");

        _testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_the_error_stacktrace()
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
                AnnotationTitleFormat = "<@test: @trace>",
                AnnotationMessageFormat = "[@test: @trace]"
            }
        );

        // Act
        context.SimulateTestRun(
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorStackTrace("ErrorStackTrace")
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<Test1: ErrorStackTrace>");
        output.Should().Contain("[Test1: ErrorStackTrace]");

        _testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_the_target_framework_version()
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
                AnnotationTitleFormat = "<@test (@framework)>",
                AnnotationMessageFormat = "[@test (@framework)]"
            }
        );

        // Act
        context.SimulateTestRun(
            "FakeTests.dll",
            "FakeTargetFramework",
            new TestResultBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorStackTrace("ErrorStackTrace")
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<Test1 (FakeTargetFramework)>");
        output.Should().Contain("[Test1 (FakeTargetFramework)]");

        _testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_line_breaks()
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
                AnnotationMessageFormat = "foo\\nbar"
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

        output.Should().Contain("foo%0Abar");

        _testOutput.WriteLine(output);
    }
}