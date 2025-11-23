using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GitHubActionsTestLogger.GitHub;
using GitHubActionsTestLogger.Reporting;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Microsoft.Testing.Platform.Services;

namespace GitHubActionsTestLogger;

internal class MtpLogger : IDataConsumer, ITestSessionLifetimeHandler
{
    private readonly bool _isEnabled;
    private readonly TestReportingContext _context;
    private readonly Stopwatch _stopwatch = new();

    private TestRunStartInfo? _testRunStartInfo;
    private List<TestResult> _testResults = [];

    public MtpLogger(GitHubWorkflow gitHubWorkflow, ICommandLineOptions commandLineOptions)
    {
        var options = MtpLoggerOptionsProvider.Resolve(out var isEnabled, commandLineOptions);
        _isEnabled = isEnabled ?? GitHubEnvironment.IsRunningInActions;
        _context = new TestReportingContext(gitHubWorkflow, options);
    }

    public string Uid => "GitHubActionsTestLogger";

    public string Version { get; } = typeof(MtpLogger).Assembly.TryGetVersionString() ?? "1.0.0";

    public string DisplayName => Uid;

    public string Description => "Reports test results to GitHub Actions.";

    public Type[] DataTypesConsumed { get; } = [typeof(TestNodeUpdateMessage)];

    public Task<bool> IsEnabledAsync() => Task.FromResult(_isEnabled);

    public async Task OnTestSessionStartingAsync(ITestSessionContext context)
    {
        _stopwatch.Restart();

        var testRunStartInfo = new TestRunStartInfo(
            context.SessionUid.Value,
            // MTP test host runs within the test assembly, so we can infer the test suite name
            // and target framework directly from the assembly metadata.
            Assembly.GetEntryAssembly()?.GetName().Name,
            AppContext.TargetFrameworkName ?? RuntimeInformation.FrameworkDescription
        );

        _testRunStartInfo = testRunStartInfo;
        _testResults = [];

        await _context.HandleTestRunStartAsync(testRunStartInfo, context.CancellationToken);
    }

    public async Task ConsumeAsync(
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

        var state =
            message.TestNode.Properties.SingleOrDefault<TestNodeStateProperty>()
            ?? throw new InvalidOperationException("Test node state property is missing.");

        // Only consider updates related to tests that have concluded in some final state
        if (state is DiscoveredTestNodeStateProperty or InProgressTestNodeStateProperty)
            return;

        var exception = state.TryGetException();

        var testDefinition = new TestDefinition(
            message.TestNode.Uid.Value,
            message.TestNode.DisplayName,
            new SymbolReference(
                message.TestNode.TryGetMinimallyQualifiedName() ?? message.TestNode.DisplayName,
                message.TestNode.TryGetFullyQualifiedName() ?? message.TestNode.DisplayName
            ),
            new SymbolReference(
                message.TestNode.TryGetTypeMinimallyQualifiedName() ?? "<>",
                message.TestNode.TryGetTypeFullyQualifiedName() ?? "<>"
            ),
            message.TestNode.TryGetSourceFilePath(),
            message.TestNode.TryGetSourceLine()
        );

        var testResult = new TestResult(
            testDefinition,
            state switch
            {
                PassedTestNodeStateProperty => TestOutcome.Passed,
                FailedTestNodeStateProperty => TestOutcome.Failed,
                ErrorTestNodeStateProperty => TestOutcome.Failed,
                TimeoutTestNodeStateProperty => TestOutcome.Failed,
                SkippedTestNodeStateProperty => TestOutcome.Skipped,
                CancelledTestNodeStateProperty => TestOutcome.Skipped,
                _ => TestOutcome.None,
            },
            state.Explanation ?? exception?.Message,
            exception?.StackTrace
        );

        _testResults.Add(testResult);
        await _context.HandleTestResultAsync(testResult, cancellationToken);
    }

    public async Task OnTestSessionFinishingAsync(ITestSessionContext context)
    {
        if (_testRunStartInfo is null)
            throw new InvalidOperationException("Test run has not been started.");

        _stopwatch.Stop();

        var testRunStatistics = new TestRunEndInfo(
            _testRunStartInfo,
            _testResults,
            _stopwatch.Elapsed
        );

        await _context.HandleTestRunEndAsync(testRunStatistics, context.CancellationToken);
    }
}
