using System;

namespace GitHubActionsTestLogger;

internal static class GitHubEnvironment
{
    public static bool IsActions => string.Equals(
        Environment.GetEnvironmentVariable("GITHUB_ACTIONS"),
        "true",
        StringComparison.OrdinalIgnoreCase
    );

    public static string? ActionSummaryFilePath =>
        Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
}