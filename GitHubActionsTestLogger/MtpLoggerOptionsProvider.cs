using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.CommandLine;

namespace GitHubActionsTestLogger;

internal class MtpLoggerOptionsProvider(MtpLoggerExtension extension) : ICommandLineOptionsProvider
{
    public const string ReportGitHubOption = "report-github";
    public const string ReportGitHubSummaryOption = "report-github-summary";
    public const string ReportGitHubTitleOption = "report-github-title";
    public const string ReportGitHubMessageOption = "report-github-message";

    public static class ReportGitHubSummaryArguments
    {
        public const string IncludePassedTests = "includePassedTests";
        public const string IncludeSkippedTests = "includeSkippedTests";
        public const string IncludeNotFoundTests = "includeNotFoundTests";
    }

    public string Uid => extension.Uid;
    public string Version => extension.Version;
    public string DisplayName => extension.DisplayName;
    public string Description => extension.Description;

    public Task<bool> IsEnabledAsync() => extension.IsEnabledAsync();

    public IReadOnlyCollection<CommandLineOption> GetCommandLineOptions() =>
        [
            new(
                ReportGitHubOption,
                "Reports test run information to GitHub Actions",
                ArgumentArity.Zero,
                isHidden: false
            ),
            // TODO: Do you prefer multiple zero argument options or a single option with argument?
            new(
                ReportGitHubSummaryOption,
                "Defines the information to include in the summary",
                ArgumentArity.OneOrMore,
                isHidden: false
            ),
            new(
                ReportGitHubTitleOption,
                "Defines the annotation title format used for reporting test failures",
                ArgumentArity.ExactlyOne,
                isHidden: false
            ),
            new(
                ReportGitHubMessageOption,
                "Defines the annotation message format used for reporting test failures",
                ArgumentArity.ExactlyOne,
                isHidden: false
            ),
        ];

    // This method is called once after all options are parsed and is used to validate the combination of options.
    public Task<ValidationResult> ValidateCommandLineOptionsAsync(
        ICommandLineOptions commandLineOptions
    )
    {
        // We just want to validate that the sub options are set only if the main option is set.
        if (commandLineOptions.IsOptionSet(ReportGitHubOption))
        {
            return ValidationResult.ValidTask;
        }

        if (
            commandLineOptions.IsOptionSet(ReportGitHubSummaryOption)
            || commandLineOptions.IsOptionSet(ReportGitHubTitleOption)
            || commandLineOptions.IsOptionSet(ReportGitHubMessageOption)
        )
        {
            return ValidationResult.InvalidTask(
                "The options 'report-github-summary', 'report-github-title', and 'report-github-message' can only be used if 'report-github' is set."
            );
        }

        return ValidationResult.ValidTask;
    }

    // This method is called once per option declared and is used to validate the arguments of the given option.
    // The arity of the option is checked before this method is called.
    public Task<ValidationResult> ValidateOptionArgumentsAsync(
        CommandLineOption commandOption,
        string[] arguments
    )
    {
        if (commandOption.Name == ReportGitHubSummaryOption)
        {
            if (arguments.Length > 3)
            {
                return ValidationResult.InvalidTask(
                    $"The option '{ReportGitHubSummaryOption}' can have at most 3 arguments."
                );
            }

            if (arguments.Distinct().Count() != arguments.Length)
            {
                return ValidationResult.InvalidTask(
                    $"The option '{ReportGitHubSummaryOption}' cannot have duplicate arguments."
                );
            }

            for (var i = 0; i < arguments.Length; i++)
            {
                if (
                    arguments[i] != ReportGitHubSummaryArguments.IncludePassedTests
                    && arguments[i] != ReportGitHubSummaryArguments.IncludeSkippedTests
                    && arguments[i] != ReportGitHubSummaryArguments.IncludeNotFoundTests
                )
                {
                    return ValidationResult.InvalidTask(
                        $"The option '{ReportGitHubSummaryOption}' can only have the arguments '{ReportGitHubSummaryArguments.IncludePassedTests}', '{ReportGitHubSummaryArguments.IncludeSkippedTests}', and '{ReportGitHubSummaryArguments.IncludeNotFoundTests}'."
                    );
                }
            }
        }

        return ValidationResult.ValidTask;
    }
}
