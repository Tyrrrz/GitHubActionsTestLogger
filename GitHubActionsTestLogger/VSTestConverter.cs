using System;
using System.Linq;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger;

public static class VSTestConverter
{
    public static LoggerTestResult ConvertTestResult(TestResult testResult)
    {
        return new LoggerTestResult(
            DisplayName: testResult.TestCase.DisplayName,
            MinimallyQualifiedName: testResult.TestCase.GetMinimallyQualifiedName(),
            FullyQualifiedName: testResult.TestCase.GetTypeFullyQualifiedName(),
            Traits: testResult.TestCase.Traits.ToDictionary(t => t.Name, t => t.Value) ?? new(),
            SourceFilePath: testResult.TestCase.CodeFilePath,
            SourceFileLine: testResult.TestCase.LineNumber,
            Outcome: (LoggerTestOutcome)testResult.Outcome,
            ErrorMessage: testResult.ErrorMessage,
            ErrorStackTrace: testResult.ErrorStackTrace
        );
    }

    public static LoggerTestRunCriteria ConvertTestRunCriteria(TestRunCriteria testRunCriteria)
    {
        return new LoggerTestRunCriteria(
            TargetFramework: testRunCriteria.TryGetTargetFramework(),
            Sources: testRunCriteria.Sources
        );
    }

    public static LoggerTestRunComplete ConvertTestRunComplete(TestRunCompleteEventArgs args)
    {
        return new LoggerTestRunComplete(
            PassedTests: args?.TestRunStatistics?[TestOutcome.Passed],
            FailedTests: args?.TestRunStatistics?[TestOutcome.Failed],
            SkippedTests: args?.TestRunStatistics?[TestOutcome.Skipped],
            ExecutedTests: args?.TestRunStatistics?.ExecutedTests,
            ElapsedTimeInRunningTests: args?.ElapsedTimeInRunningTests ?? TimeSpan.Zero
        );
    }
}
