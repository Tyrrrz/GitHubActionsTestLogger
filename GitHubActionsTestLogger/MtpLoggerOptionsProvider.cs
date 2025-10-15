using System.Collections.Generic;
using System.Threading.Tasks;
using GitHubActionsTestLogger.Reporting;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.CommandLine;

namespace GitHubActionsTestLogger;

internal class MtpLoggerOptionsProvider : MtpExtensionBase, ICommandLineOptionsProvider
{
    public IReadOnlyCollection<CommandLineOption> GetCommandLineOptions() =>
        [
            new(
                TestReportingOptions.CommandLineNames.AnnotationsTitleFormat,
                $"Specifies the title format for GitHub Annotations. See documentation for available replacement tokens. "
                    + $"Default is '{TestReportingOptions.Default.AnnotationTitleFormat}'.",
                ArgumentArity.ExactlyOne,
                false
            ),
            new(
                TestReportingOptions.CommandLineNames.AnnotationsMessageFormat,
                $"Specifies the message format for GitHub Annotations. See documentation for available replacement tokens. "
                    + $"Default is '{TestReportingOptions.Default.AnnotationMessageFormat}'.",
                ArgumentArity.ExactlyOne,
                false
            ),
            new(
                TestReportingOptions.CommandLineNames.SummaryIncludePassedTests,
                $"Whether to include passed tests (in addition to failed tests) in the GitHub Actions summary. "
                    + $"Default is '{TestReportingOptions.Default.SummaryIncludePassedTests}'.",
                ArgumentArity.ZeroOrOne,
                false
            ),
            new(
                TestReportingOptions.CommandLineNames.SummaryIncludeSkippedTests,
                $"Whether to include skipped tests (in addition to failed tests) in the GitHub Actions summary. "
                    + $"Default is '{TestReportingOptions.Default.SummaryIncludeSkippedTests}'.",
                ArgumentArity.ZeroOrOne,
                false
            ),
            new(
                TestReportingOptions.CommandLineNames.SummaryAllowEmpty,
                $"Whether to produce a summary entry for test runs where no tests were executed. "
                    + $"Default is '{TestReportingOptions.Default.SummaryAllowEmpty}'.",
                ArgumentArity.ZeroOrOne,
                false
            ),
        ];

    // This method is called once after all options are parsed and is used to validate the combination of options.
    public Task<ValidationResult> ValidateCommandLineOptionsAsync(
        ICommandLineOptions commandLineOptions
    ) => ValidationResult.ValidTask;

    // This method is called once per option declared and is used to validate the arguments of the given option.
    // The arity of the option is checked before this method is called.
    public Task<ValidationResult> ValidateOptionArgumentsAsync(
        CommandLineOption commandOption,
        string[] arguments
    ) => ValidationResult.ValidTask;
}
