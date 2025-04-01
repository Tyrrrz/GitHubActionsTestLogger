using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Microsoft.Testing.Platform.TestHost;

namespace GitHubActionsTestLogger;

/// <summary>
/// A Microsoft.Testing.Platform extension that reports test run information to GitHub Actions.
/// </summary>
internal sealed class GitHubTestReporter(GitHubTestReporterExtension extension, ICommandLineOptions commandLineOptions) : IDataConsumer, ITestSessionLifetimeHandler
{
    private readonly TestReporterContext _context = new(GitHubWorkflow.Default, TestReporterOptions.Resolve(commandLineOptions));

    public Type[] DataTypesConsumed { get; } =
        [
            typeof(TestNodeUpdateMessage),
        ];

    public string Uid => extension.Uid;
    public string Version => extension.Version;
    public string DisplayName => extension.DisplayName;
    public string Description => extension.Description;

    public Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
    {
        _context.HandleTestResult((TestNodeUpdateMessage)value);
        return Task.CompletedTask;
    }

    public Task<bool> IsEnabledAsync() => extension.IsEnabledAsync();

    public Task OnTestSessionFinishingAsync(SessionUid sessionUid, CancellationToken cancellationToken)
    {
        _context.HandleTestRunComplete();
        return Task.CompletedTask;
    }

    public Task OnTestSessionStartingAsync(SessionUid sessionUid, CancellationToken cancellationToken)
        => Task.CompletedTask;
}