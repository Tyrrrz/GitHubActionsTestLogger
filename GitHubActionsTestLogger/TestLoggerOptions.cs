using System.Collections.Generic;
using GitHubActionsTestLogger.Utils.Extensions;

namespace GitHubActionsTestLogger;

public partial class TestLoggerOptions
{
    public TestResultMessageFormat MessageFormat { get; }

    public bool ReportWarnings { get; }

    public TestLoggerOptions(
        TestResultMessageFormat messageFormat,
        bool reportWarnings)
    {
        MessageFormat = messageFormat;
        ReportWarnings = reportWarnings;
    }
}

// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleNamedExpression
public partial class TestLoggerOptions
{
    public static TestLoggerOptions Default { get; } = new(
        messageFormat: TestResultMessageFormat.Default,
        reportWarnings: true
    );

    public static TestLoggerOptions Resolve(IReadOnlyDictionary<string, string> parameters) => new(
        messageFormat:
        parameters.GetValueOrDefault("format")?.Pipe(s => new TestResultMessageFormat(s)) ??
        Default.MessageFormat,

        reportWarnings:
        parameters.GetValueOrDefault("report-warnings")?.TryParseBool() ??
        Default.ReportWarnings
    );
}
// ReSharper restore ArgumentsStyleOther
// ReSharper restore ArgumentsStyleLiteral
// ReSharper restore ArgumentsStyleNamedExpression