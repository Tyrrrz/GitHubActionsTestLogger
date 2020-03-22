using System;
using System.Collections.Generic;

namespace GitHubActionsTestLogger
{
    internal static class GitHubActions
    {
        private enum LogLevel
        {
            Debug,
            Warning,
            Error
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

        private static void WriteOutput(LogLevel level, string message,
            string? filePath = null, int? line = null, int? column = null)
        {
            var writer = Console.Out;
            var label = level.ToString().ToLowerInvariant();
            var options = FormatOptions(filePath, line, column);

            writer.WriteLine($"::{label} {options}::{message}");
        }

        public static void WriteDebug(string message,
            string? filePath = null, int? line = null, int? column = null) =>
            WriteOutput(LogLevel.Debug, message, filePath, line, column);

        public static void WriteWarning(string message,
            string? filePath = null, int? line = null, int? column = null) =>
            WriteOutput(LogLevel.Warning, message, filePath, line, column);

        public static void WriteError(string message,
            string? filePath = null, int? line = null, int? column = null) =>
            WriteOutput(LogLevel.Error, message, filePath, line, column);
    }
}