using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.CommandLine;

namespace GitHubActionsTestLogger;

internal sealed class GitHubActionsCommandLineProvider : ICommandLineOptionsProvider
{
    public string Uid => nameof(GitHubActionsCommandLineProvider);

    public string Version => "1.0.0";

    public string DisplayName => "GitHub Actions Reporter";

    public string Description => "Reports test results to GitHub actions";

    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    public IReadOnlyCollection<CommandLineOption> GetCommandLineOptions() =>
        [
            new CommandLineOption(
                "report-github-actions",
                "Enables GitHubActions reporter.",
                ArgumentArity.Zero,
                isHidden: false
            ),
        ];

    public Task<ValidationResult> ValidateOptionArgumentsAsync(
        CommandLineOption commandOption,
        string[] arguments
    )
    {
        return ValidationResult.ValidTask;
    }

    public Task<ValidationResult> ValidateCommandLineOptionsAsync(
        ICommandLineOptions commandLineOptions
    ) => ValidationResult.ValidTask;
}
