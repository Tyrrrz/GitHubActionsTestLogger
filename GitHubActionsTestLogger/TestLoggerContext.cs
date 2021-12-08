using System;
using System.IO;
using System.Linq;
using GitHubActionsTestLogger.Utils;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger;

public class TestLoggerContext
{
    public TextWriter Output { get; }

    public TestLoggerOptions Options { get; }

    public TestLoggerContext(TextWriter output, TestLoggerOptions options)
    {
        Output = output;
        Options = options;
    }

    // We use this method as a last resort if we can't get source information from anywhere else.
    // This will hopefully give us the file path of the project that contains the test,
    // which is not ideal but still better than nothing.
    private static string? TryGetProjectFilePath(string startPath)
    {
        var dirPath = !File.Exists(startPath)
            ? startPath
            : Path.GetDirectoryName(startPath);

        // Recursively ascend up
        while (!string.IsNullOrWhiteSpace(dirPath))
        {
            // *.csproj/*.fsproj/*.vbproj
            var projectFilePath = Directory.EnumerateFiles(dirPath, "*.??proj").FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(projectFilePath))
                return projectFilePath;

            dirPath = Path.GetDirectoryName(dirPath);
        }

        return null;
    }

    // This method attempts to get the stack frame that represents the call to the test method.
    // Obviously, this only works if the test threw an exception.
    private static StackFrame? TryGetTestStackFrame(TestResult testResult)
    {
        if (string.IsNullOrWhiteSpace(testResult.ErrorStackTrace))
            return null;

        var testMethodFullyQualifiedName = testResult.TestCase.FullyQualifiedName.SubstringUntil(
            "(",
            StringComparison.OrdinalIgnoreCase
        );

        var testMethodName = testMethodFullyQualifiedName.SubstringAfterLast(
            ".",
            StringComparison.OrdinalIgnoreCase
        );

        var matchingStackFrames = StackFrame.ParseMany(testResult.ErrorStackTrace)
            .Where(f =>
                // Sync method call
                // e.g. MyTests.EnsureOnePlusOneEqualsTwo()
                f.MethodCall.StartsWith(testMethodFullyQualifiedName, StringComparison.OrdinalIgnoreCase) ||
                // Async method call
                // e.g. MyTests.<EnsureOnePlusOneEqualsTwo>d__3.MoveNext()
                f.MethodCall.Contains('<' + testMethodName + '>', StringComparison.OrdinalIgnoreCase)
            );

        return matchingStackFrames.LastOrDefault();
    }

    private static string? TryGetSourceFilePath(TestResult testResult, StackFrame? stackFrame)
    {
        // See if test runner provided it (never actually happens)
        if (!string.IsNullOrWhiteSpace(testResult.TestCase.CodeFilePath))
            return testResult.TestCase.CodeFilePath;

        // Try to extract it from stack trace (works only if there was an exception)
        if (!string.IsNullOrWhiteSpace(stackFrame?.FilePath))
            return stackFrame.FilePath;

        // Get the project file path instead (not ideal, but the best we can do)
        if (!string.IsNullOrWhiteSpace(testResult.TestCase.Source))
        {
            var projectFilePath = TryGetProjectFilePath(testResult.TestCase.Source);
            if (!string.IsNullOrWhiteSpace(projectFilePath))
                return projectFilePath;
        }

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

        var message = Options.MessageFormat.Apply(testResult);

        if (testResult.Outcome == TestOutcome.Failed)
        {
            Output.WriteLine(GitHubActions.FormatError(message, filePath, line));
        }
        else if (Options.ReportWarnings)
        {
            Output.WriteLine(GitHubActions.FormatWarning(message, filePath, line));
        }
    }
}