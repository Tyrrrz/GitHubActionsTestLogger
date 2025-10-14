using System.Collections.Generic;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.Testing.Platform.CommandLine;
using static GitHubActionsTestLogger.TestReporterOptionsProvider;

namespace GitHubActionsTestLogger;

internal partial class TestReporterOptions
{
    public string AnnotationTitleFormat { get; init; } = "@test";

    public string AnnotationMessageFormat { get; init; } = "@error";

    public bool SummaryIncludePassedTests { get; init; } = true;

    public bool SummaryIncludeSkippedTests { get; init; } = true;

    public bool SummaryIncludeNotFoundTests { get; init; }
}

internal partial class TestReporterOptions
{
    public static TestReporterOptions Default { get; } = new();

    public static TestReporterOptions Resolve(IReadOnlyDictionary<string, string?> parameters) =>
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

    public static TestReporterOptions Resolve(ICommandLineOptions commandLineOptions)
    {
        var annotationTitleFormat =
            commandLineOptions.TryGetOptionArgument(ReportGitHubTitleOption)
            ?? Default.AnnotationTitleFormat;

        var annotationMessageFormat =
            commandLineOptions.TryGetOptionArgument(ReportGitHubMessageOption)
            ?? Default.AnnotationMessageFormat;

        // TODO: wire these
        var summaryIncludePassedTests = Default.SummaryIncludePassedTests;
        var summaryIncludeSkippedTests = Default.SummaryIncludeSkippedTests;
        var summaryIncludeNotFoundTests = Default.SummaryIncludeNotFoundTests;

        return new TestReporterOptions
        {
            AnnotationTitleFormat = annotationTitleFormat,
            AnnotationMessageFormat = annotationMessageFormat,
            SummaryIncludePassedTests = summaryIncludePassedTests,
            SummaryIncludeSkippedTests = summaryIncludeSkippedTests,
            SummaryIncludeNotFoundTests = summaryIncludeNotFoundTests,
        };
    }
}
