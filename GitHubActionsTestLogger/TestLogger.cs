using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger;

[FriendlyName("GitHubActions")]
[ExtensionUri("logger://tyrrrz/ghactions/v1")]
public class TestLogger : ITestLoggerWithParameters, IDisposable
{
    public TestLoggerContext? Context { get; private set; }

    private void Initialize(TestLoggerEvents events, TestLoggerOptions options)
    {
        if (!GitHubEnvironment.IsActions)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Warning: Using GitHub Actions Test Logger, but not running on GitHub Actions.");
            Console.ResetColor();
        }

        Context?.Dispose();
        Context = new TestLoggerContext(
            Console.Out,
            GitHubEnvironment.ActionSummaryFilePath,
            options
        );

        events.TestRunStart += (_, args) => Context.HandleTestRunStart(args);
        events.TestResult += (_, args) => Context.HandleTestResult(args);

        // TestRunComplete event is very inconsistent and often doesn't trigger at all, so
        // we can't rely on it for things like cleanup or finalizing the job summary.
        // https://github.com/microsoft/vstest/issues/3121
    }

    public void Initialize(TestLoggerEvents events, string testRunDirectory) =>
        Initialize(events, TestLoggerOptions.Default);

    public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters) =>
        Initialize(events, TestLoggerOptions.Resolve(parameters));

    public void Dispose()
    {
        Context?.Dispose();
    }
}