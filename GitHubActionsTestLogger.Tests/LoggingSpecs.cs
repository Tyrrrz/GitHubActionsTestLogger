using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace GitHubActionsTestLogger.Tests
{
    public class LoggingSpecs
    {
        [Fact]
        public void Passed_tests_do_not_get_logged()
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
            context.ProcessTestResult(testResult);

            // Assert
            writer.ToString().Should().BeEmpty();
        }

        [Fact]
        public void Failed_tests_get_logged()
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
                ErrorMessage = "Bla bla"
            };

            // Act
            context.ProcessTestResult(testResult);

            // Assert
            writer.ToString().Should().MatchRegex(
                @"^\:\:error.*?\:\:.+$"
            );
        }

        [Fact]
        public void Failed_tests_get_logged_with_source_information_if_exception_was_thrown()
        {
            // .NET test platform never sends source information, so we can only
            // rely on exception stack traces for it.

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
                FullyQualifiedName = "CliWrap.Tests.CancellationSpecs.I_can_execute_a_command_with_buffering_and_cancel_it_immediately()"
            })
            {
                Outcome = TestOutcome.Failed,
                ErrorMessage = "Bla bla",
                ErrorStackTrace = stackTrace
            };

            // Act
            context.ProcessTestResult(testResult);

            // Assert
            writer.ToString().Should().MatchRegex(
                @"^\:\:error.*?file=/dir/CliWrap.Tests/CancellationSpecs.cs,line=75.*?\:\:.+$"
            );
        }

        [Fact]
        public void Skipped_tests_get_logged()
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
            context.ProcessTestResult(testResult);

            // Assert
            writer.ToString().Should().MatchRegex(
                @"^\:\:warning.*?\:\:.+$"
            );
        }

        [Fact]
        public void Skipped_tests_do_not_get_logged_if_configured()
        {
            // Arrange
            using var writer = new StringWriter();
            var context = new TestLoggerContext(writer, new TestLoggerOptions(false));

            var testResult = new TestResult(new TestCase
            {
                DisplayName = "Test1"
            })
            {
                Outcome = TestOutcome.Skipped
            };

            // Act
            context.ProcessTestResult(testResult);

            // Assert
            writer.ToString().Should().BeEmpty();
        }
    }
}