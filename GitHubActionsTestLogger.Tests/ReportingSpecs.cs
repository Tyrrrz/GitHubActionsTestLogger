using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class ReportingSpecs
{
    [Fact]
    public void Passed_tests_do_not_get_reported()
    {
        // Arrange
        using var writer = new StringWriter();
        var context = new TestLoggerContext(writer, TestLoggerOptions.Default);

        var testResult = new TestResult(new TestCase
        {
            DisplayName = "Test1"
        })
        {
            Outcome = TestOutcome.Passed
        };

        // Act
        context.HandleTestResult(testResult);

        // Assert
        var output = writer.ToString().Trim();

        output.Should().BeEmpty();
    }

    [Fact]
    public void Failed_tests_get_reported()
    {
        // Arrange
        using var writer = new StringWriter();
        var context = new TestLoggerContext(writer, TestLoggerOptions.Default);

        var testResult = new TestResult(new TestCase
        {
            DisplayName = "Test1"
        })
        {
            Outcome = TestOutcome.Failed,
            ErrorMessage = "ErrorMessage"
        };

        // Act
        context.HandleTestResult(testResult);

        // Assert
        var output = writer.ToString().Trim();

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
        using var writer = new StringWriter();
        var context = new TestLoggerContext(writer, TestLoggerOptions.Default);

        var stackTrace = @"
at FluentAssertions.Execution.XUnit2TestFramework.Throw(String message)
at FluentAssertions.Execution.TestFrameworkProvider.Throw(String message)
at FluentAssertions.Execution.DefaultAssertionStrategy.HandleFailure(String message)
at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
at FluentAssertions.Execution.AssertionScope.FailWith(Func`1 failReasonFunc)
at FluentAssertions.Execution.AssertionScope.FailWith(String message, Object[] args)
at FluentAssertions.Primitives.BooleanAssertions.BeFalse(String because, Object[] becauseArgs)
at CliWrap.Tests.CancellationSpecs.I_can_execute_a_command_with_buffering_and_cancel_it_immediately() in /dir/CliWrap.Tests/CancellationSpecs.cs:line 75
".Trim();

        var testResult = new TestResult(new TestCase
        {
            DisplayName = "I can execute a command with buffering and cancel it immediately",
            FullyQualifiedName =
                "CliWrap.Tests.CancellationSpecs.I_can_execute_a_command_with_buffering_and_cancel_it_immediately()"
        })
        {
            Outcome = TestOutcome.Failed,
            ErrorMessage = "ErrorMessage",
            ErrorStackTrace = stackTrace
        };

        // Act
        context.HandleTestResult(testResult);

        // Assert
        var output = writer.ToString().Trim();

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
        using var writer = new StringWriter();
        var context = new TestLoggerContext(writer, TestLoggerOptions.Default);

        var stackTrace = @"
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
".Trim();

        var testResult = new TestResult(new TestCase
        {
            DisplayName = "SendEnvelopeAsync_ItemRateLimit_DropsItem",
            FullyQualifiedName =
                "Sentry.Tests.Internals.Http.HttpTransportTests.SendEnvelopeAsync_ItemRateLimit_DropsItem()"
        })
        {
            Outcome = TestOutcome.Failed,
            ErrorMessage = "ErrorMessage",
            ErrorStackTrace = stackTrace
        };

        // Act
        context.HandleTestResult(testResult);

        // Assert
        var output = writer.ToString().Trim();

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
        using var writer = new StringWriter();
        var context = new TestLoggerContext(writer, TestLoggerOptions.Default);

        var testResult = new TestResult(new TestCase
        {
            DisplayName = "Test1",
            Source = typeof(ReportingSpecs).Assembly.Location
        })
        {
            Outcome = TestOutcome.Failed,
            ErrorMessage = "ErrorMessage"
        };

        // Act
        context.HandleTestResult(testResult);

        // Assert
        var output = writer.ToString().Trim();

        output.Should().StartWith("::error ");
        output.Should().MatchRegex(@"file=.*?\.csproj");
        output.Should().Contain("Test1");
        output.Should().Contain("ErrorMessage");
    }

    [Fact]
    public void Skipped_tests_get_reported()
    {
        // Arrange
        using var writer = new StringWriter();
        var context = new TestLoggerContext(writer, TestLoggerOptions.Default);

        var testResult = new TestResult(new TestCase
        {
            DisplayName = "Test1"
        })
        {
            Outcome = TestOutcome.Skipped
        };

        // Act
        context.HandleTestResult(testResult);

        // Assert
        var output = writer.ToString().Trim();

        output.Should().StartWith("::warning ");
        output.Should().Contain("Test1");
        output.Should().Contain("Skipped");
    }

    [Fact]
    public void Skipped_tests_do_not_get_reported_if_configured()
    {
        // Arrange
        using var writer = new StringWriter();
        var context = new TestLoggerContext(
            writer,
            new TestLoggerOptions(
                TestLoggerOptions.Default.MessageFormat,
                false,
                TestLoggerOptions.Default.SummaryFilePath
            )
        );

        var testResult = new TestResult(new TestCase
        {
            DisplayName = "Test1"
        })
        {
            Outcome = TestOutcome.Skipped
        };

        // Act
        context.HandleTestResult(testResult);

        // Assert
        var output = writer.ToString().Trim();

        output.Should().BeEmpty();
    }
}