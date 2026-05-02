using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GitHubActionsTestLogger.Utils;
using PowerKit.Extensions;

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

    public async Task CreateWarningAnnotationAsync(string title, string message) =>
        await CreateAnnotationAsync(GitHubAnnotationKind.Warning, title, message);

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

    private string TruncateSummary(string content)
    {
        // Try to extract the underlying file path from the summary writer to monitor file size
        var filePath = summaryWriter is StreamWriter writer
            ? writer.BaseStream switch
            {
                ContentionTolerantWriteFileStream cts => cts.FilePath,
                FileStream fs => fs.Name,
                _ => null,
            }
            : null;

        if (string.IsNullOrWhiteSpace(filePath))
            return content;

        var existingSize = File.Exists(filePath) ? new FileInfo(filePath).Length : 0L;

        // Calculate required size for the summary content
        var contentSize = Encoding.UTF8.GetByteCount(content);
        var newLineSize = Encoding.UTF8.GetByteCount(Environment.NewLine);
        var requiredSize = contentSize + newLineSize * 3;

        if (existingSize + requiredSize > GitHubEnvironment.SummaryFileSizeLimit)
        {
            var availableSize = (int)
                Math.Min(
                    GitHubEnvironment.SummaryFileSizeLimit - existingSize - newLineSize * 3L,
                    int.MaxValue
                );

            return
                // There is enough space to fit the whole content
                availableSize > 0
                && requiredSize <= availableSize
                    ? content
                // There is enough space to fit some of the content
                : availableSize > 0 && requiredSize > availableSize
                    ? content.TruncateBytes(availableSize, Encoding.UTF8)
                // There is no space at all
                : string.Empty;
        }

        return content;
    }

    public async Task CreateSummaryAsync(string content)
    {
        // Truncate summary to fit into GitHub's step summary size limit
        var truncated = TruncateSummary(content);
        if (truncated.Length < content.Length)
        {
            await CreateWarningAnnotationAsync(
                "Test summary truncated",
                """
                The test summary was truncated or completely omitted because it exceeded GitHub's size limit of 1 MiB.

                To reduce the summary size, consider disabling reporting of passed and skipped tests, if enabled.
                If you have multiple summary providers in the same step (e.g. running multiple test suites), consider splitting them into separate steps to avoid sharing the same summary output.
                """
            );
        }

        // If the summary file already contains HTML content, we need to first add two newlines
        // in order to switch GitHub's parser from HTML mode back to markdown mode.
        // It's safe to do it unconditionally because, if the file is empty, these newlines
        // will simply be ignored.
        // https://github.com/Tyrrrz/GitHubActionsTestLogger/issues/22
        await summaryWriter.WriteLineAsync();
        await summaryWriter.WriteLineAsync();

        await summaryWriter.WriteLineAsync(truncated);
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
            .Pipe(s => new StreamWriter(s))
        ?? TextWriter.Null;
}
