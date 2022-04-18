using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger;

[FriendlyName("GitHubActions")]
[ExtensionUri("logger://tyrrrz/ghactions/v1")]
public class TestLogger : ITestLoggerWithParameters
{
    public TestLoggerContext? Context { get; private set; }

    private void Initialize(TestLoggerEvents events, TestLoggerOptions options)
    {
        var isRunningOnGitHub = string.Equals(
            Environment.GetEnvironmentVariable("GITHUB_ACTIONS"),
            "true",
            StringComparison.OrdinalIgnoreCase
        );

        if (!isRunningOnGitHub)
            Console.WriteLine("WARN: Not running inside GitHub Actions, but using GitHub Actions Test Logger.");

        Context = new TestLoggerContext(Console.Out, options);

        events.TestRunStart += (_, args) => Context.HandleTestRunStart(args.TestRunCriteria);
        events.TestResult += (_, args) => Context.HandleTestResult(args.Result);
        events.TestRunComplete += (_, _) => Context.HandleTestRunComplete();
    }

    public void Initialize(TestLoggerEvents events, string testRunDirectory) =>
        Initialize(events, TestLoggerOptions.Default);

    public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters) =>
        Initialize(events, TestLoggerOptions.Resolve(parameters));
}