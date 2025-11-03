using System;
using System.Linq;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class MtpExtensions
{
    public static string? GetOptionArgumentOrDefault(
        this ICommandLineOptions options,
        string optionName,
        string? defaultValue = null
    )
    {
        if (options.TryGetOptionArgumentList(optionName, out var arguments) && arguments.Length > 0)
        {
            return arguments[0];
        }

        return defaultValue;
    }

    public static bool? GetOptionSwitchValue(this ICommandLineOptions options, string optionName)
    {
        // If set, accept either "true" or "false" or no argument (which implies "true")
        if (options.IsOptionSet(optionName))
            return bool.Parse(options.GetOptionArgumentOrDefault(optionName) ?? "true");

        return null;
    }

    public static string? TryGetTypeFullyQualifiedName(this TestNode test) =>
        test.Properties.SingleOrDefault<TestMethodIdentifierProperty>() is { } method
            ? string.Join(".", method.Namespace, method.TypeName)
            : null;

    public static string? TryGetTypeMinimallyQualifiedName(this TestNode test) =>
        test.Properties.SingleOrDefault<TestMethodIdentifierProperty>() is { } method
            ? method.TypeName
            : null;

    public static string? TryGetFullyQualifiedName(this TestNode test) =>
        test.Properties.SingleOrDefault<TestMethodIdentifierProperty>() is { } method
            ? string.Join(".", method.Namespace, method.TypeName, method.MethodName)
            : null;

    public static string? TryGetMinimallyQualifiedName(this TestNode test) =>
        test.Properties.SingleOrDefault<TestMethodIdentifierProperty>() is { } method
            ? string.Join(".", method.TypeName, method.MethodName)
            : null;

    public static Exception? TryGetException(this TestNodeStateProperty state) =>
        state switch
        {
            FailedTestNodeStateProperty failedState => failedState.Exception,
            ErrorTestNodeStateProperty errorState => errorState.Exception,
            TimeoutTestNodeStateProperty timeoutState => timeoutState.Exception,
            _ => null,
        };

    public static Exception? TryGetException(this TestNode test) =>
        test.Properties.SingleOrDefault<TestNodeStateProperty>()?.TryGetException();

    public static StackFrame? TryGetTestStackFrame(this TestNode test)
    {
        var testMethodFullyQualifiedName = test.TryGetFullyQualifiedName();
        if (string.IsNullOrWhiteSpace(testMethodFullyQualifiedName))
            return null;

        var testMethodName = testMethodFullyQualifiedName.SubstringAfterLast(
            ".",
            StringComparison.OrdinalIgnoreCase
        );

        return test.TryGetException()
            ?.StackTrace?.Pipe(StackFrame.ParseMany)
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

    public static string? TryGetSourceFilePath(this TestNode test) =>
        test.Properties.SingleOrDefault<TestFileLocationProperty>()?.FilePath
        ?? test.TryGetTestStackFrame()?.FilePath;

    public static int? TryGetSourceLine(this TestNode test) =>
        test.Properties.SingleOrDefault<TestFileLocationProperty>()?.LineSpan.Start.Line
        ?? test.TryGetTestStackFrame()?.Line;
}
