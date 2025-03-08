using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Extensions.OutputDevice;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Microsoft.Testing.Platform.TestHost;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace GitHubActionsTestLogger;

internal sealed class GitHubActionsReporterDataConsumer
    : IDataConsumer,
        ITestSessionLifetimeHandler,
        IOutputDeviceDataProducer
{
    private IServiceProvider _serviceProvider;
    private int _passedTests;
    private int _failedTests;
    private int _skippedTests;
    private readonly TestLoggerContext _context;

    public GitHubActionsReporterDataConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _context = new TestLoggerContext(GitHubWorkflow.Default, new TestLoggerOptions()) { };
    }

    public Type[] DataTypesConsumed => [typeof(TestNodeUpdateMessage)];

    public string Uid => nameof(GitHubActionsReporterDataConsumer);

    public string Version => "1.0.0";

    public string DisplayName => "GitHub Actions Reporter";

    public string Description => "Reports test results to GitHub actions";

    public Task ConsumeAsync(
        IDataProducer dataProducer,
        IData value,
        CancellationToken cancellationToken
    )
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        try
        {
            TestNodeUpdateMessage testNodeUpdate = (TestNodeUpdateMessage)value;
            var testResult = MTPConverter.ConvertTestNodeUpdateMessage(testNodeUpdate);
            switch (testResult.Outcome)
            {
                case LoggerTestOutcome.None:
                case LoggerTestOutcome.NotFound:
                    throw new InvalidOperationException();
                case LoggerTestOutcome.Passed:
                    Interlocked.Increment(ref _passedTests);
                    break;
                case LoggerTestOutcome.Failed:
                    Interlocked.Increment(ref _failedTests);
                    break;
                case LoggerTestOutcome.Skipped:
                    Interlocked.Increment(ref _skippedTests);
                    break;
            }
            _context.HandleTestResult(testResult);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            // Do nothing, we're stopping
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsEnabledAsync()
    {
        return Task.FromResult(true);
    }

    public Task OnTestSessionFinishingAsync(
        SessionUid sessionUid,
        CancellationToken cancellationToken
    )
    {
        return Task.CompletedTask;
    }

    public Task OnTestSessionStartingAsync(
        SessionUid sessionUid,
        CancellationToken cancellationToken
    )
    {
        var assembly = Assembly.GetCallingAssembly().Location;
        var runtimeFramework = TargetFrameworkParser.GetShortTargetFramework(RuntimeInformation.FrameworkDescription);
        var targetFramework = TargetFrameworkParser.GetShortTargetFramework(Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkDisplayName) ?? runtimeFramework;
        var testRunCriteria = new LoggerTestRunCriteria
        (
            targetFramework,
             [assembly]
           

        );
        _context.HandleTestRunStart(testRunCriteria);
        return Task.CompletedTask;
    }
}

// borrowed from testfx (is MIT, but would need attribution)
internal static class TargetFrameworkParser
{
    public static string? GetShortTargetFramework(string? frameworkDescription)
    {
        if (frameworkDescription == null)
        {
            return null;
        }

        // https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.runtimeinformation.frameworkdescription
        string netFramework = ".NET Framework";
        if (frameworkDescription.StartsWith(netFramework, ignoreCase: false, CultureInfo.InvariantCulture))
        {
            // .NET Framework 4.7.2
            if (frameworkDescription.Length < (netFramework.Length + 6))
            {
                return frameworkDescription;
            }

            char major = frameworkDescription[netFramework.Length + 1];
            char minor = frameworkDescription[netFramework.Length + 3];
            char patch = frameworkDescription[netFramework.Length + 5];

            if (major == '4' && minor == '6' && patch == '2')
            {
                return "net462";
            }
            else if (major == '4' && minor == '7' && patch == '1')
            {
                return "net471";
            }
            else if (major == '4' && minor == '7' && patch == '2')
            {
                return "net472";
            }
            else if (major == '4' && minor == '8' && patch == '1')
            {
                return "net481";
            }
            else
            {
                // Just return the first 2 numbers.
                return $"net{major}{minor}";
            }
        }

        string netCore = ".NET Core";
        if (frameworkDescription.StartsWith(netCore, ignoreCase: false, CultureInfo.InvariantCulture))
        {
            // .NET Core 3.1
            return frameworkDescription.Length >= (netCore.Length + 4)
                ? $"netcoreapp{frameworkDescription[netCore.Length + 1]}.{frameworkDescription[netCore.Length + 3]}"
                : frameworkDescription;
        }

        string net = ".NET";
        if (frameworkDescription.StartsWith(net, ignoreCase: false, CultureInfo.InvariantCulture))
        {
            int firstDotInVersion = frameworkDescription.IndexOf('.', net.Length + 1);
            return firstDotInVersion < 1
                ? frameworkDescription
                : $"net{frameworkDescription.Substring(net.Length + 1, firstDotInVersion - net.Length - 1)}.{frameworkDescription[firstDotInVersion + 1]}";
        }

        return frameworkDescription;
    }