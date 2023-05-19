using System.Collections.Generic;
using GitHubActionsTestLogger.Utils.Extensions;

namespace GitHubActionsTestLogger;

public partial class TestLoggerOptions
{
    public string AnnotationTitleFormat { get; init; } = "$test";

    public string AnnotationMessageFormat { get; init; } = "$error";

    public bool SummaryCompactLayout { get; init; }

    public bool SummaryIncludePassedTests { get; init; }

    public bool SummaryIncludeSkippedTests { get; init; }
}

public partial class TestLoggerOptions
{
    public static TestLoggerOptions Default { get; } = new();

    public static TestLoggerOptions Resolve(IReadOnlyDictionary<string, string?> parameters) => new()
    {
        AnnotationTitleFormat =
            parameters.GetValueOrDefault("annotations.titleFormat") ??
            Default.AnnotationTitleFormat,

        AnnotationMessageFormat =
            parameters.GetValueOrDefault("annotations.messageFormat") ??
            Default.AnnotationMessageFormat,

        SummaryCompactLayout =
            parameters.GetValueOrDefault("summary.compactLayout")?.Pipe(bool.Parse) ??
            Default.SummaryCompactLayout,

        SummaryIncludePassedTests =
            parameters.GetValueOrDefault("summary.includePassedTests")?.Pipe(bool.Parse) ??
            Default.SummaryIncludePassedTests,

        SummaryIncludeSkippedTests =
            parameters.GetValueOrDefault("summary.includeSkippedTests")?.Pipe(bool.Parse) ??
            Default.SummaryIncludeSkippedTests,
    };
}