using System.IO;
using FluentAssertions;
using GitHubActionsTestLogger.Tests.Utils;
using GitHubActionsTestLogger.Tests.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class AnnotationSpecs
{
    [Fact]
    public void Passed_tests_do_not_get_reported()
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
                .SetOutcome(TestOutcome.Passed)
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();
        output.Should().BeEmpty();
    }

    [Fact]
    public void Skipped_tests_do_not_get_reported()
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
                .SetOutcome(TestOutcome.Skipped)
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();
        output.Should().BeEmpty();
    }

    [Fact]
    public void Failed_tests_get_reported()
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
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().StartWith("::error ");
        output.Should().Contain("Test1");
        output.Should().Contain("ErrorMessage");
    }

    [Fact]
    public void Failed_tests_get_reported_with_source_information_if_exception_was_thrown()
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
                .SetErrorStackTrace(@"
at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
at FluentAssertions.Primitives.BooleanAssertions.BeFalse(String because, Object[] becauseArgs)
at CliWrap.Tests.CancellationSpecs.I_can_execute_a_command_with_buffering_and_cancel_it_immediately() in /dir/CliWrap.Tests/CancellationSpecs.cs:line 75
"
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
    }

    [Fact]
    public void Failed_tests_get_reported_with_source_information_if_exception_was_thrown_in_an_async_method()
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
                .SetErrorStackTrace(@"
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
"
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
    }

    [Fact]
    public void Failed_tests_get_reported_with_approximate_source_information_if_exception_was_not_thrown()
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
                .SetSource(typeof(AnnotationSpecs).Assembly.Location)
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage")
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().StartWith("::error ");
        output.Should().MatchRegex(@"file=.*?\.csproj");
        output.Should().Contain("Test1");
        output.Should().Contain("ErrorMessage");
    }
}