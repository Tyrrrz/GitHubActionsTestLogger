using System.Linq;

using Microsoft.Testing.Platform.CommandLine;

using static GitHubActionsTestLogger.CliOptionsProvider;

namespace GitHubActionsTestLogger;

public partial class TestReporterOptions
{
    public string AnnotationTitleFormat { get; init; } = "@test";

    public string AnnotationMessageFormat { get; init; } = "@error";

    public bool SummaryIncludePassedTests { get; init; }

    public bool SummaryIncludeSkippedTests { get; init; }

    public bool SummaryIncludeNotFoundTests { get; init; } = true;
}

public partial class TestReporterOptions
{
    public static TestReporterOptions Default { get; } = new();

    public static TestReporterOptions Resolve(ICommandLineOptions commandLineOptions)
    {
        var annotationTitleFormat = 
            (commandLineOptions.TryGetOptionArgumentList(ReportGitHubTitleOption, out string[]? arguments)
                ? arguments[0]
                : null)
            ?? Default.AnnotationTitleFormat;

        var annotationMessageFormat =
            (commandLineOptions.TryGetOptionArgumentList(ReportGitHubMessageOption, out arguments)
                ? arguments[0]
                : null)
            ?? Default.AnnotationMessageFormat;

        _ = commandLineOptions.TryGetOptionArgumentList(ReportGitHubSummaryOption, out arguments);

        return new()
        {
            AnnotationTitleFormat = annotationTitleFormat,
            AnnotationMessageFormat = annotationMessageFormat,
            SummaryIncludePassedTests =
                arguments?.Contains(ReportGitHubSummaryArguments.IncludePassedTests) == true 
                || Default.SummaryIncludePassedTests,
            SummaryIncludeSkippedTests =
                arguments?.Contains(ReportGitHubSummaryArguments.IncludeSkippedTests) == true
                || Default.SummaryIncludeSkippedTests,
            SummaryIncludeNotFoundTests =
                arguments?.Contains(ReportGitHubSummaryArguments.IncludeNotFoundTests) == true
                || Default.SummaryIncludeNotFoundTests,
        };
    }
}
