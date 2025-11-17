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
