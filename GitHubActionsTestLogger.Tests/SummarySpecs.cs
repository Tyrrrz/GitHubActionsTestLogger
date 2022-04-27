using System.IO;
using FluentAssertions;
using GitHubActionsTestLogger.Tests.Fixtures;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class SummarySpecs : IClassFixture<TempOutputFixture>
{
    private readonly TempOutputFixture _tempOutput;

    public SummarySpecs(TempOutputFixture tempOutput) => _tempOutput = tempOutput;

    [Fact]
    public void Test_summary_contains_project_name()
    {
        // Arrange
        var summaryFilePath = _tempOutput.GetTempFilePath();
        using var context = new TestLoggerContext(TextWriter.Null, summaryFilePath, TestLoggerOptions.Default);

        var testResult = new TestResult(new TestCase
        {
            DisplayName = "Test1",
            FullyQualifiedName = "TestProject.SomeTests.Test1"
        })
        {
            Outcome = TestOutcome.Failed,
            ErrorMessage = "ErrorMessage"
        };

        // Act
        context.HandleTestRunStart(
            new TestRunStartEventArgs(new TestRunCriteria(new[] {"TestProject.dll"}, 100))
        );

        context.HandleTestResult(new TestResultEventArgs(testResult));

        // Assert
        var output = File.ReadAllText(summaryFilePath);
        output.Should().Contain("TestProject");
    }

    [Fact]
    public void Test_summary_contains_all_test_results()
    {
        // Arrange
        var summaryFilePath = _tempOutput.GetTempFilePath();
        using var context = new TestLoggerContext(TextWriter.Null, summaryFilePath, TestLoggerOptions.Default);

        var testResults = new[]
        {
            new TestResult(new TestCase
            {
                DisplayName = "Test1",
                FullyQualifiedName = "TestProject.SomeTests.Test1"
            })
            {
                Outcome = TestOutcome.Failed,
                ErrorMessage = "ErrorMessage"
            },
            new TestResult(new TestCase
            {
                DisplayName = "Test2",
                FullyQualifiedName = "TestProject.SomeTests.Test2"
            })
            {
                Outcome = TestOutcome.Failed
            },
            new TestResult(new TestCase
            {
                DisplayName = "Test3",
                FullyQualifiedName = "TestProject.SomeTests.Test3"
            })
            {
                Outcome = TestOutcome.Failed
            }
        };

        // Act
        context.HandleTestRunStart(
            new TestRunStartEventArgs(new TestRunCriteria(new[] {"TestProject.dll"}, 100))
        );

        foreach (var testResult in testResults)
            context.HandleTestResult(new TestResultEventArgs(testResult));

        // Assert
        var output = File.ReadAllText(summaryFilePath);

        foreach (var testResult in testResults)
        {
            output.Should().Contain(testResult.TestCase.DisplayName);
            output.Should().ContainEquivalentOf(testResult.Outcome.ToString());

            if (!string.IsNullOrWhiteSpace(testResult.ErrorMessage))
                output.Should().Contain(testResult.ErrorMessage);
        }
    }
}