using System;
using System.Collections.Generic;
using System.IO;
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

    private void WriteCommand(
        string name,
        string title,
        string message,
        string? filePath = null,
        int? line = null,
        int? column = null)
    {
        // URL-encode certain characters to escape them from being processed as command tokens
        // https://pakstech.com/blog/github-actions-workflow-commands
        static string Escape(string value) => value
            .Replace("%", "%25")
            .Replace("\n", "%0A")
            .Replace("\r", "%0D");

        string FormatOptions()
        {
            var options = new List<string>(3);

            if (!string.IsNullOrWhiteSpace(filePath))
                options.Add($"file={Escape(filePath)}");

            if (line is not null)
                options.Add($"line={Escape(line.ToString())}");

            if (column is not null)
                options.Add($"col={Escape(column.ToString())}");

            if (!string.IsNullOrWhiteSpace(title))
                options.Add($"title={Escape(title)}");

            return string.Join(",", options);
        }

        // Command should start at the beginning of the line, so add a newline
        // to make sure there is no preceding text.
        // Preceding text may sometimes appear if the .NET CLI is running with
        // ANSI color codes enabled.
        _commandWriter.WriteLine();

        _commandWriter.WriteLine(
            $"::{name} {FormatOptions()}::{Escape(message)}"
        );

        // This newline is just for symmetry
        _commandWriter.WriteLine();

        _commandWriter.Flush();
    }

    public void ReportError(
        string title,
        string message,
        string? filePath = null,
        int? line = null,
        int? column = null) =>
        WriteCommand("error", title, message, filePath, line, column);

    public void ReportSummary(string content)
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

    public static string? SummaryFilePath { get; } = Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");

    public static string? TryGetFilePermalink(string filePath, int? line)
    {
        var serverUrl = Environment.GetEnvironmentVariable("GITHUB_SERVER_URL");
        var repository = Environment.GetEnvironmentVariable("GITHUB_REPOSITORY");
        var workspaceDirPath = Environment.GetEnvironmentVariable("GITHUB_WORKSPACE");
        var commitHash = Environment.GetEnvironmentVariable("GITHUB_SHA");

        if (string.IsNullOrWhiteSpace(serverUrl) ||
            string.IsNullOrWhiteSpace(repository) ||
            string.IsNullOrWhiteSpace(workspaceDirPath) ||
            string.IsNullOrWhiteSpace(commitHash))
            return null;

        var filePathNormalized = filePath.Substring(0, workspaceDirPath.Length)
            .Pipe(s => s.Replace("\\", "/"))
            .Pipe(Uri.EscapeDataString);

        var lineMarker = line?.Pipe(l => $"#L{l}");

        return $"{serverUrl}/{repository}/blob/{commitHash}/{filePathNormalized}{lineMarker}";
    }
}