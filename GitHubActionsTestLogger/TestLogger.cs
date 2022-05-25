using System;
using System.Collections.Generic;
using System.IO;
using GitHubActionsTestLogger.Utils;
using GitHubActionsTestLogger.Utils.Extensions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger;

[FriendlyName("GitHubActions")]
[ExtensionUri("logger://tyrrrz/ghactions/v2")]
public class TestLogger : ITestLoggerWithParameters
{
    public static bool IsGood { get; set; } = false;

    public TestLoggerContext? Context { get; private set; }

    private void Initialize(TestLoggerEvents events, TestLoggerOptions options)
    {
        IsGood = true;

        if (!GitHubWorkflow.IsRunningOnAgent)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Warning: using GitHub Actions Test Logger, but not running on GitHub Actions.");
            Console.ResetColor();
        }

        // Commands are written to the standard output
        var commandWriter = Console.Out;

        // Summary is written to the file specified by an environment variable.
        // We may need to write to the summary file from multiple test suites in parallel, so we should:
        // 1. Delay acquiring the file lock until the very last moment
        // 2. Employ retry logic to handle potential race conditions
        var summaryWriter =
            GitHubWorkflow
                .SummaryFilePath?
                .Pipe(f => new ContentionTolerantWriteFileStream(f, FileMode.Append))
                .Pipe(s => new StreamWriter(s)) ??
            TextWriter.Null;

        var github = new GitHubWorkflow(commandWriter, summaryWriter);
        var context = new TestLoggerContext(github, options);

        events.TestRunStart += (_, args) => context.HandleTestRunStart(args);
        events.TestResult += (_, args) => context.HandleTestResult(args);
        events.TestRunComplete += (_, args) => context.HandleTestRunComplete(args);

        Context = context;
    }

    public void Initialize(TestLoggerEvents events, string testRunDirectory) =>
        Initialize(events, TestLoggerOptions.Default);

    public void Initialize(TestLoggerEvents events, Dictionary<string, string?> parameters) =>
        Initialize(events, TestLoggerOptions.Resolve(parameters));
}