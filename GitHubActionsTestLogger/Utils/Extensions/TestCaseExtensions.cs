using System;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class TestCaseExtensions
{
    public static string GetTypeFullyQualifiedName(this TestCase testCase) =>
        testCase.FullyQualifiedName
            // Strip the test cases (if this is a parameterized test method)
            .SubstringUntil("(", StringComparison.OrdinalIgnoreCase)
            // Strip everything after the last dot, to leave the full type name
            .SubstringUntilLast(".", StringComparison.OrdinalIgnoreCase);

    public static string GetTypeMinimallyQualifiedName(this TestCase testCase)
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

    public static string GetMinimallyQualifiedName(this TestCase testCase)
    {
        var fullyQualifiedName = testCase.GetTypeFullyQualifiedName();

        // Strip the full type name from the test method name, if it's there
        return testCase.FullyQualifiedName.StartsWith(fullyQualifiedName, StringComparison.Ordinal)
            ? testCase.FullyQualifiedName[(fullyQualifiedName.Length + 1)..]
            : testCase.FullyQualifiedName;
    }
}
