using System.IO;
using FluentAssertions;
using GitHubActionsTestLogger.Tests.Fixtures;
using GitHubActionsTestLogger.Tests.Utils;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class SummarySpecs : IClassFixture<TempOutputFixture>
{
    private readonly TempOutputFixture _tempOutput;

    public SummarySpecs(TempOutputFixture tempOutput) => _tempOutput = tempOutput;

    [Fact]
    public void Summary_contains_all_test_results()
    {
        // Arrange
        var summaryFilePath = _tempOutput.GetTempFilePath();
        using (EnvironmentVariable.Set("GITHUB_STEP_SUMMARY", summaryFilePath))
        {
            var context = new TestLoggerContext(TextWriter.Null, TestLoggerOptions.Default);

            var testResults = new[]
            {
                new TestResult(new TestCase
                {
                    DisplayName = "Test1"
                })
                {
                    Outcome = TestOutcome.Failed,
                    ErrorMessage = "ErrorMessage"
                },
                new TestResult(new TestCase
                {
                    DisplayName = "Test2"
                })
                {
                    Outcome = TestOutcome.Passed
                },
                new TestResult(new TestCase
                {
                    DisplayName = "Test3"
                })
                {
                    Outcome = TestOutcome.Skipped
                }
            };

            // Act
            foreach (var testResult in testResults)
                context.HandleTestResult(testResult);

            context.HandleTestRunCompletion();

            // Assert
            var output = File.ReadAllText(summaryFilePath);

            foreach (var testResult in testResults)
            {
                output.Should().Contain(testResult.TestCase.DisplayName);
                output.Should().Contain(testResult.Outcome.ToString());

                if (!string.IsNullOrWhiteSpace(testResult.ErrorMessage))
                    output.Should().Contain(testResult.ErrorMessage);
            }
        }
    }
}