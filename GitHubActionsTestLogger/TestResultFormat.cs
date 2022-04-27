using System;
using System.Linq;
using System.Text;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GitHubActionsTestLogger;

internal static class TestResultFormat
{
    public const string NewlineToken = "\\n";
    public const string NameToken = "$test";
    public const string TraitsToken = "$traits";
    public const string ErrorMessageToken = "$error";
    public const string ErrorStackTraceToken = "$trace";

    public static string Apply(string template, TestResult testResult)
    {
        var buffer = new StringBuilder(template);

        // New line token
        buffer.Replace(NewlineToken, Environment.NewLine);

        // Name token
        buffer.Replace(NameToken, testResult.TestCase.DisplayName);

        // Traits tokens
        foreach (var trait in testResult.Traits.Union(testResult.TestCase.Traits))
            buffer.Replace($"{TraitsToken}.{trait.Name}", trait.Value);

        // Error message
        buffer.Replace(ErrorMessageToken, testResult.ErrorMessage);

        // Error trace
        buffer.Replace(ErrorStackTraceToken, testResult.ErrorStackTrace);

        return buffer.Trim().ToString();
    }
}