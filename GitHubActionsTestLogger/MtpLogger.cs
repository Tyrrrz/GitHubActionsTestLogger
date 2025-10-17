using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GitHubActionsTestLogger.GitHub;
using GitHubActionsTestLogger.Reporting;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.Testing.Platform.CommandLine;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Microsoft.Testing.Platform.TestHost;

namespace GitHubActionsTestLogger;

internal class MtpLogger : IDataConsumer, ITestSessionLifetimeHandler
{
    private readonly bool _isEnabled;
    private readonly TestReportingContext _context;
    private readonly Stopwatch _stopwatch = new();

    private TestRunStartInfo? _testRunStartInfo;
    private List<TestResult> _testResults = [];

    public MtpLogger(ICommandLineOptions commandLineOptions)
    {
        var options = MtpLoggerOptionsProvider.Resolve(out var isEnabled, commandLineOptions);
        _isEnabled = isEnabled || GitHubEnvironment.IsRunningInActions;
        _context = new TestReportingContext(GitHubWorkflow.Default, options);
    }

    public string Uid => "GitHubActionsTestLogger";

    public string Version { get; } = typeof(MtpLogger).Assembly.TryGetVersionString() ?? "1.0.0";

    public string DisplayName => "GitHub Actions Test Logger";

    public string Description => "Reports test results to GitHub Actions";

    public Type[] DataTypesConsumed { get; } = [typeof(TestNodeUpdateMessage)];

    public Task<bool> IsEnabledAsync() => Task.FromResult(_isEnabled);

    public Task OnTestSessionStartingAsync(
        SessionUid sessionUid,
        CancellationToken cancellationToken
    )
    {
        _stopwatch.Restart();

        // MTP test host runs within the test assembly, so we can infer the test suite name
        // and target framework directly from the assembly metadata.
        var testAssembly = Assembly.GetEntryAssembly();

        var testRunStartInfo = new TestRunStartInfo(
            sessionUid.Value,
            testAssembly?.GetName().Name,
            testAssembly
                ?.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()
                ?.FrameworkName ?? RuntimeInformation.FrameworkDescription
        );

        _testRunStartInfo = testRunStartInfo;
        _testResults = [];

        _context.HandleTestRunStart(testRunStartInfo);

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

        var state =
            message.TestNode.Properties.SingleOrDefault<TestNodeStateProperty>()
            ?? throw new InvalidOperationException("Test node state property is missing.");

        var method =
            message.TestNode.Properties.SingleOrDefault<TestMethodIdentifierProperty>() ??
            // We assume that the test is always defined by a method, because MTP does not
            // appear to have a way to represent tests defined by other means.
            throw new InvalidOperationException("Test method identifier property is missing.");

        var location = message.TestNode.Properties.SingleOrDefault<TestFileLocationProperty>();
        var exception = state.TryGetException();

        var testDefinition = new TestDefinition(
            message.TestNode.Uid.Value,
            message.TestNode.DisplayName,
            new SymbolReference(
                method.MethodName,
                string.Join(".", method.Namespace, method.TypeName, method.MethodName)
            ),
            new SymbolReference(
                method.TypeName,
                string.Join(".", method.Namespace, method.TypeName)
            ),
            location?.FilePath,
            location?.LineSpan.Start.Line,
            message
                .TestNode.Properties.OfType<TestMetadataProperty>()
                .ToDictionary(p => p.Key, p => p.Value)
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
        _context.HandleTestResult(testResult);

        return Task.CompletedTask;
    }

    public Task OnTestSessionFinishingAsync(
        SessionUid sessionUid,
        CancellationToken cancellationToken
    )
    {
        if (_testRunStartInfo is null)
            throw new InvalidOperationException("Test run has not been started.");

        _stopwatch.Stop();

        var testRunStatistics = new TestRunEndInfo(
            _testRunStartInfo,
            _testResults,
            _stopwatch.Elapsed
        );

        _context.HandleTestRunEnd(testRunStatistics);

        return Task.CompletedTask;
    }
}
