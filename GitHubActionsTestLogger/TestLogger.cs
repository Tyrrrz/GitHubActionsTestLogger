using System;
using System.Collections.Generic;
using GitHubActionsTestLogger.Internal;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;

namespace GitHubActionsTestLogger
{
    // The main idea behind this logger is that it writes messages to console in a special format
    // that GitHub Actions runner recognizes as workflow commands.
    // We try to get the source information (file, line number) of the failed tests and
    // report them to GitHub Actions so it highlights them as failed checks in diff and annotations.

    // The problem is that .NET doesn't provide source information, even though the contract implies
    // that it's supposed to. That's why we're employing some additional workarounds to get it if possible.

    [FriendlyName("GitHubActions")]
    [ExtensionUri("logger://tyrrrz/ghactions/v1")]
    public partial class TestLogger : ITestLoggerWithParameters
    {
        public TestLoggerContext? Context { get; private set; }

        public void Initialize(TestLoggerEvents events, string testRunDirectory)
        {
            CheckWarnGitHubActionsContext();

            Context = new TestLoggerContext(Console.Out, TestLoggerOptions.Default);

            events.TestResult += (sender, args) => Context.ProcessTestResult(args.Result);
        }

        public void Initialize(TestLoggerEvents events, Dictionary<string, string> parameters)
        {
            CheckWarnGitHubActionsContext();

            var options = TestLoggerOptions.Extract(parameters);
            Context = new TestLoggerContext(Console.Out, options);

            events.TestResult += (sender, args) => Context.ProcessTestResult(args.Result);
        }
    }

    public partial class TestLogger
    {
        private static void CheckWarnGitHubActionsContext()
        {
            if (!GitHubActions.IsRunningInsideWorkflow())
                Console.WriteLine("WARN: Not running inside GitHub Actions, but using GitHub Actions Test Logger.");
        }
    }
}