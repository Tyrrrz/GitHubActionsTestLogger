using System;
using System.Collections.Generic;

namespace GitHubActionsTestLogger;

// More info: https://help.github.com/en/actions/reference/workflow-commands-for-github-actions
internal static class GitHubWorkflow
{
    public static bool IsRunningOnAgent => string.Equals(
        Environment.GetEnvironmentVariable("GITHUB_ACTIONS"),
        "true",
        StringComparison.OrdinalIgnoreCase
    );

    private static string FormatWorkflowCommand(
        string label,
        string message,
        string options)
    {
        // Escape linebreaks in message to allow multiline output
        // https://github.community/t/set-output-truncates-multiline-strings/16852/3
        var messageEscaped = message
            .Replace("%", "%25")
            .Replace("\n", "%0A")
            .Replace("\r", "%0D");

        return $"::{label} {options}::{messageEscaped}";
    }

    private static string FormatOptions(
        string? filePath = null,
        int? line = null,
        int? column = null,
        string? title = null)
    {
        var options = new List<string>(3);

        if (!string.IsNullOrWhiteSpace(filePath))
            options.Add($"file={filePath}");

        if (line is not null)
            options.Add($"line={line}");

        if (column is not null)
            options.Add($"col={column}");

        if (!string.IsNullOrWhiteSpace(title))
            options.Add($"title={title}");

        return string.Join(",", options);
    }

    public static string FormatError(
        string title,
        string message,
        string? filePath = null,
        int? line = null,
        int? column = null) =>
        FormatWorkflowCommand("error", message, FormatOptions(filePath, line, column, title));

    public static string FormatWarning(
        string title,
        string message,
        string? filePath = null,
        int? line = null,
        int? column = null) =>
        FormatWorkflowCommand("warning", message, FormatOptions(filePath, line, column, title));
}