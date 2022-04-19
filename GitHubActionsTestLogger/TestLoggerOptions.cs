using System;
using System.Collections.Generic;
using GitHubActionsTestLogger.Utils.Extensions;

namespace GitHubActionsTestLogger;

public partial class TestLoggerOptions
{
    public TestResultMessageFormat MessageFormat { get; }

    public bool ReportWarnings { get; }

    public string? SummaryFilePath { get; }

    public TestLoggerOptions(
        TestResultMessageFormat messageFormat,
        bool reportWarnings,
        string? summaryFilePath)
    {
        MessageFormat = messageFormat;
        ReportWarnings = reportWarnings;
        SummaryFilePath = summaryFilePath;
    }
}

// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleNamedExpression
public partial class TestLoggerOptions
{
    public static TestLoggerOptions Default { get; } = new(
        messageFormat: TestResultMessageFormat.Default,
        reportWarnings: true,
        summaryFilePath: Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY")
    );

    public static TestLoggerOptions Resolve(IReadOnlyDictionary<string, string> parameters) => new(
        messageFormat:
            parameters.GetValueOrDefault("format")?.Pipe(s => new TestResultMessageFormat(s)) ??
            Default.MessageFormat,

        reportWarnings:
            parameters.GetValueOrDefault("report-warnings")?.TryParseBool() ??
            Default.ReportWarnings,

        summaryFilePath:
            parameters.GetValueOrDefault("summary-file.path") ??
            Default.SummaryFilePath
    );
}
// ReSharper restore ArgumentsStyleOther
// ReSharper restore ArgumentsStyleLiteral
// ReSharper restore ArgumentsStyleNamedExpression