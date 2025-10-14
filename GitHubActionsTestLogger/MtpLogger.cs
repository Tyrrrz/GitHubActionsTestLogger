using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

internal class MtpLogger(ICommandLineOptions commandLineOptions)
    : MtpExtensionBase,
        // This is the extension point to subscribe to data messages published to the platform.
        // The type should then be registered as a data consumer in the test host.
        IDataConsumer,
        // This is the extension point to subscribe to test session lifetime events.
        // The type should then be registered as a test session lifetime handler in the test host.
        ITestSessionLifetimeHandler
{
    private readonly TestReportingContext _context = new(
        GitHubWorkflow.Default,
        TestReportingOptions.Resolve(commandLineOptions)
    );

    // MTP does not provide a built-in way to measure test run duration, so we do it manually
    private readonly Stopwatch _stopwatch = new();

    private TestRunStartInfo? _testRunStartInfo;
    private List<TestResult> _testResults = [];

    public Type[] DataTypesConsumed { get; } = [typeof(TestNodeUpdateMessage)];

    public Task OnTestSessionStartingAsync(
        SessionUid sessionUid,
        CancellationToken cancellationToken
    )
    {
        _stopwatch.Restart();

        var testAssembly = Assembly.GetEntryAssembly();
        var testRunStartInfo = new TestRunStartInfo(
            sessionUid.Value,
            // MTP test host runs within the test assembly, so we can infer the test suite name
            // and target framework directly from the assembly metadata.
            testAssembly?.GetName().Name,
            testAssembly
                ?.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>()
                ?.FrameworkName
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
            throw new InvalidOperationException("The test run has not been started.");

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
