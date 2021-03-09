using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHubActionsTestLogger
{
    // More info: https://help.github.com/en/actions/reference/workflow-commands-for-github-actions

    internal static class GitHubActions
    {
        public static bool IsRunningInsideWorkflow() => string.Equals(
            Environment.GetEnvironmentVariable("GITHUB_ACTIONS"),
            "true",
            StringComparison.OrdinalIgnoreCase
        );

        private static string FormatWorkflowCommand(
            string label,
            string content,
            string options)
        {
            // Commands can't have line breaks so trim the content to one line to avoid polluting the console
            var trimmedContent = content
                .Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()?
                .Trim();

            return $"::{label} {options}::{trimmedContent}";
        }

        private static string FormatOptions(
            string? filePath = null,
            int? line = null,
            int? column = null)
        {
            var options = new List<string>(3);

            if (!string.IsNullOrWhiteSpace(filePath))
                options.Add($"file={filePath}");

            if (line is not null)
                options.Add($"line={line}");

            if (column is not null)
                options.Add($"col={column}");

            return string.Join(",", options);
        }

        public static string FormatError(
            string message,
            string? filePath = null,
            int? line = null,
            int? column = null) =>
            FormatWorkflowCommand("error", message, FormatOptions(filePath, line, column));

        public static string FormatWarning(
            string message,
            string? filePath = null,
            int? line = null,
            int? column = null) =>
            FormatWorkflowCommand("warning", message, FormatOptions(filePath, line, column));
    }
}