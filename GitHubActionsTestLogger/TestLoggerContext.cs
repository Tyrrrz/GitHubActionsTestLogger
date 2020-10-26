using System;
using System.IO;
using System.Linq;
using GitHubActionsTestLogger.Internal;
using GitHubActionsTestLogger.Internal.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger
{
    public class TestLoggerContext
    {
        public TextWriter Output { get; }

        public TestLoggerOptions Options { get; }

        public TestLoggerContext(TextWriter output, TestLoggerOptions options)
        {
            Output = output;
            Options = options;
        }

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

        public void ProcessTestResult(TestResult testResult)
        {
            if (testResult.Outcome <= TestOutcome.Passed)
                return;

            var stackFrame = TryGetTestStackFrame(testResult);
            var filePath = TryGetSourceFilePath(testResult, stackFrame);
            var line = TryGetSourceLine(testResult, stackFrame);

            var message = !string.IsNullOrWhiteSpace(testResult.ErrorMessage)
                ? $"{testResult.TestCase.DisplayName}: {testResult.ErrorMessage}"
                : $"{testResult.TestCase.DisplayName}: {testResult.Outcome}";

            if (testResult.Outcome == TestOutcome.Failed)
                Output.WriteLine(GitHubActions.FormatError(message, filePath, line));
            else if (Options.ReportWarnings)
                Output.WriteLine(GitHubActions.FormatWarning(message, filePath, line));
        }
    }
}