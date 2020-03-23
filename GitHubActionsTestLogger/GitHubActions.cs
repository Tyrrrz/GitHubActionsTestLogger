using System;
using System.Collections.Generic;
using System.Linq;

namespace GitHubActionsTestLogger
{
    // More info: https://help.github.com/en/actions/reference/workflow-commands-for-github-actions

    internal static class GitHubActions
    {
        private static void WriteWorkflowCommand(string label, string content, string options)
        {
            // Commands can't have line breaks so trim the content to one line to avoid polluting the console
            var trimmedContent = content
                .Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault()
                ?.Trim();

            Console.WriteLine($"::{label} {options}::{trimmedContent}");
        }

        private static string FormatOptions(string? filePath = null, int? line = null, int? column = null)
        {
            var options = new List<string>(3);

            if (!string.IsNullOrWhiteSpace(filePath))
                options.Add($"file={filePath}");

            if (line != null)
                options.Add($"line={line}");

            if (column != null)
                options.Add($"col={column}");

            return string.Join(",", options);
        }

        public static void ReportError(string message, string? filePath = null, int? line = null, int? column = null) =>
            WriteWorkflowCommand("error", message, FormatOptions(filePath, line, column));

        public static void ReportWarning(string message, string? filePath = null, int? line = null, int? column = null) =>
            WriteWorkflowCommand("warning", message, FormatOptions(filePath, line, column));
    }
}