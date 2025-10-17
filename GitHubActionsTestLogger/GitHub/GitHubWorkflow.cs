using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitHubActionsTestLogger.Utils;
using GitHubActionsTestLogger.Utils.Extensions;

namespace GitHubActionsTestLogger.GitHub;

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
        // If the summary file already contains HTML content, we need to first add two newlines
        // in order to switch GitHub's parser from HTML mode back to markdown mode.
        // It's safe to do it unconditionally because, if the file is empty, these newlines
        // will simply be ignored.
        // https://github.com/Tyrrrz/GitHubActionsTestLogger/issues/22
        summaryWriter.WriteLine();
        summaryWriter.WriteLine();

        summaryWriter.WriteLine(content);
        summaryWriter.Flush();
    }
}

internal partial class GitHubWorkflow
{
    public static TextWriter DefaultCommandWriter => Console.Out;

    public static TextWriter DefaultSummaryWriter => // Summary is written to the file specified by an environment variable.
        // We may need to write to the summary file from multiple test suites in parallel,
        // so we should use a stream that delays acquiring the file lock until the very last moment,
        // and employs retry logic to handle potential race conditions.
        GitHubEnvironment
            .SummaryFilePath?.Pipe(f => new ContentionTolerantWriteFileStream(f, FileMode.Append))
            .Pipe(s => new StreamWriter(s)) ?? TextWriter.Null;
}
