using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitHubActionsTestLogger.Utils;
using GitHubActionsTestLogger.Utils.Extensions;

namespace GitHubActionsTestLogger;

// https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions
internal partial class GitHubWorkflow(TextWriter commandWriter, TextWriter summaryWriter)
{
    private void InvokeCommand(
        string command,
        string message,
        IReadOnlyDictionary<string, string>? options = null
    )
    {
        // URL-encode certain characters to ensure they don't get parsed as command tokens
        // https://pakstech.com/blog/github-actions-workflow-commands
        static string Escape(string value) =>
            value
                .Replace("%", "%25", StringComparison.Ordinal)
                .Replace("\n", "%0A", StringComparison.Ordinal)
                .Replace("\r", "%0D", StringComparison.Ordinal);

        var formattedOptions = options
            ?.Select(kvp => Escape(kvp.Key) + '=' + Escape(kvp.Value))
            .Pipe(s => string.Join(",", s));

        // Command should start at the beginning of the line, so add a newline
        // to make sure there is no preceding text.
        // Preceding text may sometimes appear if the .NET CLI is running with
        // ANSI color codes enabled.
        commandWriter.WriteLine();

        commandWriter.WriteLine($"::{command} {formattedOptions}::{Escape(message)}");

        // This newline is just for symmetry
        commandWriter.WriteLine();

        commandWriter.Flush();
    }

    public void CreateErrorAnnotation(
        string title,
        string message,
        string? filePath = null,
        int? line = null,
        int? column = null
    )
    {
        var options = new Dictionary<string, string> { ["title"] = title };

        if (!string.IsNullOrWhiteSpace(filePath))
            options["file"] = filePath;

        if (line is not null)
            options["line"] = line.Value.ToString();

        if (column is not null)
            options["col"] = column.Value.ToString();

        InvokeCommand("error", message, options);
    }

    public void CreateSummary(string content)
    {
        // Other steps may have reported summaries that contain HTML tags,
        // which can screw up markdown parsing, so we need to make sure
        // there's at least two newlines before our summary to be safe.
        // https://github.com/Tyrrrz/GitHubActionsTestLogger/issues/22
        summaryWriter.WriteLine();
        summaryWriter.WriteLine();

        summaryWriter.WriteLine(content);
        summaryWriter.Flush();
    }
}

internal partial class GitHubWorkflow
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
                : PathEx.GetRelativePath(WorkspacePath, filePath);

        var filePathRoute = filePathRelative.Replace('\\', '/').Trim('/');
        var lineMarker = line?.Pipe(l => $"#L{l}");

        return $"{ServerUrl}/{RepositorySlug}/blob/{CommitHash}/{filePathRoute}{lineMarker}";
    }

    public static GitHubWorkflow Default { get; } =
        new(
            // Commands are written to the standard output
            Console.Out,
            // Summary is written to the file specified by an environment variable.
            // We may need to write to the summary file from multiple test suites in parallel,
            // so we should use a stream that delays acquiring the file lock until the very last moment,
            // and employs retry logic to handle potential race conditions.
            SummaryFilePath
                ?.Pipe(f => new ContentionTolerantWriteFileStream(f, FileMode.Append))
                .Pipe(s => new StreamWriter(s)) ?? TextWriter.Null
        );
}
