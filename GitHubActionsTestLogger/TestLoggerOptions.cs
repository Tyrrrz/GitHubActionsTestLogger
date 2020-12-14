using System;
using System.Collections.Generic;

namespace GitHubActionsTestLogger
{
    public partial class TestLoggerOptions
    {
        public bool ReportWarnings { get; }

        public TestLoggerOptions(bool reportWarnings)
        {
            ReportWarnings = reportWarnings;
        }
    }

    public partial class TestLoggerOptions
    {
        public static TestLoggerOptions Default { get; } = new(true);

        public static TestLoggerOptions Extract(IReadOnlyDictionary<string, string> parameters)
        {
            var reportWarnings = !string.Equals(
                parameters.GetValueOrDefault("report-warnings"),
                "false",
                StringComparison.OrdinalIgnoreCase
            );

            return new TestLoggerOptions(
                reportWarnings
            );
        }
    }
}