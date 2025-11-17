using System;
using System.Linq;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class MtpExtensions
{
    extension(ICommandLineOptions options)
    {
        public string? GetOptionArgumentOrDefault(string optionName, string? defaultValue = null)
        {
            if (
                options.TryGetOptionArgumentList(optionName, out var arguments)
                && arguments.Length > 0
            )
            {
                return arguments[0];
            }

            return defaultValue;
        }

        public bool? GetOptionSwitchValue(string optionName)
        {
            // If set, accept either "true" or "false" or no argument (which implies "true")
            if (options.IsOptionSet(optionName))
                return bool.Parse(options.GetOptionArgumentOrDefault(optionName) ?? "true");

            return null;
        }
    }

    extension(TestNode test)
    {
        public string? TryGetTypeFullyQualifiedName() =>
            test.Properties.SingleOrDefault<TestMethodIdentifierProperty>() is { } method
                ? string.Join(".", method.Namespace, method.TypeName)
                : null;

        public string? TryGetTypeMinimallyQualifiedName() =>
            test.Properties.SingleOrDefault<TestMethodIdentifierProperty>() is { } method
                ? method.TypeName
                : null;

        public string? TryGetFullyQualifiedName() =>
            test.Properties.SingleOrDefault<TestMethodIdentifierProperty>() is { } method
                ? string.Join(".", method.Namespace, method.TypeName, method.MethodName)
                : null;

        public string? TryGetMinimallyQualifiedName() =>
            test.Properties.SingleOrDefault<TestMethodIdentifierProperty>() is { } method
                ? string.Join(".", method.TypeName, method.MethodName)
                : null;

        public Exception? TryGetException() =>
            test.Properties.SingleOrDefault<TestNodeStateProperty>()?.TryGetException();

        public StackFrame? TryGetTestStackFrame()
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

        public string? TryGetSourceFilePath() =>
            test.Properties.SingleOrDefault<TestFileLocationProperty>()?.FilePath
            ?? test.TryGetTestStackFrame()?.FilePath;

        public int? TryGetSourceLine() =>
            test.Properties.SingleOrDefault<TestFileLocationProperty>()?.LineSpan.Start.Line
            ?? test.TryGetTestStackFrame()?.Line;
    }

    extension(TestNodeStateProperty state)
    {
        public Exception? TryGetException() =>
            state switch
            {
                FailedTestNodeStateProperty failedState => failedState.Exception,
                ErrorTestNodeStateProperty errorState => errorState.Exception,
                TimeoutTestNodeStateProperty timeoutState => timeoutState.Exception,
                _ => null,
            };
    }
}
