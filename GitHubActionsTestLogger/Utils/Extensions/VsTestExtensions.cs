using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class VsTestExtensions
{
    extension(TestRunCriteria testRunCriteria)
    {
        public string? TryGetTargetFramework()
        {
            if (string.IsNullOrWhiteSpace(testRunCriteria.TestRunSettings))
                return null;

            return (string?)
                XElement
                    .Parse(testRunCriteria.TestRunSettings)
                    .Element("RunConfiguration")
                    ?.Element("TargetFrameworkVersion");
        }
    }

    extension(TestCase testCase)
    {
        public string GetTypeFullyQualifiedName() =>
            testCase
                .FullyQualifiedName
                // Strip the test cases (if this is a parameterized test method)
                .SubstringUntil("(", StringComparison.OrdinalIgnoreCase)
                // Strip everything after the last dot, to leave the full type name
                .SubstringUntilLast(".", StringComparison.OrdinalIgnoreCase);

        public string GetTypeMinimallyQualifiedName()
        {
            var fullyQualifiedName = testCase.GetTypeFullyQualifiedName();

            // We assume that the test assembly name matches the namespace.
            // This is not always true, but it's the best we can do.
            var nameSpace = Path.GetFileNameWithoutExtension(testCase.Source);

            // Strip the namespace from the type name, if it's there
            if (fullyQualifiedName.StartsWith(nameSpace + '.', StringComparison.Ordinal))
                return fullyQualifiedName[(nameSpace.Length + 1)..];

            return fullyQualifiedName;
        }

        public string GetMinimallyQualifiedName()
        {
            var fullyQualifiedName = testCase.GetTypeFullyQualifiedName();

            // Strip the full type name from the test method name, if it's there
            return testCase.FullyQualifiedName.StartsWith(
                fullyQualifiedName,
                StringComparison.Ordinal
            )
                ? testCase.FullyQualifiedName[(fullyQualifiedName.Length + 1)..]
                : testCase.FullyQualifiedName;
        }
    }

    extension(TestResult testResult)
    {
        // This method attempts to get the stack frame that represents the call to the test method.
        // Obviously, this only works if the test throws an exception.
        private StackFrame? TryGetTestStackFrame()
        {
            if (string.IsNullOrWhiteSpace(testResult.ErrorStackTrace))
                return null;

            if (string.IsNullOrWhiteSpace(testResult.TestCase.FullyQualifiedName))
                return null;

            var testMethodFullyQualifiedName =
                testResult.TestCase.FullyQualifiedName.SubstringUntil(
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

        public string? TryGetSourceFilePath()
        {
            // See if it was provided directly (requires source information collection to be enabled)
            if (!string.IsNullOrWhiteSpace(testResult.TestCase.CodeFilePath))
                return testResult.TestCase.CodeFilePath;

            // Try to extract it from the stack trace (works only if there was an exception)
            var stackFrame = testResult.TryGetTestStackFrame();
            if (!string.IsNullOrWhiteSpace(stackFrame?.FilePath))
                return stackFrame.FilePath;

            return null;
        }

        public int? TryGetSourceLine()
        {
            // See if it was provided directly (requires source information collection to be enabled)
            if (testResult.TestCase.LineNumber > 0)
                return testResult.TestCase.LineNumber;

            // Try to extract it from the stack trace (works only if there was an exception)
            return testResult.TryGetTestStackFrame()?.Line;
        }
    }
}
