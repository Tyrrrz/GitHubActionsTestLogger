using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitHubActionsTestLogger.Internal;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger
{
    // The main idea behind this logger is that it writes messages to console in a special format
    // that GitHub Actions runner recognizes as workflow commands.
    // We try to get the source information (file, line number) of the failed tests and
    // report them to GitHub Actions so it highlights them as failed checks in diff and annotations.

    // The problem is that .NET doesn't provide source information, even though the contract implies
    // that it's supposed to. That's why we're employing some additional workarounds to get it if possible.

    [FriendlyName("GitHubActions")]
    [ExtensionUri("logger://tyrrrz/ghactions/v1")]
    public class TestLogger : ITestLoggerWithParameters
    {
        private bool _reportWarnings = true;

        // This assumes only one project file per project
        private static string? TryGetProjectFilePath(string startPath)
        {
            var dirPath = File.Exists(startPath)
                ? Path.GetDirectoryName(startPath)
                : startPath;

            // Recursive ascend
            while (!string.IsNullOrWhiteSpace(dirPath))
            {
                // *.csproj/*.fsproj/*.vbproj
                var projectFilePath = Directory.EnumerateFiles(dirPath, "*.??proj").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(projectFilePath))
                    return projectFilePath;

                dirPath = Path.GetDirectoryName(dirPath);
            }

            // Give up
            return null;
        }

        private static StackFrame? TryGetTestStackFrame(TestResult testResult)
        {
            // If there's no stack trace, there's nothing we can do
            if (string.IsNullOrWhiteSpace(testResult.ErrorStackTrace))
                return null;

            // Get fully qualified test method name (substring until method parameters)
            var testMethodName = testResult.TestCase.FullyQualifiedName.SubstringUntil("(", StringComparison.OrdinalIgnoreCase);

            // Find the deepest stack frame that contains this method
            return StackFrame.ParseMany(testResult.ErrorStackTrace)
                .LastOrDefault(f => f.MethodCall.StartsWith(testMethodName, StringComparison.OrdinalIgnoreCase));
        }

        private static string? TryGetSourceFilePath(TestResult testResult, StackFrame? stackFrame)
        {
            // See if test runner provided it (never actually happens)
            if (!string.IsNullOrWhiteSpace(testResult.TestCase.CodeFilePath))
                return testResult.TestCase.CodeFilePath;

            // Try to extract it from stack trace (works only if there was an exception)
            if (stackFrame != null && !string.IsNullOrWhiteSpace(stackFrame.FilePath))
                return stackFrame.FilePath;

            // Get the project file path instead (not ideal, but the best we can do)
            if (!string.IsNullOrWhiteSpace(testResult.TestCase.Source))
            {
                var projectFilePath = TryGetProjectFilePath(testResult.TestCase.Source);
                if (!string.IsNullOrWhiteSpace(projectFilePath))
                    return projectFilePath;
            }

            // Give up
            return null;
        }

        private static int? TryGetSourceLine(TestResult testResult, StackFrame? stackFrame)
        {
            // See if test runner provided it (never actually happens)
            if (testResult.TestCase.LineNumber > 0)
                return testResult.TestCase.LineNumber;

            // Try to extract it from stack trace (works only if there was an exception)
            return stackFrame?.Line;
        }

        private void HandleTestResult(TestResult testResult)
        {
            // We only care about tests with negative outcomes
            if (testResult.Outcome <= TestOutcome.Passed)
                return;

            var stackFrame = TryGetTestStackFrame(testResult);
            var filePath = TryGetSourceFilePath(testResult, stackFrame);
            var line = TryGetSourceLine(testResult, stackFrame);

            var message = !string.IsNullOrWhiteSpace(testResult.ErrorMessage)
                ? $"{testResult.TestCase.DisplayName}: {testResult.ErrorMessage}"
                : $"{testResult.TestCase.DisplayName}: {testResult.Outcome}";

            if (testResult.Outcome == TestOutcome.Failed)
                GitHubActions.ReportError(message, filePath, line);
            else if (_reportWarnings)
                GitHubActions.ReportWarning(message, filePath, line);
        }

        public void Initialize(TestLoggerEvents events, string testRunDirectory)
        {
            if (!GitHubActions.IsRunningInsideWorkflow())
                Console.WriteLine("WARN: Not running inside GitHub Actions, but using GitHub Actions Test Logger.");

            events.TestResult += (sender, args) => HandleTestResult(args.Result);
        }

        public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
        {
            _reportWarnings = !string.Equals(
                parameters.GetValueOrDefault("report-warnings"),
                "false",
                StringComparison.OrdinalIgnoreCase
            );

            if (!GitHubActions.IsRunningInsideWorkflow())
                Console.WriteLine("WARN: Not running inside GitHub Actions, but using GitHub Actions Test Logger.");

            events.TestResult += (sender, args) => HandleTestResult(args.Result);
        }
    }
}