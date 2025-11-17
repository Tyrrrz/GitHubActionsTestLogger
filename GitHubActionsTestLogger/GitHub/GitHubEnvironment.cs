using System;
using System.IO;
using GitHubActionsTestLogger.Utils.Extensions;

namespace GitHubActionsTestLogger.GitHub;

internal static class GitHubEnvironment
{
    public static bool IsRunningInActions { get; } =
        string.Equals(
            Environment.GetEnvironmentVariable("GITHUB_ACTIONS"),
            "true",
            StringComparison.OrdinalIgnoreCase
        );

    public static string? ServerUrl { get; } =
        Environment.GetEnvironmentVariable("GITHUB_SERVER_URL");

    public static string? RepositorySlug { get; } =
        Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");

    public static string? WorkspacePath { get; } =
        Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");

    public static string? CommitHash { get; } = Environment.GetEnvironmentVariable("GITHUB_SHA");

    public static string? SummaryFilePath { get; } =
        Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");

    public static string? TryGenerateFilePermalink(string filePath, int? line = null)
    {
        if (
            string.IsNullOrWhiteSpace(ServerUrl)
            || string.IsNullOrWhiteSpace(RepositorySlug)
            || string.IsNullOrWhiteSpace(WorkspacePath)
            || string.IsNullOrWhiteSpace(CommitHash)
        )
        {
            return null;
        }

        var filePathRelative =
            // If the file path starts with /_/ but the workspace path doesn't,
            // then it's safe to assume that the file path has already been normalized
            // by the Deterministic Build feature of MSBuild.
            // In this case, we only need to remove the leading /_/ from the file path
            // to get the correct relative path.
            filePath.StartsWith("/_/", StringComparison.Ordinal)
            && !WorkspacePath.StartsWith("/_/", StringComparison.Ordinal)
                ? filePath[3..]
                : Path.GetRelativePath(WorkspacePath, filePath);

        var filePathRoute = filePathRelative.Replace('\\', '/').Trim('/');
        var lineMarker = line?.Pipe(l => $"#L{l}");

        return $"{ServerUrl}/{RepositorySlug}/blob/{CommitHash}/{filePathRoute}{lineMarker}";
    }
}
