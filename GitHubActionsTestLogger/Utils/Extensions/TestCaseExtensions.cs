using System;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class TestCaseExtensions
{
    public static string GetTypeFullyQualifiedName(this TestCase testCase) =>
        testCase.FullyQualifiedName.SubstringUntilLast(".", StringComparison.OrdinalIgnoreCase);

    public static string GetTypeMinimallyQualifiedName(this TestCase testCase)
    {
        var ns = Path.GetFileNameWithoutExtension(testCase.Source);
        var fullyQualifiedName = testCase.GetTypeFullyQualifiedName();

        return fullyQualifiedName.StartsWith(ns + '.', StringComparison.Ordinal)
            ? fullyQualifiedName[(ns.Length + 1)..]
            : fullyQualifiedName;
    }

    public static string GetMinimallyQualifiedName(this TestCase testCase) =>
        testCase.FullyQualifiedName.SubstringAfterLast(".", StringComparison.OrdinalIgnoreCase);
}