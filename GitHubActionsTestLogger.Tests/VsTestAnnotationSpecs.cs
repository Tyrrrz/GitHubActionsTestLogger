using System.IO;
using FluentAssertions;
using GitHubActionsTestLogger.GitHub;
using GitHubActionsTestLogger.Reporting;
using GitHubActionsTestLogger.Tests.VsTest;
using Xunit;
using Xunit.Abstractions;
using TestOutcome = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestOutcome;

namespace GitHubActionsTestLogger.Tests;

public class VsTestAnnotationSpecs(ITestOutputHelper testOutput)
{
    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_for_failed_tests()
    {
        // Arrange
        using var commandWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new TestReportingContext(
                new GitHubWorkflow(commandWriter, TextWriter.Null),
                TestReportingOptions.Default
            )
        );

        // Act
        events.SimulateTestRun(
            new VsTestResultBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage")
                .Build(),
            new VsTestResultBuilder()
                .SetDisplayName("Test2")
                .SetOutcome(TestOutcome.Passed)
                .Build(),
            new VsTestResultBuilder()
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

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_source_information()
    {
        // .NET test platform never sends source information, so we can only
        // rely on exception stack traces to get it.

        // Arrange
        using var commandWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new TestReportingContext(
                new GitHubWorkflow(commandWriter, TextWriter.Null),
                TestReportingOptions.Default
            )
        );

        // Act
        events.SimulateTestRun(
            new VsTestResultBuilder()
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

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_source_information_for_async_tests()
    {
        // .NET test platform never sends source information, so we can only
        // rely on exception stack traces to get it.

        // Arrange
        using var commandWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new TestReportingContext(
                new GitHubWorkflow(commandWriter, TextWriter.Null),
                TestReportingOptions.Default
            )
        );

        // Act
        events.SimulateTestRun(
            new VsTestResultBuilder()
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

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_the_test_name()
    {
        // Arrange
        using var commandWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new TestReportingContext(
                new GitHubWorkflow(commandWriter, TextWriter.Null),
                new TestReportingOptions
                {
                    AnnotationTitleFormat = "<@test>",
                    AnnotationMessageFormat = "[@test]",
                }
            )
        );

        // Act
        events.SimulateTestRun(
            new VsTestResultBuilder().SetDisplayName("Test1").SetOutcome(TestOutcome.Failed).Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<Test1>");
        output.Should().Contain("[Test1]");

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_test_traits()
    {
        // Arrange
        using var commandWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new TestReportingContext(
                new GitHubWorkflow(commandWriter, TextWriter.Null),
                new TestReportingOptions
                {
                    AnnotationTitleFormat = "<@traits.Category -> @test>",
                    AnnotationMessageFormat = "[@traits.Category -> @test]",
                }
            )
        );

        // Act
        events.SimulateTestRun(
            new VsTestResultBuilder()
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

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_the_error_message()
    {
        // Arrange
        using var commandWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new TestReportingContext(
                new GitHubWorkflow(commandWriter, TextWriter.Null),
                new TestReportingOptions
                {
                    AnnotationTitleFormat = "<@test: @error>",
                    AnnotationMessageFormat = "[@test: @error]",
                }
            )
        );

        // Act
        events.SimulateTestRun(
            new VsTestResultBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorMessage("ErrorMessage")
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<Test1: ErrorMessage>");
        output.Should().Contain("[Test1: ErrorMessage]");

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_the_error_stacktrace()
    {
        // Arrange
        using var commandWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new TestReportingContext(
                new GitHubWorkflow(commandWriter, TextWriter.Null),
                new TestReportingOptions
                {
                    AnnotationTitleFormat = "<@test: @trace>",
                    AnnotationMessageFormat = "[@test: @trace]",
                }
            )
        );

        // Act
        events.SimulateTestRun(
            new VsTestResultBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorStackTrace("ErrorStackTrace")
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<Test1: ErrorStackTrace>");
        output.Should().Contain("[Test1: ErrorStackTrace]");

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_the_target_framework_version()
    {
        // Arrange
        using var commandWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new TestReportingContext(
                new GitHubWorkflow(commandWriter, TextWriter.Null),
                new TestReportingOptions
                {
                    AnnotationTitleFormat = "<@test (@framework)>",
                    AnnotationMessageFormat = "[@test (@framework)]",
                }
            )
        );

        // Act
        events.SimulateTestRun(
            "FakeTests.dll",
            "FakeTargetFramework",
            new VsTestResultBuilder()
                .SetDisplayName("Test1")
                .SetOutcome(TestOutcome.Failed)
                .SetErrorStackTrace("ErrorStackTrace")
                .Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("<Test1 (FakeTargetFramework)>");
        output.Should().Contain("[Test1 (FakeTargetFramework)]");

        testOutput.WriteLine(output);
    }

    [Fact]
    public void I_can_use_the_logger_to_produce_annotations_that_include_line_breaks()
    {
        // Arrange
        using var commandWriter = new StringWriter();

        var events = new FakeTestLoggerEvents();
        var logger = new VsTestLogger();

        logger.Initialize(
            events,
            new TestReportingContext(
                new GitHubWorkflow(commandWriter, TextWriter.Null),
                new TestReportingOptions { AnnotationMessageFormat = "foo\\nbar" }
            )
        );

        // Act
        events.SimulateTestRun(
            new VsTestResultBuilder().SetDisplayName("Test1").SetOutcome(TestOutcome.Failed).Build()
        );

        // Assert
        var output = commandWriter.ToString().Trim();

        output.Should().Contain("foo%0Abar");

        testOutput.WriteLine(output);
    }
}
