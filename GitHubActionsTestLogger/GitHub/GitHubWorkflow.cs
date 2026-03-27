using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHubActionsTestLogger.Utils;
using GitHubActionsTestLogger.Utils.Extensions;

namespace GitHubActionsTestLogger.GitHub;

// https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions
internal partial class GitHubWorkflow(TextWriter commandWriter, TextWriter summaryWriter)
{
    // GitHub step summary file size limit (1 MiB)
    // https://docs.github.com/en/actions/writing-workflows/choosing-what-your-workflow-does/workflow-commands-for-github-actions#adding-a-job-summary
    private const long SummaryFileSizeLimit = 1024 * 1024;

    private async Task InvokeCommandAsync(
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
        await commandWriter.WriteLineAsync();

        await commandWriter.WriteLineAsync($"::{command} {formattedOptions}::{Escape(message)}");

        // This newline is just for symmetry
        await commandWriter.WriteLineAsync();

        await commandWriter.FlushAsync();
    }

    public async Task CreateErrorAnnotationAsync(
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

        await InvokeCommandAsync("error", message, options);
    }

    public async Task CreateWarningAnnotationAsync(string title, string message)
    {
        await InvokeCommandAsync(
            "warning",
            message,
            new Dictionary<string, string> { ["title"] = title }
        );
    }

    public async Task CreateSummaryAsync(string content)
    {
        // Try to extract the underlying file path from the summary writer to monitor file size.
        // This works when the writer wraps a ContentionTolerantWriteFileStream (production)
        // or a plain FileStream (tests).
        var detectedFilePath = summaryWriter is StreamWriter sw
            ? sw.BaseStream switch
            {
                ContentionTolerantWriteFileStream cts => cts.FilePath,
                FileStream fs => fs.Name,
                _ => null,
            }
            : null;

        if (detectedFilePath != null)
        {
            var existingSize = File.Exists(detectedFilePath)
                ? new FileInfo(detectedFilePath).Length
                : 0L;

            var newlineSize = Encoding.UTF8.GetByteCount(Environment.NewLine);
            var contentSize = Encoding.UTF8.GetByteCount(content);

            // Two leading newlines + content + trailing newline
            var totalToWrite = newlineSize * 3 + contentSize;

            if (existingSize + totalToWrite > SummaryFileSizeLimit)
            {
                var availableBytes = (int)(SummaryFileSizeLimit - existingSize - newlineSize * 3);

                var truncated = TryTruncateSummary(content, availableBytes);

                if (truncated == null)
                {
                    // Can't produce a legible summary — skip writing entirely
                    await CreateWarningAnnotationAsync(
                        "GitHub Actions Test Logger",
                        BuildSummaryOmittedWarning(isSharedFile: existingSize > 0)
                    );
                    return;
                }

                await CreateWarningAnnotationAsync(
                    "GitHub Actions Test Logger",
                    BuildSummaryTruncatedWarning(isSharedFile: existingSize > 0)
                );
                content = truncated;
            }
        }

        // If the summary file already contains HTML content, we need to first add two newlines
        // in order to switch GitHub's parser from HTML mode back to markdown mode.
        // It's safe to do it unconditionally because, if the file is empty, these newlines
        // will simply be ignored.
        // https://github.com/Tyrrrz/GitHubActionsTestLogger/issues/22
        await summaryWriter.WriteLineAsync();
        await summaryWriter.WriteLineAsync();

        await summaryWriter.WriteLineAsync(content);
        await summaryWriter.FlushAsync();
    }

    // Attempt to truncate the rendered summary HTML so that it fits within maxBytes.
    // Returns the truncated content, or null if even the minimal legible content won't fit.
    private static string? TryTruncateSummary(string content, int maxBytes)
    {
        if (maxBytes <= 0)
            return null;

        // Fast path: content already fits
        if (Encoding.UTF8.GetByteCount(content) <= maxBytes)
            return content;

        // HTML pattern that marks the end of a test-group list item in the outer <ul>.
        // Each group closes as: </ul><p></p></li>  (inner results list, margin, group item)
        const string groupItemClose = "</ul><p></p></li>";

        // Suffix to append after a group-item cut to produce valid HTML
        const string groupCutSuffix = "</ul></details>";

        // Minimum legible content ends right after </table>;
        // just close the <details> element to produce a valid (stats-only) summary.
        const string tableEndTag = "</table>";
        const string tableCutSuffix = "</details>";

        var tableEndIndex = content.LastIndexOf(tableEndTag, StringComparison.Ordinal);
        if (tableEndIndex < 0)
            return null;

        var tableContentEnd = tableEndIndex + tableEndTag.Length;

        // Check whether even the minimal (stats-table-only) content fits.
        // Accumulate byte counts incrementally to avoid O(n²) recalculation.
        var tableCutSuffixBytes = Encoding.UTF8.GetByteCount(tableCutSuffix);
        var tableContentBytes = Encoding.UTF8.GetByteCount(content.Substring(0, tableContentEnd));
        if (tableContentBytes + tableCutSuffixBytes > maxBytes)
            return null;

        // Try to include as many complete test groups as possible
        var groupCutSuffixBytes = Encoding.UTF8.GetByteCount(groupCutSuffix);
        var bestCutPoint = tableContentEnd;
        var bestCutSuffix = tableCutSuffix;

        var currentBytes = tableContentBytes;
        var searchFrom = tableContentEnd;
        while (true)
        {
            var groupEnd = content.IndexOf(groupItemClose, searchFrom, StringComparison.Ordinal);
            if (groupEnd < 0)
                break;

            var cutPoint = groupEnd + groupItemClose.Length;

            // Count only the bytes of the new segment (incremental accumulation)
            currentBytes += Encoding.UTF8.GetByteCount(
                content.Substring(searchFrom, cutPoint - searchFrom)
            );

            if (currentBytes + groupCutSuffixBytes > maxBytes)
                break; // This group doesn't fit — stop

            bestCutPoint = cutPoint;
            bestCutSuffix = groupCutSuffix;
            searchFrom = cutPoint;
        }

        return content.Substring(0, bestCutPoint) + bestCutSuffix;
    }

    private static string BuildSummaryTruncatedWarning(bool isSharedFile)
    {
        var message =
            "The test summary was truncated because it exceeded GitHub's step summary size limit (1 MiB). "
            + "To reduce the summary size, consider excluding passed tests by setting "
            + "`summary-include-passed=false` or skipped tests by setting `summary-include-skipped=false`.";

        if (isSharedFile)
            message +=
                " The summary file is shared with other test steps — consider splitting them into separate jobs.";

        return message;
    }

    private static string BuildSummaryOmittedWarning(bool isSharedFile)
    {
        var message =
            "The test summary was omitted because it exceeded GitHub's step summary size limit (1 MiB). "
            + "To reduce the summary size, consider excluding passed tests by setting "
            + "`summary-include-passed=false` or skipped tests by setting `summary-include-skipped=false`.";

        if (isSharedFile)
            message +=
                " The summary file is shared with other test steps — consider splitting them into separate jobs.";

        return message;
    }
}

internal partial class GitHubWorkflow
{
    public static TextWriter DefaultCommandWriter => Console.Out;

    public static TextWriter DefaultSummaryWriter =>
        // Summary is written to the file specified by an environment variable.
        // We may need to write to the summary file from multiple test suites in parallel,
        // so we should use a stream that delays acquiring the file lock until the very last moment,
        // and employs retry logic to handle potential race conditions.
        GitHubEnvironment
            .SummaryFilePath?.Pipe(f => new ContentionTolerantWriteFileStream(f, FileMode.Append))
            .Pipe(s => new StreamWriter(s)) ?? TextWriter.Null;
}
