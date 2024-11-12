using System.Collections.Generic;
using GitHubActionsTestLogger.Utils.Extensions;

namespace GitHubActionsTestLogger;

public partial class TestLoggerOptions
{
    public string AnnotationTitleFormat { get; init; } = "@test";

    public string AnnotationMessageFormat { get; init; } = "@error";

    public bool SummaryIncludePassedTests { get; init; }

    public bool SummaryIncludeSkippedTests { get; init; }

    public bool SummaryIncludeNotFoundTests { get; init; } = true;
}

public partial class TestLoggerOptions
{
    public static TestLoggerOptions Default { get; } = new();

    public static TestLoggerOptions Resolve(IReadOnlyDictionary<string, string?> parameters) =>
        new()
        {
            AnnotationTitleFormat =
                parameters.GetValueOrDefault("annotations.titleFormat")
                ?? Default.AnnotationTitleFormat,
            AnnotationMessageFormat =
                parameters.GetValueOrDefault("annotations.messageFormat")
                ?? Default.AnnotationMessageFormat,
            SummaryIncludePassedTests =
                parameters.GetValueOrDefault("summary.includePassedTests")?.Pipe(bool.Parse)
                ?? Default.SummaryIncludePassedTests,
            SummaryIncludeSkippedTests =
                parameters.GetValueOrDefault("summary.includeSkippedTests")?.Pipe(bool.Parse)
                ?? Default.SummaryIncludeSkippedTests,
            SummaryIncludeNotFoundTests =
                parameters.GetValueOrDefault("summary.includeNotFoundTests")?.Pipe(bool.Parse)
                ?? Default.SummaryIncludeNotFoundTests,
        };
}
