using System;
using System.IO;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger.Utils.Extensions;

internal static class TestCaseExtensions
{
    public static string GetTypeFullyQualifiedName(this TestCase testCase) =>
        testCase.FullyQualifiedName.SubstringUntilLast(".", StringComparison.OrdinalIgnoreCase);

    public static string GetTypeMinimallyQualifiedName(this TestCase testCase) =>
        testCase.GetTypeFullyQualifiedName().SubstringAfter(
            Path.GetFileNameWithoutExtension(testCase.Source) + '.'
        );

    public static string GetMinimallyQualifiedName(this TestCase testCase) =>
        testCase.FullyQualifiedName.SubstringAfterLast(".", StringComparison.OrdinalIgnoreCase);
}