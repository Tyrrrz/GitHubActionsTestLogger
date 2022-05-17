using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Xunit;

namespace GitHubActionsTestLogger.Tests;

public class SummarySpecs
{
    [Fact]
    public void Test_summary_contains_project_name()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var context = new TestLoggerContext(
            new GitHubWorkflow(
                TextWriter.Null,
                summaryWriter
            ),
            TestLoggerOptions.Default
        );

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
        context.HandleTestRunStart(new TestRunCriteria(new[] { "TestProject.dll" }, 100));

        context.HandleTestResult(testResult);

        context.HandleTestRunComplete(
            new TestRunStatistics(new Dictionary<TestOutcome, long>
            {
                [TestOutcome.Failed] = 1,
                [TestOutcome.Skipped] = 0,
                [TestOutcome.Passed] = 0
            }),
            TimeSpan.FromSeconds(15)
        );

        // Assert
        var output = summaryWriter.ToString().Trim();
        output.Should().Contain("TestProject");
    }

    [Fact]
    public void Test_summary_contains_all_test_results()
    {
        // Arrange
        using var summaryWriter = new StringWriter();

        var context = new TestLoggerContext(
            new GitHubWorkflow(
                TextWriter.Null,
                summaryWriter
            ),
            TestLoggerOptions.Default
        );

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
        context.HandleTestRunStart(new TestRunCriteria(new[] { "TestProject.dll" }, 100));

        foreach (var testResult in testResults)
            context.HandleTestResult(testResult);

        context.HandleTestRunComplete(
            new TestRunStatistics(new Dictionary<TestOutcome, long>
            {
                [TestOutcome.Failed] = 1,
                [TestOutcome.Skipped] = 0,
                [TestOutcome.Passed] = 0
            }),
            TimeSpan.FromSeconds(15)
        );

        // Assert
        var output = summaryWriter.ToString().Trim();

        foreach (var testResult in testResults)
        {
            output.Should().Contain(testResult.TestCase.DisplayName);
            output.Should().ContainEquivalentOf(testResult.Outcome.ToString());

            if (!string.IsNullOrWhiteSpace(testResult.ErrorMessage))
                output.Should().Contain(testResult.ErrorMessage);
        }
    }
}