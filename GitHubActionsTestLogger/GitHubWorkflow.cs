using System.Collections.Generic;
using System.IO;

namespace GitHubActionsTestLogger;

// Commands: https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions
// Summary: https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions#adding-a-markdown-summary
internal class GitHubWorkflow
{
    private readonly TextWriter _output;
    private readonly string? _summaryFilePath;

    public GitHubWorkflow(TextWriter output, string? summaryFilePath)
    {
        _output = output;
        _summaryFilePath = summaryFilePath;
    }

    private void WriteCommand(
        string name,
        string title,
        string message,
        string? filePath = null,
        int? line = null,
        int? column = null)
    {
        static string Escape(string value) => value
            // URL-encode certain characters to escape them from being processed as command tokens
            // https://pakstech.com/blog/github-actions-workflow-commands
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
        _output.WriteLine();

        _output.WriteLine(
            $"::{name} {FormatOptions()}::{Escape(message)}"
        );
    }

    public void ReportError(
        string title,
        string message,
        string? filePath = null,
        int? line = null,
        int? column = null) =>
        WriteCommand("error", title, message, filePath, line, column);

    public void ReportWarning(
        string title,
        string message,
        string? filePath = null,
        int? line = null,
        int? column = null) =>
        WriteCommand("warning", title, message, filePath, line, column);

    public void ReportSummary(string content)
    {
        if (string.IsNullOrWhiteSpace(_summaryFilePath))
            return;

        // There can be multiple test runs in a single step, so make sure to preserve
        // previous summaries as well.
        File.AppendAllText(_summaryFilePath, content);
    }
}