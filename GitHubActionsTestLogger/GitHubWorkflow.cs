using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitHubActionsTestLogger.Utils;
using GitHubActionsTestLogger.Utils.Extensions;

namespace GitHubActionsTestLogger;

// https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions
public partial class GitHubWorkflow
{
    private readonly TextWriter _commandWriter;
    private readonly TextWriter _summaryWriter;

    public GitHubWorkflow(TextWriter commandWriter, TextWriter summaryWriter)
    {
        _commandWriter = commandWriter;
        _summaryWriter = summaryWriter;
    }

    private void InvokeCommand(
        string command,
        string message,
        IReadOnlyDictionary<string, string>? options = null)
    {
        // URL-encode certain characters to ensure they don't get parsed as command tokens
        // https://pakstech.com/blog/github-actions-workflow-commands
        static string Escape(string value) => value
            .Replace("%", "%25")
            .Replace("\n", "%0A")
            .Replace("\r", "%0D");

        var formattedOptions = options?
            .Select(kvp => Escape(kvp.Key) + '=' + Escape(kvp.Value))
            .Pipe(s => string.Join(",", s));

        // Command should start at the beginning of the line, so add a newline
        // to make sure there is no preceding text.
        // Preceding text may sometimes appear if the .NET CLI is running with
        // ANSI color codes enabled.
        _commandWriter.WriteLine();

        _commandWriter.WriteLine(
            $"::{command} {formattedOptions}::{Escape(message)}"
        );

        // This newline is just for symmetry
        _commandWriter.WriteLine();

        _commandWriter.Flush();
    }

    public void CreateErrorAnnotation(
        string title,
        string message,
        string? filePath = null,
        int? line = null,
        int? column = null)
    {
        var options = new Dictionary<string, string>
        {
            ["title"] = title
        };

        if (!string.IsNullOrWhiteSpace(filePath))
            options["file"] = filePath;

        if (line is not null)
            options["line"] = line.ToString();

        if (column is not null)
            options["col"] = column.ToString();

        InvokeCommand("error", message, options);
    }

    public void CreateSummary(string content)
    {
        _summaryWriter.WriteLine(content);
        _summaryWriter.Flush();
    }
}

public partial class GitHubWorkflow
{
    public static bool IsRunningOnAgent { get; } = string.Equals(
        Environment.GetEnvironmentVariable("GITHUB_ACTIONS"),
        "true",
        StringComparison.OrdinalIgnoreCase
    );

    public static string? SummaryFilePath { get; } =
        Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");

    public static string? TryGenerateFilePermalink(string filePath, int? line = null)
    {
        var serverUrl = Environment.GetEnvironmentVariable("GITHUB_SERVER_URL");
        var repositorySlug = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
        var workspacePath = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");
        var commitHash = Environment.GetEnvironmentVariable("GITHUB_SHA");

        if (string.IsNullOrWhiteSpace(serverUrl) ||
            string.IsNullOrWhiteSpace(repositorySlug) ||
            string.IsNullOrWhiteSpace(workspacePath) ||
            string.IsNullOrWhiteSpace(commitHash))
            return null;

        var filePathRelative =
            // If the file path starts with /_/ but the workspace path doesn't,
            // then it's safe to assume that the file path has already been normalized
            // by the deterministic build feature.
            // In this case, we only need to remove the leading /_/ from the file path
            // to get the correct relative path.
            filePath.StartsWith("/_/", StringComparison.Ordinal) &&
            !workspacePath.StartsWith("/_/", StringComparison.Ordinal)
                ? filePath[3..]
                : PathEx.GetRelativePath(workspacePath, filePath);

        var filePathRoute = filePathRelative.Replace('\\', '/').Trim('/');
        var lineMarker = line?.Pipe(l => $"#L{l}");

        return $"{serverUrl}/{repositorySlug}/blob/{commitHash}/{filePathRoute}{lineMarker}";
    }
}