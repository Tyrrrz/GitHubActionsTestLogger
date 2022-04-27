﻿using System.Collections.Generic;
using System.IO;

namespace GitHubActionsTestLogger;

// https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions
internal class GitHubWorkflow
{
    private readonly TextWriter _writer;

    public GitHubWorkflow(TextWriter writer) => _writer = writer;

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

        // Command should start at the beginning of the line, so add a newline to make
        // sure there is no leading text.
        _writer.WriteLine();

        _writer.WriteLine(
            $"::{name} {FormatOptions()}::{Escape(message)}"
        );

        // This newline is just for symmetry
        _writer.WriteLine();
    }

    public void ReportError(
        string title,
        string message,
        string? filePath = null,
        int? line = null,
        int? column = null) =>
        WriteCommand("error", title, message, filePath, line, column);
}