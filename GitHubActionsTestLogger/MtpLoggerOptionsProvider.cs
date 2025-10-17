using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitHubActionsTestLogger.GitHub;
using GitHubActionsTestLogger.Reporting;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.CommandLine;

namespace GitHubActionsTestLogger;

internal partial class MtpLoggerOptionsProvider : ICommandLineOptionsProvider
{
    private static readonly CommandLineOption IsEnabledOption = new(
        "github-actions",
        $"Enables the GitHub Actions test reporter. Default is '{GitHubEnvironment.IsRunningInActions}'.",
        ArgumentArity.Zero,
        false
    );

    private static readonly CommandLineOption AnnotationsTitleFormatOption = new(
        "github-actions-annotations-title-format",
        "Specifies the title format for GitHub Annotations. See documentation for available replacement tokens. "
            + $"Default is '{TestReportingOptions.Default.AnnotationTitleFormat}'.",
        ArgumentArity.ExactlyOne,
        false
    );

    private static readonly CommandLineOption AnnotationsMessageFormatOption = new(
        "github-actions-annotations-message-format",
        "Specifies the message format for GitHub Annotations. See documentation for available replacement tokens. "
            + $"Default is '{TestReportingOptions.Default.AnnotationMessageFormat}'.",
        ArgumentArity.ExactlyOne,
        false
    );

    private static readonly CommandLineOption SummaryAllowEmptyOption = new(
        "github-actions-summary-allow-empty",
        "Whether to produce a summary entry for test runs where no tests were executed. "
            + $"Default is '{TestReportingOptions.Default.SummaryAllowEmpty}'.",
        ArgumentArity.ZeroOrOne,
        false
    );

    private static readonly CommandLineOption SummaryIncludePassedTestsOption = new(
        "github-actions-summary-include-passed-tests",
        "Whether to include passed tests (in addition to failed tests) in the GitHub Actions summary. "
            + $"Default is '{TestReportingOptions.Default.SummaryIncludePassedTests}'.",
        ArgumentArity.ZeroOrOne,
        false
    );

    private static readonly CommandLineOption SummaryIncludeSkippedTestsOption = new(
        "github-actions-summary-include-skipped-tests",
        "Whether to include skipped tests (in addition to failed tests) in the GitHub Actions summary. "
            + $"Default is '{TestReportingOptions.Default.SummaryIncludeSkippedTests}'.",
        ArgumentArity.ZeroOrOne,
        false
    );

    public string Uid => "GitHubActionsTestLogger/OptionsProvider";

    public string Version { get; } =
        typeof(MtpLoggerOptionsProvider).Assembly.TryGetVersionString() ?? "1.0.0";

    public string DisplayName => "GitHub Actions Test Logger Options Provider";

    public string Description => "Provides command line options for the GitHub Actions Test Logger";

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public IReadOnlyCollection<CommandLineOption> GetCommandLineOptions() =>
        [
            IsEnabledOption,
            AnnotationsTitleFormatOption,
            AnnotationsMessageFormatOption,
            SummaryAllowEmptyOption,
            SummaryIncludePassedTestsOption,
            SummaryIncludeSkippedTestsOption,
        ];

    // This method is called once after all options are parsed and is used to validate the combination of options.
    public Task<ValidationResult> ValidateCommandLineOptionsAsync(
        ICommandLineOptions commandLineOptions
    )
    {
        if (!commandLineOptions.IsOptionSet(IsEnabledOption.Name))
        {
            foreach (
                var optionName in GetCommandLineOptions()
                    .Select(o => o.Name)
                    .Where(n => !string.Equals(n, IsEnabledOption.Name, StringComparison.Ordinal))
            )
            {
                if (commandLineOptions.IsOptionSet(optionName))
                {
                    return ValidationResult.InvalidTask(
                        $"Option '--{optionName}' can only be used when '--{IsEnabledOption.Name}' is also specified."
                    );
                }
            }
        }

        return ValidationResult.ValidTask;
    }

    // This method is called once per option declared and is used to validate the arguments of the given option.
    // The arity of the option is checked before this method is called.
    public Task<ValidationResult> ValidateOptionArgumentsAsync(
        CommandLineOption commandOption,
        string[] arguments
    ) => ValidationResult.ValidTask;
}

internal partial class MtpLoggerOptionsProvider
{
    public static TestReportingOptions Resolve(
        out bool isEnabled,
        ICommandLineOptions commandLineOptions
    )
    {
        isEnabled = commandLineOptions.IsOptionSet(IsEnabledOption.Name);

        return new TestReportingOptions
        {
            AnnotationTitleFormat =
                commandLineOptions.GetOptionArgumentOrDefault(AnnotationsTitleFormatOption.Name)
                ?? TestReportingOptions.Default.AnnotationTitleFormat,
            AnnotationMessageFormat =
                commandLineOptions.GetOptionArgumentOrDefault(AnnotationsMessageFormatOption.Name)
                ?? TestReportingOptions.Default.AnnotationMessageFormat,
            SummaryAllowEmpty =
                commandLineOptions
                    .GetOptionArgumentOrDefault(SummaryAllowEmptyOption.Name, "true")
                    ?.Pipe(bool.Parse) ?? TestReportingOptions.Default.SummaryAllowEmpty,
            SummaryIncludePassedTests =
                commandLineOptions
                    .GetOptionArgumentOrDefault(SummaryIncludePassedTestsOption.Name, "false")
                    ?.Pipe(bool.Parse) ?? TestReportingOptions.Default.SummaryIncludePassedTests,
            SummaryIncludeSkippedTests =
                commandLineOptions
                    .GetOptionArgumentOrDefault(SummaryIncludeSkippedTestsOption.Name, "true")
                    ?.Pipe(bool.Parse) ?? TestReportingOptions.Default.SummaryIncludeSkippedTests,
        };
    }
}
