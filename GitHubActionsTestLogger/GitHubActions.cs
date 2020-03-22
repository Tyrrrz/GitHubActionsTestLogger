using System;
using System.Collections.Generic;

namespace GitHubActionsTestLogger
{
    internal enum LogLevel
    {
        Debug,
        Warning,
        Error
    }

    internal static class GitHubActions
    {
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

        public static void WriteOutput(LogLevel level, string message,
            string? filePath = null, int? line = null, int? column = null)
        {
            var writer = Console.Out;
            var label = level.ToString().ToLowerInvariant();
            var options = FormatOptions(filePath, line, column);

            writer.WriteLine($"::{label} {options}::{message}");
        }
    }
}