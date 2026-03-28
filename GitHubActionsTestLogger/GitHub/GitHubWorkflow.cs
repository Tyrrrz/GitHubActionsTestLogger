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

    private async Task CreateAnnotationAsync(
        GitHubAnnotationKind kind,
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

        await InvokeCommandAsync(kind.ToString().ToLowerInvariant(), message, options);
    }

    public async Task CreateErrorAnnotationAsync(
        string title,
        string message,
        string? filePath = null,
        int? line = null,
        int? column = null
    ) =>
        await CreateAnnotationAsync(
            GitHubAnnotationKind.Error,
            title,
            message,
            filePath,
            line,
            column
        );

    public async Task CreateWarningAnnotationAsync(string title, string message) =>
        await CreateAnnotationAsync(GitHubAnnotationKind.Warning, title, message);

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

        if (!string.IsNullOrWhiteSpace(detectedFilePath))
        {
            var existingSize = File.Exists(detectedFilePath)
                ? new FileInfo(detectedFilePath).Length
                : 0L;

            var newlineSize = Encoding.UTF8.GetByteCount(Environment.NewLine);
            var contentSize = Encoding.UTF8.GetByteCount(content);

            // Two leading newlines + content + trailing newline
            var totalToWrite = newlineSize * 3 + contentSize;

            if (existingSize + totalToWrite > GitHubEnvironment.SummaryFileSizeLimit)
            {
                var availableSizeLong =
                    GitHubEnvironment.SummaryFileSizeLimit - existingSize - newlineSize * 3L;

                string? truncated = null;

                if (availableSizeLong > 0)
                {
                    var availableSize = (int)Math.Min(availableSizeLong, int.MaxValue);
                    var bytes = Encoding.UTF8.GetBytes(content);
                    if (bytes.Length > availableSize)
                    {
                        // Trim back to a valid UTF-8 character boundary
                        var count = availableSize;
                        while (count > 0 && (bytes[count] & 0xC0) == 0x80)
                            count--;

                        if (count > 0)
                            truncated = Encoding.UTF8.GetString(bytes, 0, count);
                    }
                    else
                    {
                        truncated = content;
                    }
                }

                if (string.IsNullOrWhiteSpace(truncated))
                {
                    // Can't produce a legible summary — skip writing entirely
                    await CreateWarningAnnotationAsync(
                        "GitHub Actions Test Logger",
                        "The test summary was omitted because it exceeded GitHub's step summary size limit (1 MiB). "
                            + "To reduce the summary size, consider disabling passed or skipped test reporting."
                            + (
                                existingSize > 0
                                    ? " The summary file is shared with other test steps — consider splitting them into separate jobs."
                                    : ""
                            )
                    );
                    return;
                }

                await CreateWarningAnnotationAsync(
                    "GitHub Actions Test Logger",
                    "The test summary was truncated because it exceeded GitHub's step summary size limit (1 MiB). "
                        + "To reduce the summary size, consider disabling passed or skipped test reporting."
                        + (
                            existingSize > 0
                                ? " The summary file is shared with other test steps — consider splitting them into separate jobs."
                                : ""
                        )
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
