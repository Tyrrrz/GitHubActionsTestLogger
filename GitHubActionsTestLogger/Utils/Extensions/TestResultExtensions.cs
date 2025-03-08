using System;
using System.Linq;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class LoggerTestResultExtensions
{
    // This method attempts to get the stack frame that represents the call to the test method.
    // Obviously, this only works if the test throws an exception.
    private static StackFrame? TryGetTestStackFrame(this LoggerTestResult testResult)
    {
        if (string.IsNullOrWhiteSpace(testResult.ErrorStackTrace))
            return null;

        if (string.IsNullOrWhiteSpace(testResult.FullyQualifiedName))
            return null;

        var testMethodFullyQualifiedName = testResult.FullyQualifiedName.SubstringUntil(
            "(",
            StringComparison.OrdinalIgnoreCase
        );

        var testMethodName = testMethodFullyQualifiedName.SubstringAfterLast(
            ".",
            StringComparison.OrdinalIgnoreCase
        );

        return StackFrame
            .ParseMany(testResult.ErrorStackTrace)
            .LastOrDefault(f =>
                // Sync method call
                // e.g. MyTests.EnsureOnePlusOneEqualsTwo()
                f.MethodCall.StartsWith(
                    testMethodFullyQualifiedName,
                    StringComparison.OrdinalIgnoreCase
                )
                ||
                // Async method call
                // e.g. MyTests.<EnsureOnePlusOneEqualsTwo>d__3.MoveNext()
                f.MethodCall.Contains(
                    '<' + testMethodName + '>',
                    StringComparison.OrdinalIgnoreCase
                )
            );
    }

    public static string? TryGetSourceFilePath(this LoggerTestResult testResult)
    {
        // See if it was provided directly (requires source information collection to be enabled)
        if (!string.IsNullOrWhiteSpace(testResult.SourceFilePath))
            return testResult.SourceFilePath;

        // Try to extract it from the stack trace (works only if there was an exception)
        var stackFrame = testResult.TryGetTestStackFrame();
        if (!string.IsNullOrWhiteSpace(stackFrame?.FilePath))
            return stackFrame.FilePath;

        return null;
    }

    public static int? TryGetSourceLine(this LoggerTestResult testResult)
    {
        // See if it was provided directly (requires source information collection to be enabled)
        if (testResult.SourceFileLine > 0)
            return testResult.SourceFileLine;

        // Try to extract it from the stack trace (works only if there was an exception)
        return testResult.TryGetTestStackFrame()?.Line;
    }
}
