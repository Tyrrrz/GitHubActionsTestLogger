using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger;

[FriendlyName("GitHubActions")]
[ExtensionUri("logger://tyrrrz/ghactions/v2")]
public class TestLogger : ITestLoggerWithParameters
{
    public TestLoggerContext? Context { get; private set; }

    private void Initialize(TestLoggerEvents events, TestLoggerOptions options)
    {
        var github = new GitHubWorkflow();

        if (!github.IsRunningOnAgent)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Warning: using GitHub Actions Test Logger, but not running on GitHub Actions.");
            Console.ResetColor();
        }

        var context = new TestLoggerContext(github, options);

        events.TestRunStart += (_, args) => context.HandleTestRunStart(args.TestRunCriteria);
        events.TestResult += (_, args) => context.HandleTestResult(args.Result);
        events.TestRunComplete += (_, args) => context.HandleTestRunComplete(
            args.TestRunStatistics,
            args.ElapsedTimeInRunningTests
        );

        Context = context;
    }

    public void Initialize(TestLoggerEvents events, string testRunDirectory) =>
        Initialize(events, TestLoggerOptions.Default);

    public void Initialize(TestLoggerEvents events, Dictionary<string, string?> parameters) =>
        Initialize(events, TestLoggerOptions.Resolve(parameters));
}