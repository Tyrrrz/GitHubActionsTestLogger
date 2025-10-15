using System.Collections.Generic;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.Testing.Platform.CommandLine;

namespace GitHubActionsTestLogger.Reporting;

internal partial class TestReportingOptions
{
    public string AnnotationTitleFormat { get; init; } = "@test";

    public string AnnotationMessageFormat { get; init; } = "@error";

    public bool SummaryAllowEmpty { get; init; }

    public bool SummaryIncludePassedTests { get; init; } = true;

    public bool SummaryIncludeSkippedTests { get; init; } = true;
}

internal partial class TestReportingOptions
{
    public static TestReportingOptions Default { get; } = new();
}

internal partial class TestReportingOptions
{
    public static class CommandLineNames
    {
        public const string MtpPrefix = "github-";
        public const string AnnotationsTitleFormat = "annotations-title-format";
        public const string AnnotationsMessageFormat = "annotations-message-format";
        public const string SummaryAllowEmpty = "summary-allow-empty";
        public const string SummaryIncludePassedTests = "summary-include-passed-tests";
        public const string SummaryIncludeSkippedTests = "summary-include-skipped-tests";
    }

    public static TestReportingOptions Resolve(IReadOnlyDictionary<string, string?> parameters) =>
        new()
        {
            AnnotationTitleFormat =
                parameters.GetValueOrDefault(CommandLineNames.AnnotationsTitleFormat)
                ?? Default.AnnotationTitleFormat,
            AnnotationMessageFormat =
                parameters.GetValueOrDefault(CommandLineNames.AnnotationsMessageFormat)
                ?? Default.AnnotationMessageFormat,
            SummaryAllowEmpty =
                parameters.GetValueOrDefault(CommandLineNames.SummaryAllowEmpty)?.Pipe(bool.Parse)
                ?? Default.SummaryAllowEmpty,
            SummaryIncludePassedTests =
                parameters
                    .GetValueOrDefault(CommandLineNames.SummaryIncludePassedTests)
                    ?.Pipe(bool.Parse) ?? Default.SummaryIncludePassedTests,
            SummaryIncludeSkippedTests =
                parameters
                    .GetValueOrDefault(CommandLineNames.SummaryIncludeSkippedTests)
                    ?.Pipe(bool.Parse) ?? Default.SummaryIncludeSkippedTests,
        };

    public static TestReportingOptions Resolve(ICommandLineOptions commandLineOptions) =>
        new()
        {
            AnnotationTitleFormat =
                commandLineOptions.GetOptionArgumentOrDefault(
                    CommandLineNames.MtpPrefix + CommandLineNames.AnnotationsTitleFormat
                ) ?? Default.AnnotationTitleFormat,
            AnnotationMessageFormat =
                commandLineOptions.GetOptionArgumentOrDefault(
                    CommandLineNames.MtpPrefix + CommandLineNames.AnnotationsMessageFormat
                ) ?? Default.AnnotationMessageFormat,
            SummaryAllowEmpty =
                commandLineOptions
                    .GetOptionArgumentOrDefault(
                        CommandLineNames.MtpPrefix + CommandLineNames.SummaryAllowEmpty,
                        "true"
                    )
                    ?.Pipe(bool.Parse) ?? Default.SummaryAllowEmpty,
            SummaryIncludePassedTests =
                commandLineOptions
                    .GetOptionArgumentOrDefault(
                        CommandLineNames.MtpPrefix + CommandLineNames.SummaryIncludePassedTests,
                        "true"
                    )
                    ?.Pipe(bool.Parse) ?? Default.SummaryIncludePassedTests,
            SummaryIncludeSkippedTests =
                commandLineOptions
                    .GetOptionArgumentOrDefault(
                        CommandLineNames.MtpPrefix + CommandLineNames.SummaryIncludeSkippedTests,
                        "true"
                    )
                    ?.Pipe(bool.Parse) ?? Default.SummaryIncludeSkippedTests,
        };
}
