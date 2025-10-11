using System;
using System.Linq;
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
internal class TestReporter(TestReporterExtension extension, ICommandLineOptions commandLineOptions)
    :
    // This is the extension point to subscribe to data messages published to the platform.
    // The type should then be registered as a data consumer in the test host.
    IDataConsumer,
        // This is the extension point to subscribe to test session lifetime events.
        // The type should then be registered as a test session lifetime handler in the test host.
        ITestSessionLifetimeHandler
{
    private readonly TestReporterContext _context = new(
        GitHubWorkflow.Default,
        TestReporterOptions.Resolve(commandLineOptions)
    );

    public Type[] DataTypesConsumed { get; } = [typeof(TestNodeUpdateMessage)];

    public string Uid => extension.Uid;
    public string Version => extension.Version;
    public string DisplayName => extension.DisplayName;
    public string Description => extension.Description;

    public Task<bool> IsEnabledAsync() => extension.IsEnabledAsync();

    public Task ConsumeAsync(
        IDataProducer dataProducer,
        IData value,
        CancellationToken cancellationToken
    )
    {
        if (value is not TestNodeUpdateMessage message)
            throw new InvalidOperationException(
                $"Unexpected data type: {value.GetType().FullName}"
            );

        var state = message.TestNode.Properties.SingleOrDefault<TestNodeStateProperty>();
        if (state is null)
            throw new InvalidOperationException("Test node state property is missing.");

        _context.HandleTestResult(
            new TestResult(
                new TestDefinition(
                    message.TestNode.Uid.Value,
                    message.TestNode.DisplayName,
                    message
                        .TestNode.Properties.OfType<TestMetadataProperty>()
                        .ToDictionary(p => p.Key, p => p.Value)
                ),
                state.ToTestOutcome(),
                state switch
                {
                    FailedTestNodeStateProperty failedState => failedState.Exception,
                    ErrorTestNodeStateProperty errorState => errorState.Exception,
                    TimeoutTestNodeStateProperty timeoutState => timeoutState.Exception,
                    _ => null,
                }
            )
        );

        return Task.CompletedTask;
    }

    public Task OnTestSessionStartingAsync(
        SessionUid sessionUid,
        CancellationToken cancellationToken
    ) => Task.CompletedTask;

    public Task OnTestSessionFinishingAsync(
        SessionUid sessionUid,
        CancellationToken cancellationToken
    )
    {
        _context.HandleTestRunComplete();
        return Task.CompletedTask;
    }
}
