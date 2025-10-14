using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GitHubActionsTestLogger.Bridge;
using GitHubActionsTestLogger.Utils.Extensions;
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

    // MTP does not provide a built-in way to measure test run duration, so we do it manually
    private readonly Stopwatch _stopwatch = new();

    // MTP does not produce test run statistics at the end of the test session, so we build it
    // manually by collecting all test results.
    private List<TestResult> _testResults = [];

    public Type[] DataTypesConsumed { get; } = [typeof(TestNodeUpdateMessage)];

    public string Uid => extension.Uid;
    public string Version => extension.Version;
    public string DisplayName => extension.DisplayName;
    public string Description => extension.Description;

    public Task<bool> IsEnabledAsync() => extension.IsEnabledAsync();

    public Task OnTestSessionStartingAsync(
        SessionUid sessionUid,
        CancellationToken cancellationToken
    )
    {
        _stopwatch.Restart();
        _testResults = [];

        var testAssembly = Assembly.GetEntryAssembly();

        _context.HandleTestRunStart(
            new TestRunStartInfo(
                sessionUid.Value,
                // MTP test host runs within the test assembly, so we can infer the test suite name
                // and target framework directly from the assembly metadata.
                testAssembly?.GetName().Name,
                testAssembly
                    ?.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()
                    ?.FrameworkName
            )
        );

        return Task.CompletedTask;
    }

    public Task ConsumeAsync(
        IDataProducer dataProducer,
        IData value,
        CancellationToken cancellationToken
    )
    {
        if (value is not TestNodeUpdateMessage message)
        {
            throw new InvalidOperationException(
                $"Unexpected data type: {value.GetType().FullName}."
            );
        }

        var state = message.TestNode.Properties.SingleOrDefault<TestNodeStateProperty>();
        if (state is null)
        {
            throw new InvalidOperationException("Test node state property is missing.");
        }

        var exception = state.TryGetException();

        var testResult = new TestResult(
            new TestDefinition(
                message.TestNode.Uid.Value,
                message.TestNode.DisplayName,
                null, // TODO: SourceFilePath
                null, // TODO: SourceFileLineNumber
                message
                    .TestNode.Properties.OfType<TestMetadataProperty>()
                    .ToDictionary(p => p.Key, p => p.Value)
            ),
            state.ToTestOutcome(),
            state.Explanation ?? exception?.Message,
            exception?.StackTrace
        );

        _context.HandleTestResult(testResult);

        _testResults.Add(testResult);

        return Task.CompletedTask;
    }

    public Task OnTestSessionFinishingAsync(
        SessionUid sessionUid,
        CancellationToken cancellationToken
    )
    {
        _stopwatch.Stop();

        var testRunStatistics = new TestRunStatistics(
            _testResults.Count(r => r.Outcome == TestOutcome.Passed),
            _testResults.Count(r => r.Outcome == TestOutcome.Failed),
            _testResults.Count(r => r.Outcome == TestOutcome.Skipped),
            _testResults.Count,
            _stopwatch.Elapsed
        );

        _context.HandleTestRunEnd(testRunStatistics);

        return Task.CompletedTask;
    }
}
